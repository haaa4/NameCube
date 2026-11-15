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
using System.Security.Cryptography;
using System.Text;

namespace NameCube.Function
{
    /// <summary>
    /// Edge 朗读（微软在线 TTS）握手时需要携带的 Sec-MS-GEC 令牌。
    /// </summary>
    /// <remarks>
    /// 服务端拿这个令牌做两件事：算出来必须完全一致、且时间戳必须落在当前 5 分钟窗口内，
    /// 否则 WebSocket 直接返回 403。算法逆向自 edge-tts 的 drm.py：
    /// 1. 明文的 SHA-256 是标准的（前缀 + 时间戳 + 客户端令牌），
    /// 2. 但令牌前半段 CS64 用了微软自定义的混淆轮函数，必须按原样移植（下面的常量表不能动）。
    /// 令牌形如 `{CS64 16 位十六进制}-{SHA256 64 位十六进制}`，配 Sec-MS-GEC-Version=2-xxx 使用。
    /// </remarks>
    internal static class EdgeTtsToken
    {
        /// <summary>微软 Edge 朗读公开使用的客户端令牌（浏览器端硬编码值）。</summary>
        internal const string TrustedClientToken = "6A5AA1D4EAFF4E9FB37E23D68491D6F4";

        /// <summary>令牌版本号，必须与令牌算法代次对应（2 代 + 143 版 Chromium）。</summary>
        internal const string GecVersion = "2-143.0.3650.75";

        // .NET 的 DateTime.Ticks 从 0001-01-01 起算，Windows FILETIME 从 1601-01-01 起算，差这么多 100ns。
        private const long WinEpochOffsetTicks = 504911232000000000L;

        private static readonly byte[] Prefix =
            Encoding.ASCII.GetBytes("Copyright (C) Microsoft Corporation. All rights reserved.");

        // CS64 混淆用的系数/偏移表（14 项）。这是逆向出来的“魔法数字”，改动即失效。
        private static readonly (uint Coef, uint Offset)[] Coeffs =
        {
            (0xE82E91B1, 0xB32700D0), (0x44DE24C6, 0x88DFF51D), (0x3BEB65BC, 0x29514038), (0xC89C5933, 0xB328CBDF),
            (0x8D1EC211, 0xD405B4D5), (0x314EF5D5, 0x26E55F3C), (0xE92491B1, 0x607801C6), (0xF0A5B9D5, 0x82B2C1FB),
            (0x06064CF3, 0xFA51A3DA), (0xDA79FA60, 0x207CC464), (0xBED20560, 0x3C3C74F3), (0x89F9C9F8, 0xD2D14B61),
            (0x35FA38C6, 0xBB04A2C1), (0xCB49E3C6, 0xC5626167),
        };

        private static readonly object Gate = new object();
        private static long _cachedWindowTicks = long.MinValue;
        private static string _cachedToken;

        /// <summary>
        /// 取当前时间窗口的令牌。5 分钟窗口内直接复用缓存，省掉一次 SHA-256 + 混淆运算。
        /// </summary>
        internal static string GetToken()
        {
            long windowTicks = GetWindowTicks(DateTime.UtcNow);
            lock (Gate)
            {
                if (_cachedToken != null && _cachedWindowTicks == windowTicks)
                {
                    return _cachedToken;
                }

                _cachedToken = Generate(windowTicks);
                _cachedWindowTicks = windowTicks;
                return _cachedToken;
            }
        }

        /// <summary>
        /// 计算令牌（内部可见，便于自检）。ticks 为对齐到 300 秒的 Windows 时间戳。
        /// </summary>
        internal static string Generate(long windowTicks)
        {
            var (cs64, digest) = TokenParts($"{windowTicks}{TrustedClientToken}");
            return $"{cs64}-{digest}";
        }

        /// <summary>把当前 UTC 时间对齐到 300 秒窗口，转成 Windows FILETIME（100ns）。</summary>
        private static long GetWindowTicks(DateTime utcNow)
        {
            long ticks = utcNow.Ticks - WinEpochOffsetTicks;
            return ticks - ticks % (300L * TimeSpan.TicksPerSecond);
        }

