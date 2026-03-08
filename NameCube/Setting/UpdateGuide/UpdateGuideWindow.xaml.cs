using NameCube.Function;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Path = System.IO.Path;

namespace NameCube.Setting.UpdateGuide
{
    /// <summary>
    /// UpdateGuideWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateGuideWindow : Window
    {
        string version;
        bool isError = false;
        public UpdateGuideWindow(string version)
        {
            InitializeComponent();
            this.version = version;
        }
        //example:https://github.com/haaa4/NameCube/releases/download/V1.3-Beta-2.1(%23p0%23)/NameCubeSetupX64.exe
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(version=="Error")
            {
                //这里是B端未正常启动而触发的逻辑
                Image1st.Source = FinishExampleImage.Source;
                Text1st.Foreground= Brushes.Green;
                Text2nd.Foreground = Brushes.Red;
                ErrorAttention.Text = "未能如期启动更新程序，请检查你的操作，或者前往Github进行手动更新";
                isError = true;
                return;
            }
            if(version=="Finish")
            {
                //这里是B端已完成更新而触发的逻辑
                Image1st.Source = FinishExampleImage.Source;
                Image2nd.Source = FinishExampleImage.Source;
                Image3rd.Source = FinishExampleImage.Source;
                Image4th.Source = FinishExampleImage.Source;
                Text1st.Foreground = Brushes.Green;
                Text2nd.Foreground = Brushes.Green;
                Text3rd.Foreground = Brushes.Green;
                Text4th.Foreground = Brushes.Green;
                Text5th.Foreground = Brushes.Blue;
                try
                {
                    Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update"),true);
                    ErrorAttention.Text= "更新完成，引导将在5秒后自动退出";
                    Text5th.Foreground = Brushes.Green;
                    Image5th.Source = FinishExampleImage.Source;
                }
                catch (Exception ex)
                {
                    ErrorAttention.Text = ex.Message+"(不影响新版本软件继续使用，引导将在5秒后自动退出)";
                    Text5th.Foreground = Brushes.Orange;
                }
                await Task.Delay(5000);
                Assembly assembly = Assembly.GetExecutingAssembly();
                string[] get = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
                string resourceName = "NameCube.MarkDown.NewVersion.md";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new Exception("资源未找到");
                    using (FileStream fileStream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\新版本介绍.md", FileMode.Create))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "\\新版本介绍.md");
                //防止被阻止
                isError = true;
                this.Close();
            }
            //第一步，下载文件
            //尝试下载更新文件，并实时更新进度条和下载速度
            Text1st.Foreground = Brushes.Blue;
            string url = version.Replace("#", "%23");
            string localPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update","NameCubeSetupX64.exe");
            try
            {
                Directory.CreateDirectory(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update"));
                if(GlobalVariablesData.config.AllSettings.DownloadWay==0)
                {
                    await DownloadFileAsync("https://github.com/haaa4/NameCube/releases/download/" + url + "/NameCubeSetupX64.exe", localPath);
                }
                else
                {
                    await DownloadFileAsync("https://gitee.com/haaa4/NameCube/releases/download/" + url + "/NameCubeSetupX64.exe", localPath);
                }

            }
            catch (Exception ex)
            {
                ErrorAttention.Text = ex.Message;
                Text1st.Foreground = Brushes.Red;
                isError = true;
                return;
            }
            Text1st.Foreground = Brushes.Green;
            Image1st.Source = FinishExampleImage.Source;
            //第二步，下载完成后，自动打开文件所在目录，并选中下载的文件
            Text2nd.Foreground = Brushes.Blue;
            try
            {
                File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update", "START"), "");
            }
            catch (Exception ex)
            {
                ErrorAttention.Text = ex.Message;
                Text2nd.Foreground = Brushes.Red;
                isError = true;
                return;
            }
            StartButton.IsEnabled = true;
            //接下来的内容由更新B端完成
        }
        /// <summary>
        /// 异步下载文件，并实时更新进度条和下载速度。
        /// </summary>
        /// <param name="url">文件URL</param>
        /// <param name="localPath">本地保存路径</param>
        private async Task DownloadFileAsync(string url, string localPath)
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
                                        string speedText = FormatSpeed(speedBps);

                                        // 更新速度文本（必须在UI线程）
                                        await Dispatcher.InvokeAsync(() =>
                                        {
                                            SpeedTextBlock.Text = speedText;
                                        });
                                    }

                                    // 更新进度条（如果有总大小）
                                    if (canReportProgress)
                                    {
                                        double percentage = (double)totalBytesRead / totalBytes.Value * 100;
                                        await Dispatcher.InvokeAsync(() =>
                                        {
                                            DownloadProgressBar.Value = percentage;
                                        });
                                    }

                                    // 重置计时状态
                                    lastUpdateTime = now;
                                    lastTotalBytesRead = totalBytesRead;
                                }
                            }

                            stopwatch.Stop();

                            // 下载完成后确保进度条显示100%（如果可确定总大小）
                            if (canReportProgress)
                            {
                                await Dispatcher.InvokeAsync(() =>
                                {
                                    DownloadProgressBar.Value = 100;
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex; // 将异常抛出到调用者处理
                }
            }
        }

        /// <summary>
        /// 将字节/秒格式化为合适的单位（B/s, KB/s, MB/s, GB/s）
        /// </summary>
        private string FormatSpeed(double bytesPerSecond)
        {
            const double KB = 1024;
            const double MB = KB * 1024;
            const double GB = MB * 1024;

            if (bytesPerSecond >= GB)
                return $"{bytesPerSecond / GB:F2} GB/s";
            if (bytesPerSecond >= MB)
                return $"{bytesPerSecond / MB:F2} MB/s";
            if (bytesPerSecond >= KB)
                return $"{bytesPerSecond / KB:F2} KB/s";
            return $"{bytesPerSecond:F0} B/s";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //在不报错的情况下，就不允许用户关闭窗口
            if(!isError)
            {
                MessageBoxFunction.ShowMessageBoxWarning("在完成更新前，请勿关闭此窗口！");
                e.Cancel = true; // 取消关闭事件，防止窗口被关闭
            }

        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update", "NameCubeSetupX64.exe"),"UpdateMode");
            Log.Information("用户已点击开始安装，正在启动安装程序...应用将退出");
            Application.Current.Shutdown(); // 关闭当前应用程序
        }
    }
}
