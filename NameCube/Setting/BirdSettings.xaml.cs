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

namespace NameCube.Setting
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
            Initialize();
            CanChange = true;
        }
        private void BallCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.StartToDo.Ball = BallCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
                var snackbar = new Snackbar(SnackbarPresenterHost)
                {
                    Content = "设置已保存,请重启软件", // 设置提示内容
                    Title="提示",
                    Appearance=ControlAppearance.Info,
                    Timeout = TimeSpan.FromSeconds(10) // 显示时长
                };
                snackbar.Show();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            File.WriteAllText(Path.Combine(GlobalVariables.configDir, "START"), "The cake is a lie");
            System.Windows.Application.Current.Shutdown();
            Process.Start(System.Windows.Application.ResourceAssembly.Location, string.Join(" ", args.Skip(1)));
        }

        
         //巨史警告
         


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.BirdSettings.StartWay = 0;
            GlobalVariables.SaveJson();
            DropWay.Content = "左键";
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.BirdSettings.StartWay = 1;
            GlobalVariables.SaveJson();
            DropWay.Content = "右键";
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.BirdSettings.StartWay = 2;
            GlobalVariables.SaveJson();
            DropWay.Content = "长按";
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.BirdSettings.StartWay = 3;
            GlobalVariables.SaveJson();
            DropWay.Content = "左键+右键";
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.BirdSettings.StartWay = 4;
            GlobalVariables.SaveJson();
            DropWay.Content = "长按+右键";
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
                        Ring.Visibility = Visibility.Visible;
                        ImageIcon.Visibility = Visibility.Collapsed;
                        CopyImage(openFileDialog.FileName);
                        GlobalVariables.json.BirdSettings.UseDefinedImage = true;
                        GlobalVariables.SaveJson();
                    }
                }
            }
        }
        private async Task CopyImage(string Filename)
        {
            await Task.Run(() =>
            {

                File.Delete(Path.Combine(GlobalVariables.configDir, "Bird_data", "Image", "image.png"));
                File.Copy(Filename, Path.Combine(GlobalVariables.configDir, "Bird_data", "Image", "image.png"));
                this.Dispatcher.Invoke(new Action(() =>
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(Path.Combine(GlobalVariables.configDir, "Bird_data", "Image", "image.png"));
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ImageIcon.Source = bitmap;
                    Ring.Visibility= Visibility.Collapsed;
                    ImageIcon.Visibility = Visibility.Visible;
                    ChangeBird();
                }));
                return;
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                ImageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/icon.ico"));
                GlobalVariables.json.BirdSettings.UseDefinedImage = false;
                GlobalVariables.SaveJson();
                ChangeBird() ;
            }
        }

        private void ABSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.BirdSettings.AdsorbValue = ABSlider.Value.ToInt32();
                GlobalVariables.SaveJson();
                ChangeBird() ;  
            }
        }

        private void AutoAdsorb_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.BirdSettings.AutoAbsord = AutoAdsorb.IsChecked.Value;
                GlobalVariables.SaveJson();
                ChangeBird() ;
            }
        }

        private void Diaphaneity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.BirdSettings.diaphaneity = Diaphaneity.Value.ToInt32();
                GlobalVariables.SaveJson();
                ChangeBird() ;
            }
        }
        private void Initialize()
        {
            BallCheck.IsChecked = GlobalVariables.json.StartToDo.Ball;
            if (GlobalVariables.json.BirdSettings.StartWay == 0)
            {
                DropWay.Content = "左键";
            }
            else if (GlobalVariables.json.BirdSettings.StartWay == 1)
            {
                DropWay.Content = "右键";
            }
            else if (GlobalVariables.json.BirdSettings.StartWay == 2)
            {
                DropWay.Content = "长按";
            }
            else if (GlobalVariables.json.BirdSettings.StartWay == 3)
            {
                DropWay.Content = "左键+右键";

            }
            else if (GlobalVariables.json.BirdSettings.StartWay == 4)
            {
                DropWay.Content = "右键+长按";
            }
            if (GlobalVariables.json.BirdSettings.UseDefinedImage) 
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(Path.Combine(GlobalVariables.configDir, "Bird_data", "Image", "image.png"));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ImageIcon.Source = bitmap;
            }
            ABSlider.Value = GlobalVariables.json.BirdSettings.AdsorbValue;
            AutoAdsorb.IsChecked = GlobalVariables.json.BirdSettings.AutoAbsord;
            Diaphaneity.Value=GlobalVariables.json.BirdSettings.diaphaneity;
        }
        private void ChangeBird()
        {
            var Bird = System.Windows.Application.Current.Windows.OfType<Bird>().FirstOrDefault();

            if (Bird == null)
            {
                // 创建新实例
                Bird = new Bird();
            }

            // 确保窗口可见并激活
            Bird.Initialize();
        }
    }
}
