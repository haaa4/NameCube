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
                var bird = Application.Current.Windows.OfType<Bird>().FirstOrDefault();
                if(bird==null)
                {
                    bird=new Bird();
                }
                if(GlobalVariables.json.StartToDo.Ball)
                {
                    bird.Show();
                }
                else
                {
                    bird.Hide();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            File.WriteAllText(Path.Combine(GlobalVariables.configDir, "START"), "The cake is a lie");
            System.Windows.Application.Current.Shutdown();
            Process.Start(System.Windows.Application.ResourceAssembly.Location, string.Join(" ", args.Skip(1)));
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
                    Ring.Visibility = Visibility.Collapsed;
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
                ImageIcon.Source = new BitmapImage(new Uri("pack://application:,,,/BallPicture.png"));
                GlobalVariables.json.BirdSettings.UseDefinedImage = false;
                GlobalVariables.SaveJson();
                ChangeBird();
            }
        }

        private void ABSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.BirdSettings.AdsorbValue = ABSlider.Value.ToInt32();
                GlobalVariables.SaveJson();
                ChangeBird();
            }
        }

        private void AutoAdsorb_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.BirdSettings.AutoAbsord = AutoAdsorb.IsChecked.Value;
                GlobalVariables.SaveJson();
                ChangeBird();
            }
        }

        private void Diaphaneity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.BirdSettings.diaphaneity = Diaphaneity.Value.ToInt32();
                GlobalVariables.SaveJson();
                ChangeBird();
            }
        }
        private void Initialize()
        {
            BallCheck.IsChecked = GlobalVariables.json.StartToDo.Ball;
            StartWayComboBox.SelectedIndex = GlobalVariables.json.BirdSettings.StartWay;
            if (GlobalVariables.json.BirdSettings.UseDefinedImage)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(Path.Combine(GlobalVariables.configDir, "Bird_data", "Image", "image.png"));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ImageIcon.Source = bitmap;
            }
            if (GlobalVariables.json.BirdSettings.diaphaneity == 0)
            {
                GlobalVariables.json.BirdSettings.diaphaneity = 100;
            }
            ABSlider.Value = GlobalVariables.json.BirdSettings.AdsorbValue;
            AutoAdsorb.IsChecked = GlobalVariables.json.BirdSettings.AutoAbsord;
            Diaphaneity.Value = GlobalVariables.json.BirdSettings.diaphaneity;
            StartLocationWay.SelectedIndex = GlobalVariables.json.BirdSettings.StartLocationWay;
            BallWidth.Value = GlobalVariables.json.BirdSettings.Width;
            BallHeight.Value = GlobalVariables.json.BirdSettings.Height;
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

        private void StartWayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.BirdSettings.StartWay = StartWayComboBox.SelectedIndex;
                GlobalVariables.SaveJson();
            }
        }

        private void StartLocationWay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.BirdSettings.StartLocationWay = StartLocationWay.SelectedIndex;
                GlobalVariables.SaveJson();
            }
        }

        private void BallWidthHeight_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.BirdSettings.Width = BallWidth.Value.ToInt32();
                GlobalVariables.json.BirdSettings.Height = BallHeight.Value.ToInt32();
                GlobalVariables.SaveJson();
                var Bird = System.Windows.Application.Current.Windows.OfType<Bird>().FirstOrDefault();

                if (Bird == null)
                {
                    // 创建新实例
                    Bird = new Bird();
                }

                // 确保窗口可见并激活
                Bird.ShowReRectangle();
            }
        }
    }
}
