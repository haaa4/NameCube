using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;  // 添加Serilog命名空间

namespace NameCube
{
    internal class GithubData
    {
        public class GitHubRelease
        {
            [JsonProperty("tag_name")]
            public string TagName { get; set; }

            [JsonProperty("published_at")]
            public DateTime PublishedAt { get; set; }

            [JsonProperty("prerelease")]
            public bool IsPrerelease { get; set; }

            [JsonProperty("draft")]
            public bool IsDraft { get; set; }
        }

        /// <summary>
        /// 获取最新仓库版本（支持 GitHub 令牌认证）
        /// </summary>
        /// <param name="owner">仓库的所有者</param>
        /// <param name="repoName">仓库的名称</param>
        /// <param name="githubToken">GitHub 个人访问令牌（可选）</param>
        /// <returns></returns>
        /// <exception cref="Exception">响应失败</exception>
        public static async Task<string> GetLatestReleaseVersionAsync(
            string owner,
            string repoName,
            string githubToken = null)  // 添加可选令牌参数
        {
            Log.Information("开始获取GitHub最新版本，仓库: {Owner}/{RepoName}", owner, repoName);

            using (var httpClient = new HttpClient())
            {
                const string userAgent = "WPF-Update-Checker";

                try
                {
                    // 配置基础请求头
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                    Log.Debug("设置User-Agent: {UserAgent}", userAgent);

                    // 添加 GitHub 令牌认证（如果提供了令牌）
                    if (!string.IsNullOrWhiteSpace(githubToken))
                    {
                        httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", githubToken);
                        Log.Debug("已添加GitHub令牌认证");
                    }
                    else
                    {
                        Log.Debug("未提供GitHub令牌，使用匿名访问");
                    }

                    // 发送API请求
                    string apiUrl = $"https://api.github.com/repos/{owner}/{repoName}/releases";
                    Log.Debug("发送GitHub API请求: {ApiUrl}", apiUrl);

                    var response = await httpClient.GetAsync(apiUrl);
                    Log.Debug("GitHub API响应状态: {StatusCode}", response.StatusCode);

                    response.EnsureSuccessStatusCode();

                    // 解析响应内容
                    var content = await response.Content.ReadAsStringAsync();
                    Log.Debug("GitHub API响应内容长度: {ContentLength}", content.Length);

                    var releases = JsonConvert.DeserializeObject<List<GitHubRelease>>(content);
                    Log.Debug("解析到{ReleaseCount}个发布版本", releases?.Count ?? 0);

                    if (GlobalVariables.json.AllSettings.UpdataGet == 0)
                    {
                        Log.Debug("仅获取正式版发布");
                        // 筛选正式版 Release
                        var validReleases = releases?
                            .Where(r => !r.IsDraft && !r.IsPrerelease)
                            .OrderByDescending(r => r.PublishedAt)
                            .ToList();

                        var latestRelease = validReleases?.FirstOrDefault();
                        if (latestRelease != null)
                        {
                            Log.Information("找到最新正式版: {TagName}，发布时间: {PublishedAt}",
                                latestRelease.TagName, latestRelease.PublishedAt);
                            return latestRelease.TagName;
                        }
                        else
                        {
                            Log.Warning("未找到有效的正式版发布");
                            throw new Exception("未找到有效版本");
                        }
                    }
                    else
                    {
                        Log.Debug("获取包含预发布的版本");
                        // 包含预发布版本
                        var validReleases = releases?
                           .Where(r => !r.IsDraft)
                           .OrderByDescending(r => r.PublishedAt)
                           .ToList();

                        var latestRelease = validReleases?.FirstOrDefault();
                        if (latestRelease != null)
                        {
                            Log.Information("找到最新版本: {TagName}，预发布: {IsPrerelease}，发布时间: {PublishedAt}",
                                latestRelease.TagName, latestRelease.IsPrerelease, latestRelease.PublishedAt);
                            return latestRelease.TagName;
                        }
                        else
                        {
                            Log.Warning("未找到有效的发布版本");
                            throw new Exception("未找到有效版本");
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    Log.Error(ex, "GitHub API请求失败");
                    throw new Exception($"GitHub API请求失败: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    Log.Error(ex, "GitHub API响应数据解析失败");
                    throw new Exception("响应数据解析失败: " + ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "获取GitHub最新版本时发生未知错误");
                    throw;
                }
            }
        }
    }

    public class DownloadHelper
    {
        private readonly HttpClient _client;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private Stopwatch _stopwatch = new Stopwatch();
        private long _totalBytes = 0;
        private long _bytesRead = 0;

        public event Action<long, long, double, double> ProgressChanged;

        public DownloadHelper()
        {
            Log.Debug("初始化DownloadHelper");
            _client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        }

        private void ReportProgress()
        {
            try
            {
                var speed = _bytesRead / _stopwatch.Elapsed.TotalSeconds; // 字节/秒
                var progress = _totalBytes > 0 ? (double)_bytesRead / _totalBytes * 100 : 0;

                Log.Debug("下载进度: {BytesRead}/{TotalBytes} ({Progress:F1}%)，速度: {Speed:F1} B/s",
                    _bytesRead, _totalBytes, progress, speed);

                ProgressChanged?.Invoke(
                    _bytesRead,
                    _totalBytes,
                    speed,
                    progress
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "报告下载进度时发生错误");
            }
        }

        public void Cancel()
        {
            try
            {
                Log.Information("取消下载");
                _cts?.Cancel();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "取消下载时发生错误");
            }
        }
    }
}