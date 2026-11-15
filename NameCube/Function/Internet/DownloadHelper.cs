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
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NameCube.Function.Internet
{
    internal static class DownloadHelper
    {
        // 复用同一个 HttpClient：每次 new 一个会不断堆积 TIME_WAIT 的 socket，
        // 超时也统一在这里设置，网络异常时不会无限期等待。
        private static readonly HttpClient httpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };
            // 部分 CDN 对没有 User-Agent 的请求更严格，这里带上一个固定的客户端标识。
            client.DefaultRequestHeaders.UserAgent.ParseAdd("NameCube-Update-Client/1.0");
            return client;
        }

        /// <summary>
        /// 异步下载文件至本地。
        /// </summary>
        /// <param name="url">文件URL</param>
        /// <param name="localPath">本地保存路径</param>
        public static async Task DownloadFileAsync(string url, string localPath)
        {
            using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    await contentStream.CopyToAsync(fileStream);
                }
            }
        }

        /// <summary>
        /// 异步下载文件至字符串。
        /// 失败时抛出原始的网络异常（HttpRequestException 等），由调用方决定如何降级，
        /// 不再包装成无法区分的 Exception。
        /// </summary>
        /// <param name="url">文件URL</param>
        /// <param name="cancellationToken">取消令牌</param>
        public static async Task<string> DownloadFileAsyncToString(string url, CancellationToken cancellationToken = default)
        {
            using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead, cancellationToken))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"服务器返回 {(int)response.StatusCode} {response.ReasonPhrase}",
                        null,
                        response.StatusCode);
                }

                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
        }
    }
}
