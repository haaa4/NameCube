using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NameCube.Mode.HomeChildrenPage
{
    public partial class Photos : Page
    {
        private Storyboard storyBoard;
        private CancellationTokenSource _cts;
        private DispatcherTimer _timer;          // 用于 UI 线程定时切换

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
            "NameCube.Image.HeadMap.Photo3.png",
            "online"//占位符，表示该图片从网络获取
        };

        private int photoIndex = 0;
        private BitmapImage currentPhoto = null;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource();
            if(GlobalVariablesData.config.AllSettings.DownloadWay==0)
            {
                _ = LoadImageFromWebAsync("https://raw.githubusercontent.com/haaa4/NameCube/refs/heads/1.3/NameCube/Image/HeadMap/Online.png");
            }
            else
            {
                _ = LoadImageFromWebAsync("https://gitee.com/haaa4/NameCube/raw/1.3/NameCube/Image/HeadMap/Online.png");
            }

                // 使用 DispatcherTimer 在 UI 线程上定时切换本地图片
                _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += OnTimerTick;
            _timer.Start();

            // 立即显示第一张图片
            LoadNextPhoto();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            LoadNextPhoto();
        }

        private void LoadNextPhoto()
        {
            if (photoIndex >= photosPath.Count)
                photoIndex = 0;
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = photosPath[photoIndex];
            if(resourceName=="online")
            {
                if (currentPhoto != null)
                {
                    ChangePhoto(currentPhoto);
                }
                else
                {
                    Log.Debug("网络图片尚未加载完成，无法切换");
                }
                photoIndex++;
                return;
            }
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // 可选，保证跨线程安全（虽已在 UI 线程，但无副作用）
                    ChangePhoto(bitmapImage);
                }
            }

            photoIndex++;
        }

        private void ChangePhoto(ImageSource image)
        {
            // 此方法由 DispatcherTimer 调用，已在 UI 线程，无需额外 Invoke
            Image2.Source = image;
            storyBoard?.Begin();
            Log.Debug("切换照片");
        }

        private async Task LoadImageFromWebAsync(string imageUrl)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] imageData = await client.GetByteArrayAsync(imageUrl);
                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        // 回到 UI 线程保存引用（后续如需展示网络图片可在此处直接更新）
                        await Dispatcher.InvokeAsync(() => { currentPhoto = bitmapImage; });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning("加载头像失败：{Error}", ex);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();
            _timer = null;
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}