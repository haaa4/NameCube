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

namespace NameCube
{
    /// <summary>
    /// RepeatWarning.xaml 的交互逻辑
    /// </summary>
    public partial class RepeatWarning
    {
        public RepeatWarning()
        {
            InitializeComponent();
            if (GlobalVariables.json.AllSettings.NameCubeMode == 1)
            {
                Text.Text = "点鸣魔方已经启动，请勿再次启动";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            CleanupTempFile();
            Environment.Exit(0);
        }
        private string _tempVideoPath;
        private void ExtractAndPlayVideo()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resourceName = "NameCube.Video.RepeatWarningVideo.mp4";

                if (assembly.GetManifestResourceStream(resourceName) == null)
                {
                    resourceName = assembly.GetName().Name + ".Video.RepeatWarningVideo.mp4";
                }

                using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream != null)
                    {
                        _tempVideoPath = System.IO.Path.GetTempFileName() + ".mp4";

                        using (FileStream fileStream = File.Create(_tempVideoPath))
                        {
                            resourceStream.CopyTo(fileStream);
                        }

                        mediaElement.Source = new Uri(_tempVideoPath);
                        mediaElement.Play();
                    }
                    else
                    {
                        MessageBoxFunction.ShowMessageBoxError("无法找到嵌入式视频资源");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError("加载视频时出错",true,ex);
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = TimeSpan.Zero;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                mediaElement.Play();
            }), DispatcherPriority.Background);
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            CleanupTempFile();
            Environment.Exit(0);
        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ExtractAndPlayVideo();
        }
        private void CleanupTempFile()
        {
            try
            {
                if (!string.IsNullOrEmpty(_tempVideoPath) && File.Exists(_tempVideoPath))
                {
                    File.Delete(_tempVideoPath);
                }
            }
            catch
            {
                // 忽略清理时的错误
            }
        }
    }
}
