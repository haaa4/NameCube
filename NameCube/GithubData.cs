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
            using (var httpClient = new HttpClient())
            {
                const string userAgent = "WPF-Update-Checker";

                try
                {
                    // 配置基础请求头
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

                    // 添加 GitHub 令牌认证（如果提供了令牌）
                    if (!string.IsNullOrWhiteSpace(githubToken))
                    {
                        httpClient.DefaultRequestHeaders.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", githubToken);
                    }

                    // 发送API请求
                    var response = await httpClient.GetAsync(
                        $"https://api.github.com/repos/{owner}/{repoName}/releases");

                    response.EnsureSuccessStatusCode();

                    // 解析响应内容
                    var content = await response.Content.ReadAsStringAsync();
                    var releases = JsonConvert.DeserializeObject<List<GitHubRelease>>(content);

                    if (GlobalVariables.json.AllSettings.UpdataGet == 0)
                    {
                        // 筛选正式版 Release
                        var validReleases = releases?
                            .Where(r => !r.IsDraft && !r.IsPrerelease)
                            .OrderByDescending(r => r.PublishedAt)
                            .ToList();

                        return validReleases?.FirstOrDefault()?.TagName
                               ?? throw new Exception("未找到有效版本");
                    }
                    else
                    {
                        // 包含预发布版本
                        var validReleases = releases?
                           .Where(r => !r.IsDraft)
                           .OrderByDescending(r => r.PublishedAt)
                           .ToList();

                        return validReleases?.FirstOrDefault()?.TagName
                               ?? throw new Exception("未找到有效版本");
                    }
                }
                catch (HttpRequestException ex)
                {
                    throw new Exception($"GitHub API请求失败: {ex.Message}");
                }
                catch (JsonException ex)
                {
                    throw new Exception("响应数据解析失败: " + ex.Message);
                }
            }
        }
    }
    public class DownloadHelper
    {
        private readonly HttpClient _client;
        private CancellationTokenSource _cts=new CancellationTokenSource();
        private Stopwatch _stopwatch=new Stopwatch();
        private long _totalBytes=0;
        private long _bytesRead=0;

        public event Action<long, long, double, double> ProgressChanged;

        public DownloadHelper()
        {
            _client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        }



        private void ReportProgress()
        {
            var speed = _bytesRead / _stopwatch.Elapsed.TotalSeconds; // 字节/秒
            var progress = _totalBytes > 0 ? (double)_bytesRead / _totalBytes * 100 : 0;

            ProgressChanged?.Invoke(
                _bytesRead,
                _totalBytes,
                speed,
                progress
            );
        }

        public void Cancel()
        {
            _cts?.Cancel();
        }
    }
}
