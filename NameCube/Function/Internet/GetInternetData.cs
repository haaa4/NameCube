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
using Serilog;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NameCube.Function.Internet
{
    internal static class GetInternetData
    {
        /// <summary>
        /// 服务器上的版本数据地址（GitHub 源）。
        /// </summary>
        private const string GithubDataUrl = "https://raw.githubusercontent.com/haaa4/NameCubeResources/refs/heads/main/Data";

        /// <summary>
        /// 服务器上的版本数据地址（Gitee 源）。GitHub 源不可用时作为兜底。
        /// 这里用的是不带签名参数的常驻地址，不会随签名过期而失效。
        /// </summary>
        private const string GiteeDataUrl = "https://gitee.com/haaa4/NameCubeResources/raw/main/Data";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// 最近一次成功解析的服务器数据。获取失败时保持上一次的值，永远不会被写成 null。
        /// </summary>
        public static NewVersionInformationClass NewVersionInformationClass { get; private set; } = new NewVersionInformationClass();

        /// <summary>
        /// 从服务器刷新版本信息。
        /// 优先使用用户在设置里选择的下载源，失败后自动切换到另一个源；
        /// 全部失败时返回 null（并记录日志），绝不抛出异常、也绝不把空字符串交给反序列化。
        /// </summary>
        /// <returns>成功时返回新的数据；失败时返回 null，此时 <see cref="NewVersionInformationClass"/> 保持旧值。</returns>
        public static async Task<NewVersionInformationClass> RefreshNewVersionInformationClassAsync(CancellationToken cancellationToken = default)
        {
            // DownloadWay: 0 = GitHub，1 = Gitee。选中的源放前面，另一个源兜底。
            bool preferGitee = GlobalVariablesData.config?.AllSettings?.DownloadWay == 1;
            string[] urls = preferGitee
                ? new[] { GiteeDataUrl, GithubDataUrl }
                : new[] { GithubDataUrl, GiteeDataUrl };

            Exception lastException = null;

            foreach (string url in urls)
            {
                try
                {
                    string jsonstring = await DownloadHelper.DownloadFileAsyncToString(url, cancellationToken);

                    // 拿到空响应绝不能继续往下走，否则 Deserialize 会抛 JsonException。
                    if (string.IsNullOrWhiteSpace(jsonstring))
                    {
                        Log.Warning("Failed to download data from the internet. Url: {Url} (empty response)", url);
                        continue;
                    }

                    NewVersionInformationClass parsed;
                    try
                    {
                        parsed = JsonSerializer.Deserialize<NewVersionInformationClass>(jsonstring, JsonOptions);
                    }
                    catch (JsonException ex)
                    {
                        // 代理/CDN 返回 HTML 错误页时会走到这里，换下一个源再试。
                        Log.Warning(ex, "服务器返回的更新数据不是有效的 JSON。Url: {Url}", url);
                        lastException = ex;
                        continue;
                    }

                    if (parsed == null)
                    {
                        Log.Warning("服务器返回的更新数据为 null。Url: {Url}", url);
                        continue;
                    }

                    NewVersionInformationClass = parsed;
                    Log.Information("更新数据获取成功，来源: {Url}", url);
                    return parsed;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    // 网络失败很常见（比如国内直连 GitHub 超时），只记一行消息，完整堆栈留给 Debug。
                    Log.Debug(ex, "从 {Url} 获取更新数据失败（完整异常）", url);
                    Log.Warning("从 {Url} 获取更新数据失败：{Message}", url, ex.Message);
                }
            }

            Log.Error(lastException, "无法从任何下载源获取更新数据，本次检查跳过。");
            return null;
        }
    }
}
