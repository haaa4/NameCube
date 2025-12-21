using System.Diagnostics;
using System;
using System.Windows;
using System.Windows.Controls;
using Serilog; // 添加Serilog引用

namespace NameCube.Setting
{
    /// <summary>
    /// Speech.xaml 的交互逻辑
    /// </summary>
    public partial class Speech : Page
    {
        private static readonly ILogger _logger = Log.ForContext<Speech>(); // 添加Serilog日志实例
        bool CanChange;

        public Speech()
        {
            InitializeComponent();
            _logger.Debug("Speech 页面初始化开始");

            CanChange = false;
            VolumeSlider.Value = GlobalVariables.json.AllSettings.Volume;
            SpeedSlider.Value = GlobalVariables.json.AllSettings.Speed + 10;
            SystemSpeechCheck.IsChecked = GlobalVariables.json.AllSettings.SystemSpeech;
            SpeedSlider.IsEnabled = !GlobalVariables.json.AllSettings.SystemSpeech;
            VolumeSlider.IsEnabled = !GlobalVariables.json.AllSettings.SystemSpeech;
            CanChange = true;

            _logger.Information("语音设置加载完成，音量: {Volume}, 语速: {Speed}, 系统语音: {SystemSpeech}",
                GlobalVariables.json.AllSettings.Volume,
                GlobalVariables.json.AllSettings.Speed,
                GlobalVariables.json.AllSettings.SystemSpeech);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.AllSettings.Volume = (int)VolumeSlider.Value;
                GlobalVariables.SaveJson();
                _logger.Debug("音量设置修改为: {Volume}", (int)VolumeSlider.Value);
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.AllSettings.Speed = (int)SpeedSlider.Value - 10;
                GlobalVariables.SaveJson();
                _logger.Debug("语速设置修改为: {Speed}", (int)SpeedSlider.Value - 10);
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
                _logger.Information("系统语音设置修改为: {SystemSpeech}", SystemSpeechCheck.IsChecked.Value);
            }
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("尝试打开系统朗读人设置");
            try
            {
                // 使用系统协议直接打开 "设置" 应用中的朗读人界面
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:easeofaccess-narrator", // Win10/11 专用 URI
                    UseShellExecute = true // 必须启用 Shell 执行
                });
                _logger.Debug("成功打开系统朗读人设置");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "无法打开系统朗读人设置界面");
                Console.WriteLine($"无法打开设置界面: {ex.Message}");
            }
        }
    }
}