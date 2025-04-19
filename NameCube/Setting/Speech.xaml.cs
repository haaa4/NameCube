using System.Diagnostics;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.Setting
{
    /// <summary>
    /// Speech.xaml 的交互逻辑
    /// </summary>
    public partial class Speech : Page
    {
        bool CanChange;
        public Speech()
        {
            InitializeComponent();
            CanChange = false;
            VolumeSlider.Value = GlobalVariables.json.AllSettings.Volume;
            SpeedSlider.Value = GlobalVariables.json.AllSettings.Speed + 10;
            SystemSpeechCheck.IsChecked = GlobalVariables.json.AllSettings.SystemSpeech;
            SpeedSlider.IsEnabled = !GlobalVariables.json.AllSettings.SystemSpeech;
            VolumeSlider.IsEnabled = !GlobalVariables.json.AllSettings.SystemSpeech;
            CanChange = true;
        }
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.AllSettings.Volume = (int)VolumeSlider.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.AllSettings.Speed = (int)SpeedSlider.Value - 10;
                GlobalVariables.SaveJson();
            }
        }

        private void SystemSpeechCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.AllSettings.SystemSpeech = SystemSpeechCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
                SpeedSlider.IsEnabled = !GlobalVariables.json.AllSettings.SystemSpeech;
                VolumeSlider.IsEnabled = !GlobalVariables.json.AllSettings.SystemSpeech;
            }
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 使用系统协议直接打开 "设置" 应用中的朗读人界面
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:easeofaccess-narrator", // Win10/11 专用 URI
                    UseShellExecute = true // 必须启用 Shell 执行
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法打开设置界面: {ex.Message}");
            }
        }
    }
}
