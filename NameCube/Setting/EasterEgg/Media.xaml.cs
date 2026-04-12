using System;
using System.Windows;

namespace NameCube.Setting.EasterEgg
{
    /// <summary>
    /// Media.xaml 的交互逻辑
    /// </summary>
    public partial class Media : Window
    {
        // 视频 URL（可根据需要修改）
        private string VideoUrl;

        public Media(string url)
        {
            InitializeComponent();
            VideoUrl = url;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD

=======
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
        }

        // 视频成功打开时触发
        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
<<<<<<< HEAD

=======
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
        }

        // 视频播放失败时触发
        private void VideoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show($"视频播放失败: {e.ErrorException?.Message}\n请检查网络或视频格式是否支持。",
                            "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            VideoPlayer.Source = null;
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Visibility = Visibility.Collapsed;
            VideoPlayer.Source = new Uri(VideoUrl);
        }
    }
}