        /// <summary>
        /// 由 “时间戳 + 客户端令牌” 算出 (CS64 十六进制, SHA256 十六进制)。
        /// </summary>
        private static (string Cs64, string Digest) TokenParts(string ticksAndToken)
        {
            byte[] suffix = Encoding.ASCII.GetBytes(ticksAndToken);
            byte[] message = new byte[Prefix.Length + suffix.Length];
            Prefix.CopyTo(message, 0);
            suffix.CopyTo(message, Prefix.Length);

            byte[] digest = SHA256.HashData(message);
            uint keyA = BitConverter.ToUInt32(digest, 0) | 1u;
            uint keyB = BitConverter.ToUInt32(digest, 4) | 1u;

            // 1) 先铺 14 项固定表（奇偶交替乘以 keyA/keyB 的倍数）
            uint f0 = unchecked(keyA * 0x82893933u);
            uint f1 = unchecked(keyB * 0x82893933u);
            var terms = new List<uint>();
            var offsets = new List<uint>();
            foreach (var (coef, offset) in Coeffs)
            {
                terms.Add(unchecked(coef * (offsets.Count % 2 == 0 ? f0 : f1)));
                offsets.Add(offset);
            }

            // 2) 再把输入文本补一个前导 0 字节，按 4 字节一组（LE32、奇数补 0）逐组展开
            byte[] data = new byte[1 + suffix.Length];
            suffix.CopyTo(data, 1);
            uint[] words = WordsLeEven(data);
            for (int i = 0; i < words.Length; i++)
            {
                uint term = unchecked(words[i] * (i % 2 == 0 ? keyA : keyB));
                uint offset = unchecked(Rol16(words[i]) * (i % 2 == 0 ? 0xEF0569FBu : 0xC31713DBu));
                if (i == 0)
                {
                    term = unchecked(term + 0xF2F90B1Au * f0);
                    offset = unchecked(offset + 0x0B1A0000u);
                }

                terms.Add(term);
                offsets.Add(offset);
            }

            // 3) 两两一组跑 swap 链与 state 链
            uint swapAcc = 0, swapLast = 0, state = 0, acc = 0;
            for (int i = 0; i < terms.Count; i += 2)
            {
                uint x = terms[i], y = terms[i + 1];

                uint t = Rol16(unchecked(offsets[i] + x));
                uint q = unchecked(t * 0x76F50F34u);
                swapAcc = unchecked(swapAcc + (Rol16(q) * 0xC3EFEA97u + q));
                t = Rol16(unchecked(offsets[i + 1] + y));
                q = unchecked(t * 0x1881CE3Cu);
                swapLast = unchecked(Rol16(q) * 0x35BD1EC9u + q);
                swapAcc = unchecked(swapAcc + swapLast);

                state = unchecked(state * keyA + x);
                state = unchecked(Rol16(state) * 0xCF98B111u);
                state = unchecked(Rol16(state) * 0x87085B9Fu);
                state = unchecked(Rol16(state) * 0x12CEB96Du);
                state = unchecked(Rol16(state) * 0x257E1D83u);
                acc = unchecked(acc + state);
                t = unchecked(state * keyB + y);
                t = unchecked(Rol16(t) * 0xA27416F5u);
                t = unchecked(Rol16(t) * 0xD38396FFu);
                state = unchecked(Rol16(unchecked(Rol16(t) * 0x7C932B89u)) * 0xBFA49F69u);
                acc = unchecked(acc + state);
            }

            byte[] cs64 = new byte[8];
            BitConverter.GetBytes(swapLast ^ state).CopyTo(cs64, 0);
            BitConverter.GetBytes(swapAcc ^ acc).CopyTo(cs64, 4);

            return (Convert.ToHexString(cs64), Convert.ToHexString(digest));
        }

        private static uint Rol16(uint x) => (x << 16) | (x >> 16);

        /// <summary>按小端 4 字节切分并补零到偶数个，模拟原实现的 to_words()。</summary>
        private static uint[] WordsLeEven(byte[] data)
        {
            int pad4 = (4 - data.Length % 4) % 4;
            byte[] buf = new byte[data.Length + pad4];
            Array.Copy(data, buf, data.Length);

            int count = buf.Length / 4;
            bool odd = count % 2 == 1;
            uint[] words = new uint[count + (odd ? 1 : 0)];
            for (int i = 0; i < count; i++)
            {
                words[i] = BitConverter.ToUInt32(buf, i * 4);
            }

            return words;
        }
    }
}
