using Masuit.Tools;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using Application = System.Windows.Application;
using Serilog; // 添加Serilog引用

namespace NameCube.Setting
{
    /// <summary>
    /// BirdSettings.xaml 的交互逻辑
    /// </summary>
    public partial class BirdSettings : Page
    {
        private static readonly ILogger _logger = Log.ForContext<BirdSettings>(); // 添加Serilog日志实例

        bool CanChange;
        public BirdSettings()
        {
            InitializeComponent();
            _logger.Debug("BirdSettings 页面初始化开始");

            CanChange = false;
            Initialize();
            CanChange = true;

            _logger.Information("悬浮球设置加载完成");
        }

        private void BallCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.StartToDo.Ball = BallCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                _logger.Information("悬浮球显示状态修改为: {Ball}", BallCheck.IsChecked.Value);

                var bird = Application.Current.Windows.OfType<Bird>().FirstOrDefault();
                if (bird == null)
                {
                    bird = new Bird();
                }

                if (GlobalVariablesData.config.StartToDo.Ball)
                {
                    bird.Show();
                    _logger.Debug("显示悬浮球窗口");
                }
                else
                {
                    bird.Hide();
                    _logger.Debug("隐藏悬浮球窗口");
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("重启悬浮球功能");
            string[] args = Environment.GetCommandLineArgs();
            File.WriteAllText(Path.Combine(GlobalVariablesData.configDir, "START"), "The cake is a lie");

            _logger.Information("程序退出以重启悬浮球");
            Application.Current.Shutdown();
            Process.Start(Application.ResourceAssembly.Location, string.Join(" ", args.Skip(1)));
        }

        private void ImageIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CanChange)
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "c:\\";
                    openFileDialog.Title = "选择图片";
                    openFileDialog.Filter = "png (*.png)|*.png|所有文件 (*.*)|*.*";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _logger.Information("选择悬浮球自定义图片: {FilePath}", openFileDialog.FileName);
                        Ring.Visibility = Visibility.Visible;
                        ImageIcon.Visibility = Visibility.Collapsed;
                        CopyImage(openFileDialog.FileName);
                        GlobalVariablesData.config.BirdSettings.UseDefinedImage = true;
                        GlobalVariablesData.SaveConfig();
                    }
                }
            }
        }

        private async void CopyImage(string Filename)
        {
            _logger.Debug("开始复制图片: {Filename}", Filename);

            await Task.Run(() =>
            {
                try
                {
                    File.Delete(Path.Combine(GlobalVariablesData.userDataDir, "Bird_data", "Image", "image.png"));
                    File.Copy(Filename, Path.Combine(GlobalVariablesData.userDataDir, "Bird_data", "Image", "image.png"));

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(Path.Combine(GlobalVariablesData.userDataDir, "Bird_data", "Image", "image.png"));
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        ImageIcon.Source = bitmap;
                        Ring.Visibility = Visibility.Collapsed;
                        ImageIcon.Visibility = Visibility.Visible;
                        ChangeBird();
                    }));

                    _logger.Information("图片复制完成");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "复制图片时发生异常");
                }
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                ImageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BallPicture.png"));
                GlobalVariablesData.config.BirdSettings.UseDefinedImage = false;
                GlobalVariablesData.SaveConfig();
                _logger.Information("恢复悬浮球默认图片");
                ChangeBird();
            }
        }

        private void ABSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.BirdSettings.AdsorbValue = ABSlider.Value.ToInt32();
                GlobalVariablesData.SaveConfig();
                _logger.Debug("吸附值修改为: {Value}", ABSlider.Value.ToInt32());
                ChangeBird();
            }
        }

        private void AutoAdsorb_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.BirdSettings.AutoAbsord = AutoAdsorb.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                _logger.Information("自动吸附修改为: {AutoAbsorb}", AutoAdsorb.IsChecked.Value);
                ChangeBird();
            }
        }

        private void Diaphaneity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.BirdSettings.diaphaneity = Diaphaneity.Value.ToInt32();
                GlobalVariablesData.SaveConfig();
                _logger.Debug("透明度修改为: {Diaphaneity}", Diaphaneity.Value.ToInt32());
                ChangeBird();
            }
        }

        private void Initialize()
        {
            BallCheck.IsChecked = GlobalVariablesData.config.StartToDo.Ball;
            StartWayComboBox.SelectedIndex = GlobalVariablesData.config.BirdSettings.StartWay;

            if (GlobalVariablesData.config.BirdSettings.UseDefinedImage)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(Path.Combine(GlobalVariablesData.userDataDir, "Bird_data", "Image", "image.png"));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ImageIcon.Source = bitmap;
                _logger.Debug("加载自定义悬浮球图片");
            }

            if (GlobalVariablesData.config.BirdSettings.diaphaneity == 0)
            {
                GlobalVariablesData.config.BirdSettings.diaphaneity = 100;
                _logger.Debug("透明度为0，重置为100");
            }

            ABSlider.Value = GlobalVariablesData.config.BirdSettings.AdsorbValue;
            AutoAdsorb.IsChecked = GlobalVariablesData.config.BirdSettings.AutoAbsord;
            Diaphaneity.Value = GlobalVariablesData.config.BirdSettings.diaphaneity;
            StartLocationWay.SelectedIndex = GlobalVariablesData.config.BirdSettings.StartLocationWay;
            BallWidth.Value = GlobalVariablesData.config.BirdSettings.Width;
            BallHeight.Value = GlobalVariablesData.config.BirdSettings.Height;
            LongPressMisjudgment.Value = GlobalVariablesData.config.BirdSettings.LongPressMisjudgment;
        }

        private void ChangeBird()
        {
            var Bird = Application.Current.Windows.OfType<Bird>().FirstOrDefault();

            if (Bird == null)
            {
                Bird = new Bird();
                _logger.Debug("创建新的悬浮球窗口");
            }

            Bird.Initialize();
            _logger.Debug("悬浮球窗口已重新初始化");
        }

        private void StartWayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.BirdSettings.StartWay = StartWayComboBox.SelectedIndex;
                GlobalVariablesData.SaveConfig();
                _logger.Information("悬浮球启动方式修改为: {StartWay}", StartWayComboBox.SelectedIndex);
            }
        }

        private void StartLocationWay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.BirdSettings.StartLocationWay = StartLocationWay.SelectedIndex;
                GlobalVariablesData.SaveConfig();
                _logger.Information("悬浮球启动位置修改为: {StartLocationWay}", StartLocationWay.SelectedIndex);
            }
        }

        private void BallWidthHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.BirdSettings.Width = BallWidth.Value.ToInt32();
                GlobalVariablesData.config.BirdSettings.Height = BallHeight.Value.ToInt32();
                GlobalVariablesData.SaveConfig();
                _logger.Information("悬浮球尺寸修改为: {Width}x{Height}", BallWidth.Value.ToInt32(), BallHeight.Value.ToInt32());

                var Bird = Application.Current.Windows.OfType<Bird>().FirstOrDefault();

                if (Bird == null)
                {
                    Bird = new Bird();
                }

                Bird.ShowReRectangle();
            }
        }

        private void LongPressMisjudgment_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.BirdSettings.LongPressMisjudgment = (int)LongPressMisjudgment.Value;
                GlobalVariablesData.SaveConfig();
                _logger.Debug("长按误判阈值修改为: {Value}", (int)LongPressMisjudgment.Value);
            }
        }
    }
}