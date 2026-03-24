using System;
using System.Collections.Generic;
using System.Linq;
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
            VideoUrl= url;
            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置视频源
            VideoPlayer.Source = new Uri(VideoUrl);
        }

        // 视频成功打开时触发
        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
             if (VideoPlayer.NaturalVideoWidth > 0 && VideoPlayer.NaturalVideoHeight > 0)
            {
                this.Width = VideoPlayer.NaturalVideoWidth;
                this.Height = VideoPlayer.NaturalVideoHeight;
            }
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
    }
}
