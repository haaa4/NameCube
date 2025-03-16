using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace NameCube
{
    /// <summary>
    /// setting.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class setting : Page
    {
        public class AllName
        {
            public string Index { get; set; }
            public string Name { get; set; }
        }

        public ObservableCollection<AllName> AllNames { get; set; } = new ObservableCollection<AllName>();
        Json json = new Json();
        bool CanChange = false;
        public void SaveJson()
        {
            string jsonString = JsonSerializer.Serialize(json);
            File.WriteAllText("config.json", jsonString);
        }
        public setting()
        {
            InitializeComponent();
            if (!File.Exists("config.json"))
            {
                json = new Json
                {
                    Name = new List<string>(),
                    Speech = true,
                    Dark = false,
                    Volume = 100,
                    Speed = 0,
                    Wait = false,
                };
                json.Name.Add("张三");

                SaveJson();
            }
            else
            {
                string jsonstring = File.ReadAllText("config.json");
                json = JsonSerializer.Deserialize<Json>(jsonstring);
            }
            VolumeSlider.Value = json.Volume;
            SpeedSlider.Value = json.Speed + 10;
            DarkLight.IsChecked = json.Dark;
            CanChange = true;
            BallCheck.IsChecked = json.Ball;
            StartCheck.IsChecked = json.Start;
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                json.Volume = (int)VolumeSlider.Value;
                SaveJson();
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                json.Speed = (int)SpeedSlider.Value - 10;
                SaveJson();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveJson();
        }

        private void DarkLight_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                json.Dark = DarkLight.IsChecked.Value;
                SaveJson();
                if (DarkLight.IsChecked.Value)
                {
                    Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                        Wpf.Ui.Appearance.ApplicationTheme.Dark, // Theme type
                         Wpf.Ui.Controls.WindowBackdropType.Auto,  // Background type
                         true                                      // Whether to change accents automatically
                       );
                }
                else
                {
                    Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                        Wpf.Ui.Appearance.ApplicationTheme.Light, // Theme type
                         Wpf.Ui.Controls.WindowBackdropType.Auto,  // Background type
                         true                                      // Whether to change accents automatically
                       );
                }
            }
        }

        private void BallCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                json.Ball = BallCheck.IsChecked.Value;
                SaveJson();
            }
        }
        private const string RegistryRunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "NameCube"; // 注册表中显示的名称

        /// <summary>
        /// 启用开机自启动
        /// </summary>
        public static void EnableAutoStart()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, true))
                {
                    // 获取当前应用的完整路径
                    var appPath = Process.GetCurrentProcess().MainModule.FileName;

                    // 添加注册表项（带引号防止路径中有空格）
                    key.SetValue(AppName, $"\"{appPath}\"");
                }
            }
            catch (Exception ex)
            {
                // 处理异常（如权限不足）
                Debug.WriteLine($"启用自启动失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 禁用开机自启动
        /// </summary>
        public static void DisableAutoStart()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, true))
                {
                    if (key.GetValue(AppName) != null)
                    {
                        key.DeleteValue(AppName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"禁用自启动失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查是否已启用自启动
        /// </summary>
        public static bool IsAutoStartEnabled()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, false))
                {
                    return key.GetValue(AppName) != null;
                }
            }
            catch
            {
                return false;
            }
        }


        private void StartCheck_Click(object sender, RoutedEventArgs e)
        {
            if(CanChange)
            {
                json.Start = StartCheck.IsChecked.Value;
                SaveJson();
                if(json.Start)
                {
                    EnableAutoStart();
                }
                else
                {
                    DisableAutoStart();
                }
            }
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            var executablePath = Process.GetCurrentProcess().MainModule.FileName;

            // 启动新进程
            Process.Start(new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = true
            });

            // 关闭当前应用
            Application.Current.Shutdown();
        }
    }
}
