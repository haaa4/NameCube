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
        /// 获取最新仓库版本
        /// </summary>
        /// <param name="owner">仓库的所有者</param>
        /// <param name="repoName">仓库的名称</param>
        /// <returns></returns>
        /// <exception cref="Exception">响应失败</exception>
        public static async Task<string> GetLatestReleaseVersionAsync(string owner, string repoName)
        {
            using (var httpClient = new HttpClient()) // 改为传统using语法
            {
                const string userAgent = "WPF-Update-Checker";

                try
                {
                    // 配置请求头
                    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

                    // 发送API请求
                    var response = await httpClient.GetAsync(
                        $"https://api.github.com/repos/{owner}/{repoName}/releases");

                    response.EnsureSuccessStatusCode();

                    // 解析响应内容
                    var content = await response.Content.ReadAsStringAsync();
                    var releases = JsonConvert.DeserializeObject<List<GitHubRelease>>(content);

                    // 筛选有效Release
                    var validReleases = releases?
                        .Where(r => !r.IsDraft)
                        .OrderByDescending(r => r.PublishedAt)
                        .ToList();

                    return validReleases?.FirstOrDefault()?.TagName
                           ?? throw new Exception("No valid releases found");
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
        private CancellationTokenSource _cts;
        private Stopwatch _stopwatch;
        private long _totalBytes;
        private long _bytesRead;

        public event Action<long, long, double, double> ProgressChanged;
        public event Action<bool> Completed;

        public DownloadHelper()
        {
            _client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        }

        public async Task DownloadFileAsync(string url, string savePath, CancellationToken cancellationToken = default)
        {   
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
