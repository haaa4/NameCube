using NameCube.Setting.EasterEgg;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
<<<<<<< HEAD
using Windows.Media.Protection.PlayReady;
=======
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
using Wpf.Ui.Controls;
using Image = Wpf.Ui.Controls.Image;
using StackPanel = System.Windows.Controls.StackPanel;

namespace NameCube.Setting
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class About : Page
    {
        private int clickTimes = 0;

        public About()
        {
            InitializeComponent();
            Log.Debug("About页面初始化完成");
        }

        private async void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                clickTimes++;
                Log.Verbose("关于图标被点击，次数: {ClickTimes}", clickTimes);

                if (clickTimes >= 10)
                {
                    Log.Information("触发Debug模式入口");

                    var dialog = new Wpf.Ui.Controls.ContentDialog();
                    dialog = new ContentDialog()
                    {
                        CloseButtonText = "取消",
                        PrimaryButtonText = "确定",
                        Title = "进入Debug模式",
                        Content = new StackPanel()
                        {
                            Children =
                            {
                                new System.Windows.Controls.TextBlock()
                                {
                                    Text = "请输入对应的密码，开启对应的Debug模式"
                                },
                                new Wpf.Ui.Controls.TextBox()
                            }
                        }
                    };
                    var mainWindow = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();
                    dialog.DialogHostEx = mainWindow.RootContentDialogPresenter;
                    Wpf.Ui.Controls.ContentDialogResult contentDialogResult = await dialog.ShowAsync();

                    if (contentDialogResult == Wpf.Ui.Controls.ContentDialogResult.None)
                    {
                        Log.Debug("用户取消Debug模式输入");
                        dialog.Hide();
                    }
                    else
                    {
                        if (dialog.Content is StackPanel panel)
                        {
                            var password = panel.Children.OfType<Wpf.Ui.Controls.TextBox>().FirstOrDefault();
                            string enteredPassword = password?.Text ?? string.Empty;

                            Log.Debug("用户输入Debug密码，长度: {Length}", enteredPassword.Length);

                            switch (enteredPassword)
                            {
                                case "0d612c12d2ac33625bf3e0351b6f5e4f73829fa8":
                                    Log.Information("开启自动流程Debug模式");
                                    GlobalVariablesData.config.AutomaticProcess.debug = true;
                                    GlobalVariablesData.SaveConfig();
                                    SnackBarFunction.ShowSnackBarInSettingWindow("自动流程Debug模式已开启", ControlAppearance.Success);
                                    break;

                                case "c53d2f1a9a8499bcb477be56c31caa5c76ae60f5":
                                    Log.Information("开启开发者调试");
                                    GlobalVariablesData.config.AllSettings.debug = true;
                                    GlobalVariablesData.SaveConfig();
                                    SnackBarFunction.ShowSnackBarInSettingWindow("开发者调试已开启", ControlAppearance.Success);
                                    break;

                                case "7a7bc4496e501462270ce7f6f8023c96d32098d8":
                                    Log.Information("开启势能模式调试模式");
                                    GlobalVariablesData.config.MemoryFactorModeSettings.debug = true;
                                    GlobalVariablesData.SaveConfig();
                                    SnackBarFunction.ShowSnackBarInSettingWindow("势能模式调试已开启", ControlAppearance.Success);
                                    break;
                                //以下是彩蛋部分
                                case "philia093":
                                    Log.Information("触发彩蛋：philia093");
                                    Media media = new Media("https://launcher-webstatic.mihoyo.com/launcher-public/2025/10/31/49fab36b3317cbe36b673e9183ed22c3_4733825790845625523.webm");
                                    media.ShowDialog();
                                    break;

                                case "Columbina":
                                    Log.Information("触发彩蛋：Columbina");
                                    Media media2 = new Media("https://launcher-webstatic.mihoyo.com/launcher-public/2026/01/08/f3c44cd72c6214ed680afe5fe90b26fc_6413191254498564796.webm");
                                    media2.ShowDialog();
                                    break;

                                default:
                                    Log.Warning("Debug密码错误: {Password}", enteredPassword);
                                    SnackBarFunction.ShowSnackBarInSettingWindow("密码错误", Wpf.Ui.Controls.ControlAppearance.Caution);
                                    break;
                            }
                        }
                        else
                        {
                            Log.Warning("Debug对话框内容解析失败");
                        }
                    }
                    clickTimes = 0; // 重置点击次数
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理Debug模式入口时发生异常");
            }
        }

        private async Task LoadImageFromWebAsync(string imageUrl, Image targetImage)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // 异步获取图片字节流
                    byte[] imageData = await client.GetByteArrayAsync(imageUrl);

                    // 在内存流中创建图片
                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        // 设置源为内存流
                        bitmapImage.StreamSource = ms;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.DecodePixelWidth = 400;
                        bitmapImage.EndInit();
                        // 图片解码后，通过Dispatcher切换到UI线程更新控件
<<<<<<< HEAD
                        this.Dispatcher.Invoke(() => {
=======
                        this.Dispatcher.Invoke(() =>
                        {
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
                            targetImage.Source = bitmapImage;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning("加载头像失败：{Error}", ex);
            }
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            VersionTextBlock.Text = GlobalVariablesData.VERSION;
<<<<<<< HEAD
            if(GlobalVariablesData.config.AllSettings.DownloadWay == 0)
=======
            if (GlobalVariablesData.config.AllSettings.DownloadWay == 0)
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
                await LoadImageFromWebAsync("https://avatars.githubusercontent.com/u/172395030?v=4", HeadImage);
            else
                await LoadImageFromWebAsync("https://foruda.gitee.com/avatar/1774776926077586438/15207534_haaa4_1774776926.png!avatar200", HeadImage);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ThanksWindow thanksWindow = new ThanksWindow();
            thanksWindow.ShowDialog();
        }
    }
}