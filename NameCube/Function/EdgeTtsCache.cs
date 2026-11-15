// /*
//  * NameCube-<点名器>
//  * Copyright (C) 2025-2026 haaa4
//  *
//  * This program is free software: you can redistribute it and/or modify
//  * it under the terms of the GNU General Public License as published by
//  * the Free Software Foundation, either version 3 of the License, or
//  * (at your option) any later version.
//  *
//  * This program is distributed in the hope that it will be useful,
//  * but WITHOUT ANY WARRANTY

using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NameCube.Function
{
    /// <summary>
    /// Edge 朗读音频的本地缓存：把合成过的 MP3 按“音色 + 语速 + 文本”存到 user\tts-cache，
    /// 下次同一句话直接读文件，不用再联网（也不需要等一两秒的合成时间）。
    /// </summary>
    /// <remarks>
    /// 文件名是 SHA-256（音色|语速|文本）的十六进制，天然去重、不用处理非法字符；
    /// 旁边的 index.json 记住每个文件对应哪句话，用来做统计和“清理名单外音频”。
    /// 缓存坏了/被删了都不要紧：读不到就重新联网合成，功能自动恢复。
    /// </remarks>
    internal static class EdgeTtsCache
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(EdgeTtsCache));

        // 索引与文件操作都可能来自后台预加载线程，统一用一把锁串行化
        private static readonly object Gate = new object();

        private static Dictionary<string, CacheEntry> _index;
        private static bool _loaded;

        /// <summary>缓存文件所在文件夹（user\tts-cache）。</summary>
        public static string DirectoryPath =>
            Path.Combine(GlobalVariablesData.userDataDir, "tts-cache");

        private static string IndexPath => Path.Combine(DirectoryPath, "index.json");

        /// <summary>缓存总开关：关掉后既不读也不写，但磁盘上已有的文件不会动。</summary>
        public static bool Enabled => GlobalVariablesData.config.AllSettings.TtsCacheEnabled ?? true;

        /// <summary>一条缓存记录。</summary>
        private sealed class CacheEntry
        {
            public string Voice { get; set; }
            public int Rate { get; set; }

            /// <summary>这条缓存对应的原文（旧版本遗留/索引丢失时为 null）。</summary>
            public string Text { get; set; }

            public long Length { get; set; }
            public DateTime Created { get; set; }
        }

        /// <summary>试着从磁盘取一段音频。命中返回 true。</summary>
        public static bool TryRead(string voice, int rate, string text, out byte[] audio)
        {
            audio = null;
            if (!Enabled || string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            lock (Gate)
            {
                try
                {
                    string path = GetFilePath(voice, rate, text);
                    if (!File.Exists(path))
                    {
                        return false;
                    }

                    byte[] bytes = File.ReadAllBytes(path);
                    if (bytes.Length == 0)
                    {
                        return false;
                    }

                    // 索引丢了也照样能用：补一条只剩长度的记录，方便统计
                    EnsureEntry(Key(voice, rate, text), voice, rate, text, bytes.Length);
                    audio = bytes;
                    _logger.Debug("Edge 朗读命中本地缓存（{Bytes} 字节）", bytes.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    // 缓存读失败不该影响正常朗读：当作没命中，继续联网合成
                    _logger.Debug(ex, "读取 Edge 朗读缓存失败，已忽略");
                    return false;
                }
            }
        }

        /// <summary>把新合成的音频写进缓存。失败只记日志，不打断朗读。</summary>
        public static void Write(string voice, int rate, string text, byte[] audio)
        {
            if (!Enabled || audio == null || audio.Length == 0 || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            lock (Gate)
            {
                try
                {
                    Directory.CreateDirectory(DirectoryPath);
                    string path = GetFilePath(voice, rate, text);
                    if (!File.Exists(path))
                    {
                        // 先写临时文件再改名，避免中途崩溃留下半截 MP3
                        string temp = path + ".tmp";
                        File.WriteAllBytes(temp, audio);
                        File.Move(temp, path, overwrite: true);
                    }

                    EnsureEntry(Key(voice, rate, text), voice, rate, text, audio.Length);
                    SaveIndex();
                    _logger.Debug("Edge 朗读音频已缓存（{Bytes} 字节）", audio.Length);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "写入 Edge 朗读缓存失败，本次朗读不受影响");
                }
            }
        }

        /// <summary>判断某段文本是否已经缓存（预加载时用来跳过）。</summary>
        public static bool Contains(string voice, int rate, string text)
        {
            if (!Enabled || string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            lock (Gate)
            {
                try
                {
                    string key = Key(voice, rate, text);
                    return File.Exists(GetFilePath(voice, rate, text)) && LoadIndex().ContainsKey(key);
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "查询 Edge 朗读缓存失败，已忽略");
                    return false;
                }
            }
        }

        /// <summary>缓存统计：条数与占用字节数。</summary>
        public static (int Count, long Bytes) GetStats()
        {
            lock (Gate)
            {
                try
                {
                    if (!Directory.Exists(DirectoryPath))
                    {
                        return (0, 0);
                    }

                    var files = new DirectoryInfo(DirectoryPath).GetFiles("*.mp3");
                    return (files.Length, files.Sum(f => f.Length));
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "统计 Edge 朗读缓存失败，已忽略");
                    return (0, 0);
                }
            }
        }

        /// <summary>删除全部缓存，返回删掉的音频文件数。</summary>
        public static int RemoveAll()
        {
            lock (Gate)
            {
                int count = 0;
                try
                {
                    if (Directory.Exists(DirectoryPath))
                    {
                        foreach (string file in Directory.GetFiles(DirectoryPath, "*.mp3"))
                        {
                            TryDelete(file, ref count);
                        }

                        foreach (string file in Directory.GetFiles(DirectoryPath, "*.tmp"))
                        {
                            TryDelete(file, ref count, countAsAudio: false);
                        }

                        if (File.Exists(IndexPath))
                        {
                            File.Delete(IndexPath);
                        }
                    }

                    _index = new Dictionary<string, CacheEntry>();
                    _loaded = true;
                    _logger.Information("已清空 Edge 朗读缓存，共删除 {Count} 个音频文件", count);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "清空 Edge 朗读缓存时发生异常");
                }

                return count;
            }
        }

        /// <summary>
        /// 删除“不属于给定文本集合”的缓存（例如名单换人之后，旧名字的音频就没用了）。
        /// 索引里没有原文的旧文件会保留，避免误删。
        /// </summary>
        public static int RemoveTextsNotIn(IEnumerable<string> keepTexts)
        {
            var keep = new HashSet<string>(keepTexts ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
            lock (Gate)
            {
                int count = 0;
                try
                {
                    Dictionary<string, CacheEntry> index = LoadIndex();
                    foreach (var pair in index.ToList())
                    {
                        CacheEntry entry = pair.Value;
                        if (entry.Text == null || keep.Contains(entry.Text))
                        {
                            continue;
                        }

                        if (TryDelete(GetFilePath(entry.Voice, entry.Rate, entry.Text), ref count))
                        {
                            index.Remove(pair.Key);
                        }
                    }

                    SaveIndex();
                    _logger.Information("已清理名单外的 Edge 朗读缓存，共删除 {Count} 个音频文件", count);
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "清理名单外的 Edge 朗读缓存时发生异常");
                }

                return count;
            }
        }

        // ---------------------------------------------------------------- 内部实现

        private static bool TryDelete(string path, ref int count, bool countAsAudio = true)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }

                File.Delete(path);
                if (countAsAudio)
                {
                    count++;
                }

                return true;
            }
            catch (Exception ex)
            {
                // 文件被占用（比如正在播放）时删不掉，跳过即可
                _logger.Debug(ex, "删除缓存文件 {Path} 失败，已跳过", path);
                return false;
            }
        }

        private static string Key(string voice, int rate, string text)
        {
            string raw = $"{voice}|{rate}|{text}";
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
        }

        private static string GetFilePath(string voice, int rate, string text)
        {
            return Path.Combine(DirectoryPath, Key(voice, rate, text) + ".mp3");
        }

        private static void EnsureEntry(string key, string voice, int rate, string text, long length)
        {
            Dictionary<string, CacheEntry> index = LoadIndex();
            if (index.TryGetValue(key, out CacheEntry existing))
            {
                if (existing.Text == null)
                {
                    // 之前是从文件名重建的残缺记录，这里补全
                    existing.Text = text;
                    existing.Voice = voice;
                    existing.Rate = rate;
                }

                existing.Length = length;
                return;
            }

            index[key] = new CacheEntry
            {
                Voice = voice,
                Rate = rate,
                Text = text,
                Length = length,
                Created = DateTime.Now,
            };
        }

        private static Dictionary<string, CacheEntry> LoadIndex()
        {
            if (_loaded && _index != null)
            {
                return _index;
            }

            _loaded = true;
            _index = new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (File.Exists(IndexPath))
                {
                    string json = File.ReadAllText(IndexPath);
                    Dictionary<string, CacheEntry> saved =
                        JsonConvert.DeserializeObject<Dictionary<string, CacheEntry>>(json);
                    if (saved != null)
                    {
                        _index = new Dictionary<string, CacheEntry>(saved, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
            catch (Exception ex)
            {
                // 索引损坏就当没有：文件还在，只是无法按文本清理
                _logger.Warning(ex, "Edge 朗读缓存索引损坏，已重建");
                _index = new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);
            }

            // 把磁盘上存在、索引里没有的文件补进来（只记长度，不知道原文）
            try
            {
                if (Directory.Exists(DirectoryPath))
                {
                    foreach (FileInfo file in new DirectoryInfo(DirectoryPath).GetFiles("*.mp3"))
                    {
                        string key = Path.GetFileNameWithoutExtension(file.Name);
                        if (!_index.ContainsKey(key))
                        {
                            _index[key] = new CacheEntry
                            {
                                Length = file.Length,
                                Created = file.LastWriteTime,
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "扫描 Edge 朗读缓存目录失败，已忽略");
            }

            return _index;
        }

        private static void SaveIndex()
        {
            try
            {
                Directory.CreateDirectory(DirectoryPath);
                string json = JsonConvert.SerializeObject(_index, Formatting.Indented);
                string temp = IndexPath + ".tmp";
                File.WriteAllText(temp, json, new UTF8Encoding(false));
                File.Move(temp, IndexPath, overwrite: true);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "保存 Edge 朗读缓存索引失败");
            }
        }
    }
}
