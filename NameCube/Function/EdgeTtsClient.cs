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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using Serilog;

namespace NameCube.Function
{
    /// <summary>
    /// 微软 Edge 在线朗读（Edge TTS）客户端：把一段文字合成成 MP3，并负责播放。
    /// </summary>
    /// <remarks>
    /// 协议本身是 “WebSocket + 两次文本帧 + 一堆二进制音频帧”：
    /// 先连上 wss 端点（query 里带鉴权令牌与连接 id），发一条 speech.config 说清输出格式，
    /// 再发一条 SSML 描述要读什么、用什么音色，之后服务端就开始推二进制帧，直到收到 Path:turn.end。
    /// 鉴权令牌见 <see cref="EdgeTtsToken"/>。
    ///
    /// 该功能依赖网络，所以所有对外方法都按“失败就返回错误/静默跳过”处理，
    /// 不让网络波动把点名流程带崩。
    /// </remarks>
    internal static class EdgeTtsClient
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(EdgeTtsClient));

        // 两个端点是等价的：新版在 api.msedgeservices.com，老版在 speech.platform.bing.com。
        // 参数名不同（新端点用订阅键参数名，老端点用 TrustedClientToken），所以分开描述。
        private static readonly (string Name, string Url, string KeyParameter)[] Endpoints =
        {
            ("msedgeservices", "wss://api.msedgeservices.com/tts/cognitiveservices/websocket/v1",
                "Ocp-Apim-Subscription-Key"),
            ("bing", "wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1",
                "TrustedClientToken"),
        };

        /// <summary>音频输出格式：24kHz / 48kbps 单声道 MP3，与官方“朗读”功能一致。</summary>
        private const string OutputFormat = "audio-24khz-48kbitrate-mono-mp3";

        private const string SpeechConfig =
            "{\"context\":{\"synthesis\":{\"audio\":{\"metadataoptions\":{\"sentenceBoundaryEnabled\":false," +
            "\"wordBoundaryEnabled\":true},\"outputFormat\":\"" + OutputFormat + "\"}}}}";

        private const string UserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/143.0.0.0 Safari/537.36 Edg/143.0.0.0";

        private const string Origin = "chrome-extension://jdiccldimpdaibmpdkjnbmckianbfold";

        /// <summary>连接超时：握手不该超过这个时间，否则说明网络有问题。</summary>
        private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(15);

        /// <summary>整个合成过程的超时：防止连接成功后服务端不吭声、一直挂着。</summary>
        private static readonly TimeSpan SynthesisTimeout = TimeSpan.FromSeconds(30);

        /// <summary>最近合成过的音频缓存，避免同一个名字反复联网（键里含音色与语速）。</summary>
        private const int CacheCapacity = 32;

        private static readonly object CacheGate = new object();
        private static readonly Dictionary<string, byte[]> Cache = new Dictionary<string, byte[]>();
        private static readonly Queue<string> CacheOrder = new Queue<string>();

        /// <summary>
        /// 合成一段文字，返回 MP3 字节。失败抛 <see cref="EdgeTtsException"/>。
        /// </summary>
        /// <param name="text">要朗读的文本（会被做 XML 转义）。</param>
        /// <param name="voice">音色短名，如 zh-CN-XiaoxiaoNeural。</param>
        /// <param name="ratePercent">语速，-100 ~ 100 的百分比。</param>
        public static async Task<byte[]> SynthesizeAsync(
            string text, string voice, int ratePercent, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new EdgeTtsException("朗读文本为空");
            }

            string cacheKey = $"{voice}|{ratePercent}|{text}";
            if (TryGetCached(cacheKey, out byte[] cached))
            {
                return cached;
            }

            // 本地缓存优先：命中就不用联网，也不用等合成，点名时几乎是瞬间出声
            if (EdgeTtsCache.TryRead(voice, ratePercent, text, out byte[] fromDisk))
            {
                PutCache(cacheKey, fromDisk);
                return fromDisk;
            }

            var errors = new List<string>();
            foreach (var endpoint in Endpoints)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    byte[] audio = await SynthesizeWithEndpointAsync(
                        endpoint.Url, endpoint.KeyParameter, text, voice, ratePercent, cancellationToken)
                        .ConfigureAwait(false);
                    PutCache(cacheKey, audio);
                    // 顺手落盘，下次同一句话（哪怕换了进程）都能直接播放
                    EdgeTtsCache.Write(voice, ratePercent, text, audio);
                    return audio;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // 端点挂了就换下一个；两个都挂才把错误抛给上层
                    errors.Add($"{endpoint.Name}: {ex.Message}");
                    _logger.Debug(ex, "Edge TTS 端点 {Endpoint} 合成失败，尝试下一个", endpoint.Name);
                }
            }

            throw new EdgeTtsException("Edge TTS 合成失败：" + string.Join("；", errors));
        }

        /// <summary>
        /// 批量预加载：把一组文本全部合成好并写进本地缓存，已经缓存过的会直接跳过。
        /// 用 3 个并发连接，既比逐条串行快，又不至于被服务端限流。
        /// </summary>
        /// <param name="texts">要预加载的文本（通常是名单里的名字）。</param>
        /// <param name="voice">音色短名。</param>
        /// <param name="ratePercent">语速百分比。</param>
        /// <param name="progress">进度回调：(已完成, 总数)。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>成功数、失败数、总数。</returns>
        public static async Task<(int Succeeded, int Failed, int Total)> PreloadAsync(
            IEnumerable<string> texts,
            string voice,
            int ratePercent,
            IProgress<(int Done, int Total)> progress,
            CancellationToken cancellationToken)
        {
            // 去重但保持原顺序；空白项直接忽略
            var unique = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string raw in texts ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(raw) || !seen.Add(raw))
                {
                    continue;
                }

                unique.Add(raw);
            }

            int total = unique.Count;
            if (total == 0)
            {
                progress?.Report((0, 0));
                return (0, 0, 0);
            }

            int succeeded = 0;
            int failed = 0;
            int done = 0;

            var options = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 3,
            };

            await Parallel.ForEachAsync(unique, options, async (text, token) =>
            {
                try
                {
                    if (!EdgeTtsCache.Contains(voice, ratePercent, text))
                    {
                        await SynthesizeAsync(text, voice, ratePercent, token).ConfigureAwait(false);
                    }

                    Interlocked.Increment(ref succeeded);
                }
                catch (OperationCanceledException)
                {
                    // 用户点了取消：把异常抛给 Parallel.ForEachAsync 结束整个循环
                    throw;
                }
                catch (Exception ex)
                {
                    // 单个名字失败不影响其它名字，最后汇总失败数量
                    Interlocked.Increment(ref failed);
                    _logger.Debug(ex, "预加载 Edge 朗读音频失败：{Text}", text);
                }
                finally
                {
                    progress?.Report((Interlocked.Increment(ref done), total));
                }
            }).ConfigureAwait(false);

            _logger.Information(
                "Edge 朗读预加载结束：成功 {Succeeded}，失败 {Failed}，共 {Total}",
                succeeded, failed, total);
            return (succeeded, failed, total);
        }

        private static async Task<byte[]> SynthesizeWithEndpointAsync(
            string baseUrl, string keyParameter, string text, string voice,
            int ratePercent, CancellationToken cancellationToken)
        {
            string connectionId = Guid.NewGuid().ToString("N");
            string timestamp = DateTime.UtcNow.ToString(
                "ddd MMM dd yyyy HH:mm:ss 'GMT+0000 (Coordinated Universal Time)'");
            string url = $"{baseUrl}?{keyParameter}={EdgeTtsToken.TrustedClientToken}" +
                         $"&ConnectionId={connectionId}" +
                         $"&Sec-MS-GEC={Uri.EscapeDataString(EdgeTtsToken.GetToken())}" +
                         $"&Sec-MS-GEC-Version={EdgeTtsToken.GecVersion}";

            using var webSocket = new ClientWebSocket();
            webSocket.Options.SetRequestHeader("User-Agent", UserAgent);
            webSocket.Options.SetRequestHeader("Accept-Encoding", "gzip, deflate, br, zstd");
            webSocket.Options.SetRequestHeader("Accept-Language", "en-US,en;q=0.9");
            webSocket.Options.SetRequestHeader("Pragma", "no-cache");
            webSocket.Options.SetRequestHeader("Cache-Control", "no-cache");
            webSocket.Options.SetRequestHeader("Origin", Origin);
            webSocket.Options.SetRequestHeader("Cookie", $"muid={MakeMuid()};");

            using (var connectCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                connectCts.CancelAfter(ConnectTimeout);
                await webSocket.ConnectAsync(new Uri(url), connectCts.Token).ConfigureAwait(false);
            }

            using var overallCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            overallCts.CancelAfter(SynthesisTimeout);
            CancellationToken token = overallCts.Token;

            string configFrame = BuildTextFrame(
                ("Content-Type", "application/json; charset=utf-8"),
                ("Path", "speech.config"),
                ("X-Timestamp", timestamp)) + SpeechConfig;

            string ssml = BuildSsml(text, voice, ratePercent);
            string ssmlFrame = BuildTextFrame(
                ("Content-Type", "application/ssml+xml"),
                ("Path", "ssml"),
                ("X-RequestId", connectionId),
                ("X-Timestamp", timestamp)) + ssml;

            await webSocket.SendAsync(Encoding.UTF8.GetBytes(configFrame), WebSocketMessageType.Text, true, token)
                .ConfigureAwait(false);
            await webSocket.SendAsync(Encoding.UTF8.GetBytes(ssmlFrame), WebSocketMessageType.Text, true, token)
                .ConfigureAwait(false);

            using var audio = new MemoryStream();
            byte[] buffer = new byte[64 * 1024];
            using var frame = new MemoryStream();

            while (webSocket.State == WebSocketState.Open)
            {
                frame.SetLength(0);
                WebSocketReceiveResult result;
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token).ConfigureAwait(false);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    frame.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    // 服务端主动断开：已经收到的音频仍然可用，没有音频才当失败
                    if (audio.Length > 0)
                    {
                        break;
                    }

                    throw new EdgeTtsException($"连接被服务端关闭：{webSocket.CloseStatus} {webSocket.CloseStatusDescription}");
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string textFrame = Encoding.UTF8.GetString(frame.ToArray());
                    if (textFrame.IndexOf("Path:turn.end", StringComparison.Ordinal) >= 0)
                    {
                        break;
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    byte[] payload = frame.ToArray();
                    // 前 2 字节大端 = 帧头长度，帧头里是 Path:audio 之类的元数据，音频数据在它后面
                    if (payload.Length < 2)
                    {
                        continue;
                    }

                    int headerLength = (payload[0] << 8) | payload[1];
                    int audioStart = 2 + headerLength;
                    if (audioStart < payload.Length)
                    {
                        audio.Write(payload, audioStart, payload.Length - audioStart);
                    }
                }
            }

            if (audio.Length == 0)
            {
                throw new EdgeTtsException("服务端没有返回音频数据");
            }

            return audio.ToArray();
        }

        /// <summary>
        /// 拼一个“文本帧”：先是若干 header 行（\r\n 分隔），再一个空行，最后是正文。
        /// </summary>
        private static string BuildTextFrame(params (string Key, string Value)[] headers)
        {
            var builder = new StringBuilder();
            foreach (var (key, value) in headers)
            {
                builder.Append(key).Append(": ").Append(value).Append("\r\n");
            }

            return builder.Append("\r\n").ToString();
        }

        /// <summary>构造 SSML：语速用 prosody rate 表达；音量交给播放器（WaveOutEvent.Volume）。</summary>
        private static string BuildSsml(string text, string voice, int ratePercent)
        {
            string lang = "zh-CN";
            if (!string.IsNullOrEmpty(voice))
            {
                string[] parts = voice.Split('-');
                if (parts.Length >= 2)
                {
                    lang = $"{parts[0]}-{parts[1]}";
                }
            }

            string rate = $"{(ratePercent >= 0 ? "+" : "-")}{Math.Abs(ratePercent)}%";

            return "<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' " +
                   $"xml:lang='{lang}'><voice name='{EscapeXml(voice)}'><prosody pitch='+0Hz' " +
                   $"rate='{rate}' volume='+0%'>{EscapeXml(text)}</prosody></voice></speak>";
        }

        private static string EscapeXml(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private static string MakeMuid()
        {
            // Edge 会带一个随机的 muid Cookie，服务端只要求格式合法
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        }

        private static bool TryGetCached(string key, out byte[] audio)
        {
            lock (CacheGate)
            {
                return Cache.TryGetValue(key, out audio);
            }
        }

        private static void PutCache(string key, byte[] audio)
        {
            lock (CacheGate)
            {
                if (Cache.ContainsKey(key))
                {
                    return;
                }

                Cache[key] = audio;
                CacheOrder.Enqueue(key);
                while (CacheOrder.Count > CacheCapacity)
                {
                    Cache.Remove(CacheOrder.Dequeue());
                }
            }
        }

        /// <summary>拿一次在线音色列表；失败返回空列表（调用方自行回退到内置音色）。</summary>
        public static async Task<List<EdgeTtsVoice>> GetVoicesAsync(CancellationToken cancellationToken)
        {
            const string url =
                "https://speech.platform.bing.com/consumer/speech/synthesize/readaloud/voices/list" +
                "?trustedclienttoken=" + EdgeTtsToken.TrustedClientToken;

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", UserAgent);

            try
            {
                string json = await http.GetStringAsync(url, cancellationToken).ConfigureAwait(false);
                return ParseVoices(json);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "获取 Edge TTS 音色列表失败，将使用内置音色");
                return new List<EdgeTtsVoice>();
            }
        }

        private static List<EdgeTtsVoice> ParseVoices(string json)
        {
            var voices = new List<EdgeTtsVoice>();
            JArray array = JArray.Parse(json);
            foreach (JToken item in array)
            {
                string shortName = (string)item["ShortName"];
                if (string.IsNullOrEmpty(shortName))
                {
                    continue;
                }

                voices.Add(new EdgeTtsVoice
                {
                    ShortName = shortName,
                    Locale = (string)item["Locale"] ?? string.Empty,
                    Gender = (string)item["Gender"] ?? string.Empty,
                    FriendlyName = (string)item["FriendlyName"] ?? shortName,
                });
            }

            return voices
                .OrderBy(v => v.Locale.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(v => v.Locale)
                .ThenBy(v => v.ShortName)
                .ToList();
        }
    }

    /// <summary>Edge TTS 音色。</summary>
    public sealed class EdgeTtsVoice
    {
        /// <summary>音色短名，如 zh-CN-XiaoxiaoNeural（也是配置里保存的值）。</summary>
        public string ShortName { get; set; }

        /// <summary>语言区域，如 zh-CN。</summary>
        public string Locale { get; set; }

        /// <summary>性别（Female / Male）。</summary>
        public string Gender { get; set; }

        /// <summary>完整名称，如 “Microsoft 晓晓 Online (Natural) - Chinese (Mainland)”。</summary>
        public string FriendlyName { get; set; }

        /// <summary>下拉框里显示的文字：友好名 + 短名 + 性别（短名方便排查问题）。</summary>
        public string DisplayName
        {
            get
            {
                string gender = Gender == "Male" ? "男声" : Gender == "Female" ? "女声" : Gender;
                return string.IsNullOrEmpty(gender)
                    ? $"{FriendlyName} [{ShortName}]"
                    : $"{FriendlyName} [{ShortName} · {gender}]";
            }
        }

        /// <summary>下拉框里显示的文字。</summary>
        public override string ToString()
        {
            return DisplayName;
        }

        /// <summary>
        /// 内置音色（联网失败时的兜底）。只保留各语言最常用的几个，够用且不会过期得太难看。
        /// </summary>
        public static List<EdgeTtsVoice> GetFallbackVoices()
        {
            return new List<EdgeTtsVoice>
            {
                Fallback("zh-CN-XiaoxiaoNeural", "zh-CN", "女声", "晓晓"),
                Fallback("zh-CN-XiaoyiNeural", "zh-CN", "女声", "晓伊"),
                Fallback("zh-CN-YunxiNeural", "zh-CN", "男声", "云希"),
                Fallback("zh-CN-YunjianNeural", "zh-CN", "男声", "云健"),
                Fallback("zh-CN-YunyangNeural", "zh-CN", "男声", "云扬"),
                Fallback("zh-CN-YunxiaNeural", "zh-CN", "男声", "云夏"),
                Fallback("zh-CN-liaoning-XiaobeiNeural", "zh-CN-liaoning", "女声", "晓北（东北话）"),
                Fallback("zh-CN-shaanxi-XiaoniNeural", "zh-CN-shaanxi", "女声", "晓妮（陕西话）"),
                Fallback("zh-HK-HiuMaanNeural", "zh-HK", "女声", "曉曼（粤语）"),
                Fallback("zh-HK-WanLungNeural", "zh-HK", "男声", "雲龍（粤语）"),
                Fallback("zh-TW-HsiaoChenNeural", "zh-TW", "女声", "曉臻（台湾）"),
                Fallback("zh-TW-YunJheNeural", "zh-TW", "男声", "雲哲（台湾）"),
                Fallback("en-US-AriaNeural", "en-US", "女声", "Aria"),
                Fallback("en-US-GuyNeural", "en-US", "男声", "Guy"),
                Fallback("ja-JP-NanamiNeural", "ja-JP", "女声", "Nanami"),
            };
        }

        private static EdgeTtsVoice Fallback(string shortName, string locale, string gender, string name)
        {
            return new EdgeTtsVoice
            {
                ShortName = shortName,
                Locale = locale,
                Gender = gender == "男声" ? "Male" : "Female",
                FriendlyName = $"{name} · {locale}",
            };
        }
    }

    /// <summary>Edge TTS 相关错误（网络、协议、服务端拒绝等），统一成一种异常方便上层处理。</summary>
    public sealed class EdgeTtsException : Exception
    {
        public EdgeTtsException(string message) : base(message)
        {
        }

        public EdgeTtsException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    /// <summary>
    /// 用 NAudio 播放 Edge TTS 返回的 MP3 数据。同一时间只允许播一段。
    /// </summary>
    internal sealed class EdgeTtsPlayer : IDisposable
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(EdgeTtsPlayer));

        private readonly object _gate = new object();
        private WaveOutEvent _output;
        private bool _disposed;

        /// <summary>
        /// 播放一段 MP3，阻塞到播完或取消。取消会立即停止发声。
        /// </summary>
        public void Play(byte[] audio, int volume, CancellationToken cancellationToken)
        {
            if (audio == null || audio.Length == 0 || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            WaveOutEvent output = null;
            Mp3FileReader reader = null;
            try
            {
                reader = new Mp3FileReader(new MemoryStream(audio));
                output = new WaveOutEvent
                {
                    Volume = Math.Clamp(volume, 0, 100) / 100f,
                };

                using var stopped = new ManualResetEventSlim(false);
                output.PlaybackStopped += (_, _) => stopped.Set();

                // 先把“停止播放”挂上：初始化设备可能要几百毫秒，这期间就被取消的话，
                // 下面的循环会发现并立刻停掉，而不是傻等整段播完。
                using var registration = cancellationToken.Register(Stop);

                output.Init(reader);
                lock (_gate)
                {
                    if (_disposed)
                    {
                        return;
                    }

                    _output = output;
                }

                output.Play();

                // 轮询而不是无限等：既能响应取消，也能覆盖“取消发生在注册之前”的竞态
                while (!stopped.Wait(50))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Stop();
                    }
                }
            }
            finally
            {
                lock (_gate)
                {
                    if (ReferenceEquals(_output, output))
                    {
                        _output = null;
                    }
                }

                try
                {
                    output?.Stop();
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "停止 Edge TTS 播放时发生异常，已忽略");
                }

                try
                {
                    output?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "释放 Edge TTS 播放器时发生异常，已忽略");
                }

                reader?.Dispose();
            }
        }

        /// <summary>立即停止当前播放（没有播放时什么也不做）。可从任意线程调用。</summary>
        public void Stop()
        {
            lock (_gate)
            {
                try
                {
                    _output?.Stop();
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "停止 Edge TTS 播放时发生异常，已忽略");
                }
            }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                _disposed = true;
            }

            Stop();
        }
    }
}
