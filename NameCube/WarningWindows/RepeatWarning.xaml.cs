using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Threading;
using Windows.UI.Xaml.Controls;
using Serilog;

namespace NameCube
{
    /// <summary>
    /// RepeatWarning.xaml 的交互逻辑
    /// </summary>
    public partial class RepeatWarning
    {
        private string _tempVideoPath;

        public RepeatWarning()
        {
            Log.Information("创建重复启动警告窗口");
            try
            {
                InitializeComponent();
                if (GlobalVariables.json.AllSettings.NameCubeMode == 1)
                {
                    Text.Text = "点鸣魔方已经启动，请勿再次启动";
                    Log.Information("点鸣魔方模式设置为1，显示对应警告消息");
                }
                else
                {
                    Log.Debug("点鸣魔方模式: {Mode}", GlobalVariables.json.AllSettings.NameCubeMode);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化重复启动警告窗口时发生错误");
                throw;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击退出按钮，关闭重复启动警告窗口");
            this.Hide();
            CleanupTempFile();
            Environment.Exit(0);
        }

        private void ExtractAndPlayVideo()
        {
            try
            {
                Log.Debug("开始提取并播放嵌入式视频");
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resourceName = "NameCube.Video.RepeatWarningVideo.mp4";

                if (assembly.GetManifestResourceStream(resourceName) == null)
                {
                    resourceName = assembly.GetName().Name + ".Video.RepeatWarningVideo.mp4";
                    Log.Debug("资源名称调整为: {ResourceName}", resourceName);
                }

                using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream != null)
                    {
                        Log.Debug("找到嵌入式视频资源，大小: {Size} bytes", resourceStream.Length);
                        _tempVideoPath = System.IO.Path.GetTempFileName() + ".mp4";
                        Log.Debug("创建临时视频文件: {TempVideoPath}", _tempVideoPath);

                        using (FileStream fileStream = File.Create(_tempVideoPath))
                        {
                            resourceStream.CopyTo(fileStream);
                        }

                        mediaElement.Source = new Uri(_tempVideoPath);
                        mediaElement.Play();
                        Log.Information("嵌入式视频已提取并开始播放");
                    }
                    else
                    {
                        Log.Warning("无法找到嵌入式视频资源: {ResourceName}", resourceName);
                        MessageBoxFunction.ShowMessageBoxError("无法找到嵌入式视频资源");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "提取并播放视频时出错");
                MessageBoxFunction.ShowMessageBoxError("加载视频时出错", true, ex);
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Log.Debug("视频播放结束，重新开始播放");
            mediaElement.Position = TimeSpan.Zero;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                mediaElement.Play();
                Log.Debug("视频重新开始播放");
            }), DispatcherPriority.Background);
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Log.Information("重复启动警告窗口正在关闭");
            this.Hide();
            CleanupTempFile();
            Environment.Exit(0);
        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("重复启动警告窗口加载完成");
            ExtractAndPlayVideo();
        }

        private void CleanupTempFile()
        {
            try
            {
                if (!string.IsNullOrEmpty(_tempVideoPath) && File.Exists(_tempVideoPath))
                {
                    Log.Debug("清理临时视频文件: {TempVideoPath}", _tempVideoPath);
                    File.Delete(_tempVideoPath);
                    Log.Debug("临时视频文件已删除");
                }
                else
                {
                    Log.Debug("没有需要清理的临时视频文件");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "清理临时文件时发生错误");
                // 忽略清理时的错误，因为这只是临时文件
            }
        }
    }
}