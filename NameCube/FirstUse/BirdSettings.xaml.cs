using Masuit.Tools;
using Masuit.Tools.Logging;
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
using Serilog;  // 添加Serilog命名空间

namespace NameCube.FirstUse
{
    /// <summary>
    /// BirdSettings.xaml 的交互逻辑
    /// </summary>
    public partial class BirdSettings : Page
    {
        bool CanChange;
        public BirdSettings()
        {
            InitializeComponent();
            CanChange = false;
            Log.Information("初始化BirdSettings页面");
            Initialize();
            CanChange = true;
            Log.Information("BirdSettings页面初始化完成");
        }

        private void BallCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.StartToDo.Ball = BallCheck.IsChecked.Value;
                Log.Information("切换悬浮球启动状态: {BallStatus}", BallCheck.IsChecked.Value);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                Log.Information("开始重启应用操作");
                File.WriteAllText(Path.Combine(GlobalVariablesData.configDir, "START"), "The cake is a lie");
                Log.Information("重启标记文件已写入");
                Log.Information("程序退出，准备重启");
                System.Windows.Application.Current.Shutdown();
                Process.Start(System.Windows.Application.ResourceAssembly.Location, string.Join(" ", args.Skip(1)));
                Log.Information("新进程已启动");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "重启应用时发生错误");
            }
        }

        private void ImageIcon_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CanChange)
            {
                Log.Information("开始选择自定义图片");
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "c:\\";
                    openFileDialog.Title = "选择图片";
                    openFileDialog.Filter = "png (*.png)|*.png|所有文件 (*.*)|*.*";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        Log.Information("用户选择了图片文件: {FileName}", openFileDialog.FileName);
                        Ring.Visibility = Visibility.Visible;
                        ImageIcon.Visibility = Visibility.Collapsed;
                        CopyImage(openFileDialog.FileName);
                        GlobalVariablesData.config.BirdSettings.UseDefinedImage = true;
                        Log.Information("已切换到自定义图片模式");
                    }
                    else
                    {
                        Log.Information("用户取消了图片选择");
                    }
                }
            }
        }

        private async void CopyImage(string Filename)
        {
            Log.Information("开始复制图片: {FileName}", Filename);
            await Task.Run(() =>
            {
                try
                {
                    string targetPath = Path.Combine(GlobalVariablesData.userDataDir, "Bird_data", "Image", "image.png");
                    string targetDir = Path.GetDirectoryName(targetPath);

                    // 确保目录存在
                    if (!Directory.Exists(targetDir))
                    {
                        Directory.CreateDirectory(targetDir);
                        Log.Debug("创建图片目录: {Directory}", targetDir);
                    }

                    // 删除已存在的文件
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                        Log.Debug("删除已存在的图片文件");
                    }

                    File.Copy(Filename, targetPath);
                    Log.Information("图片复制完成: {Source} -> {Target}", Filename, targetPath);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        try
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(targetPath);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            ImageIcon.Source = bitmap;
                            Ring.Visibility = Visibility.Collapsed;
                            ImageIcon.Visibility = Visibility.Visible;
                            ChangeBird();
                            Log.Debug("UI图片更新完成");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "更新UI图片时发生错误");
                        }
                    }));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "复制图片时发生错误");
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        Ring.Visibility = Visibility.Collapsed;
                        ImageIcon.Visibility = Visibility.Visible;
                    }));
                }
                return;
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                Log.Information("恢复默认图片");
                ImageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BallPicture.png"));
                GlobalVariablesData.config.BirdSettings.UseDefinedImage = false;
                ChangeBird();
                Log.Information("已恢复到默认图片");
            }
        }

        private void ABSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                int value = ABSlider.Value.ToInt32();
                GlobalVariablesData.config.BirdSettings.AdsorbValue = value;
                Log.Information("吸附值更改为: {AdsorbValue}", value);
                ChangeBird();
            }
        }

        private void AutoAdsorb_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                bool isChecked = AutoAdsorb.IsChecked.Value;
                GlobalVariablesData.config.BirdSettings.AutoAbsord = isChecked;
                Log.Information("自动吸附状态更改为: {AutoAdsorb}", isChecked);
                ChangeBird();
            }
        }

        private void Diaphaneity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                int value = Diaphaneity.Value.ToInt32();
                GlobalVariablesData.config.BirdSettings.diaphaneity = value;
                Log.Information("透明度更改为: {Diaphaneity}%", value);
                ChangeBird();
            }
        }

        private void Initialize()
        {
            try
            {
                Log.Information("开始初始化BirdSettings控件");
                BallCheck.IsChecked = GlobalVariablesData.config.StartToDo.Ball;
                StartWayComboBox.SelectedIndex = GlobalVariablesData.config.BirdSettings.StartWay;

                if (GlobalVariablesData.config.BirdSettings.UseDefinedImage)
                {
                    Log.Debug("加载自定义图片");
                    string imagePath = Path.Combine(GlobalVariablesData.userDataDir, "Bird_data", "Image", "image.png");

                    if (File.Exists(imagePath))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imagePath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        ImageIcon.Source = bitmap;
                        Log.Debug("自定义图片加载成功");
                    }
                    else
                    {
                        Log.Warning("自定义图片文件不存在: {ImagePath}", imagePath);
                        GlobalVariablesData.config.BirdSettings.UseDefinedImage = false;
                    }
                }
                else
                {
                    Log.Debug("使用默认图片");
                }

                if (GlobalVariablesData.config.BirdSettings.diaphaneity == 0)
                {
                    GlobalVariablesData.config.BirdSettings.diaphaneity = 100;
                    Log.Debug("修复透明度为默认值100%");
                }

                ABSlider.Value = GlobalVariablesData.config.BirdSettings.AdsorbValue;
                AutoAdsorb.IsChecked = GlobalVariablesData.config.BirdSettings.AutoAbsord;
                Diaphaneity.Value = GlobalVariablesData.config.BirdSettings.diaphaneity;
                StartLocationWay.SelectedIndex = GlobalVariablesData.config.BirdSettings.StartLocationWay;
                BallWidth.Value = GlobalVariablesData.config.BirdSettings.Width;
                BallHeight.Value = GlobalVariablesData.config.BirdSettings.Height;

                Log.Information("BirdSettings控件初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化BirdSettings控件时发生错误");
            }
        }

        private void ChangeBird()
        {
            try
            {
                Log.Information("开始更新Bird窗口");
                var Bird = System.Windows.Application.Current.Windows.OfType<Bird>().FirstOrDefault();

                if (Bird == null)
                {
                    Log.Debug("未找到现有Bird窗口，创建新实例");
                    Bird = new Bird();
                }
                else
                {
                    Log.Debug("找到现有Bird窗口，重新初始化");
                }

                Bird.Initialize();
                Log.Information("Bird窗口更新完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "更新Bird窗口时发生错误");
            }
        }

        private void StartWayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CanChange)
            {
                int selectedIndex = StartWayComboBox.SelectedIndex;
                GlobalVariablesData.config.BirdSettings.StartWay = selectedIndex;
                Log.Information("启动方式更改为索引: {StartWay}", selectedIndex);
            }
        }

        private void StartLocationWay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CanChange)
            {
                int selectedIndex = StartLocationWay.SelectedIndex;
                GlobalVariablesData.config.BirdSettings.StartLocationWay = selectedIndex;
                Log.Information("启动位置方式更改为索引: {StartLocationWay}", selectedIndex);
            }
        }

        private void BallWidthHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                int width = BallWidth.Value.ToInt32();
                int height = BallHeight.Value.ToInt32();

                GlobalVariablesData.config.BirdSettings.Width = width;
                GlobalVariablesData.config.BirdSettings.Height = height;

                Log.Information("悬浮球尺寸更改为: {Width}x{Height}", width, height);

                var Bird = System.Windows.Application.Current.Windows.OfType<Bird>().FirstOrDefault();

                if (Bird == null)
                {
                    Log.Debug("未找到Bird窗口，创建新实例");
                    Bird = new Bird();
                }

                Bird.ShowReRectangle();
                Log.Debug("已请求更新悬浮球矩形");
            }
        }
    }
}