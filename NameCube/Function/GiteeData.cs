using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Net.Http;

namespace NameCube.Function
{
    internal class GiteeData
    {

            /// <summary>
            /// 异步下载文件。
            /// </summary>
            /// <param name="url">文件URL</param>
            /// <param name="localPath">本地保存路径</param>
            private static async Task DownloadFileAsync(string url, string localPath)
            {
                // 使用HttpClient发送请求
                using (var httpClient = new HttpClient())
                {
                    try
                    {
                        // 发送请求，仅获取响应头（不下载内容）
                        using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                        {
                            response.EnsureSuccessStatusCode();

                            // 获取文件总大小（可能为null，表示服务器未返回）
                            long? totalBytes = response.Content.Headers.ContentLength;
                            bool canReportProgress = totalBytes.HasValue && totalBytes.Value > 0;

                            // 打开网络流和本地文件流
                            using (var contentStream = await response.Content.ReadAsStreamAsync())
                            using (var fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                            {
                                var buffer = new byte[8192];
                                long totalBytesRead = 0;
                                int bytesRead;

                                // 用于限速更新UI的计时器和状态
                                var stopwatch = Stopwatch.StartNew();
                                var lastUpdateTime = DateTime.UtcNow;
                                long lastTotalBytesRead = 0;
                                TimeSpan updateInterval = TimeSpan.FromMilliseconds(100); // 每100ms更新一次UI

                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                                    totalBytesRead += bytesRead;

                                    // 计算当前时间，判断是否需要更新UI
                                    var now = DateTime.UtcNow;
                                    if (now - lastUpdateTime > updateInterval || totalBytesRead == totalBytes) // 最后强制更新一次
                                    {
                                        // 计算瞬时速度（基于上次更新以来的平均值）
                                        var timeSinceLastUpdate = now - lastUpdateTime;
                                        if (timeSinceLastUpdate.TotalSeconds > 0)
                                        {
                                            long bytesSinceLastUpdate = totalBytesRead - lastTotalBytesRead;
                                            double speedBps = bytesSinceLastUpdate / timeSinceLastUpdate.TotalSeconds;
                                            //string speedText = FormatSpeed(speedBps);

                                            // 更新速度文本（必须在UI线程）
                                            //await Dispatcher.InvokeAsync(() =>
                                            //{
                                            //    SpeedTextBlock.Text = speedText;
                                            //});
                                        }

                                        // 更新进度条（如果有总大小）
                                        if (canReportProgress)
                                        {
                                            double percentage = (double)totalBytesRead / totalBytes.Value * 100;
                                            //await Dispatcher.InvokeAsync(() =>
                                            //{
                                            //    DownloadProgressBar.Value = percentage;
                                            //});
                                        }

                                        // 重置计时状态
                                        lastUpdateTime = now;
                                        lastTotalBytesRead = totalBytesRead;
                                    }
                                }

                                stopwatch.Stop();

                                //// 下载完成后确保进度条显示100%（如果可确定总大小）
                                //if (canReportProgress)
                                //{
                                //    await Dispatcher.InvokeAsync(() =>
                                //    {
                                //        //DownloadProgressBar.Value = 100;
                                //    });
                                //}
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex; // 将异常抛出到调用者处理
                    }
                }
            }
            public static async Task<string> GerVersion()
            {
                try
                {
                    await DownloadFileAsync("https://gitee.com/haaa4/NameCube/releases/download/V0/Version.txt", AppDomain.CurrentDomain.BaseDirectory + "\\TEMP.version");
                    string get = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\TEMP.version");
                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\TEMP.version");
                    return get;
                }
                catch (Exception ex) { throw ex; }
            }
        }
    
}
