using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;  // 正确的命名空间
using System.Windows.Media.Imaging;

namespace NameCube.Mode.HomeChildrenPage
{
    public partial class Photos : Page
    {
        private Storyboard storyBoard;
        private CancellationTokenSource _cts;

        public Photos()
        {
            Log.Debug("Photos开始初始化");
            InitializeComponent();

            storyBoard = FindResource("ShowPhoto") as Storyboard;
            if (storyBoard == null)
                throw new InvalidOperationException("未找到 ShowPhoto 资源");

            storyBoard.Completed += (sender, e) =>
            {
                Image1.Source = Image2.Source;
                Image1.Opacity = 1;
                Image2.Visibility = Visibility.Hidden;
            };

            Log.Debug("Photos初始化完成");
        }

        public List<string> photosPath = new List<string>
        {
            "NameCube.Image.HeadMap.Photo1.png",
            "NameCube.Image.HeadMap.Photo2.png",
            "NameCube.Image.HeadMap.Photo3.png"
        };

        private int photoIndex = 0;

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource();
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    if (photoIndex >= photosPath.Count)
                    {
                        photoIndex = 0;
                    }

                    var assembly = Assembly.GetExecutingAssembly();
                    string resourceName = photosPath[photoIndex];
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = stream;
                            bitmapImage.EndInit();
                            bitmapImage.Freeze(); // 跨线程安全，但本例中仍用于 UI 线程
                            ChangePhoto(bitmapImage);
                        }
                    }

                    photoIndex++;
                    await Task.Delay(10000, _cts.Token).ConfigureAwait(true); // 保留 UI 上下文
                }
            }
            catch (TaskCanceledException)
            {
                // 循环正常结束
            }
        }

        private void ChangePhoto(ImageSource image)
        {
            Image2.Source = image;
            storyBoard?.Begin();
            Log.Debug("切换照片");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}