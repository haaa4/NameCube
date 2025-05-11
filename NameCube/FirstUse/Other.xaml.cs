using Masuit.Tools.Logging;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.FirstUse
{
    /// <summary>
    /// Other.xaml 的交互逻辑
    /// </summary>
    public partial class Other : Page
    {
        bool CanChange;
        public Other()
        {
            InitializeComponent();
            CanChange = false;
            StartCheck.IsChecked = IsStartupApplication(AppName);
            TopCheck.IsChecked = GlobalVariables.json.AllSettings.Top;
            if(GlobalVariables.json.AllSettings.NameCubeMode==1)
            {
                StartActionCard.Visibility = Visibility.Collapsed;
                TopActionCard.Visibility = Visibility.Collapsed;
            }
            CanChange = true;
        }
        private void StartCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                if (StartCheck.IsChecked.Value)
                {
                    EnableAutoStart();
                }
                else
                {
                    DisableAutoStart();
                }
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

        private void TopCheck_Click(object sender, RoutedEventArgs e)
        {
            if(CanChange)
            {
                GlobalVariables.json.AllSettings.Top = TopCheck.IsChecked.Value;
            }
        }
        static bool IsStartupApplication(string appName)
        {
            try
            {
                using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (runKey != null)
                    {
                        string[] valueNames = runKey.GetValueNames();
                        foreach (string valueName in valueNames)
                        {
                            if (valueName.EndsWith(appName, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError(ex.Message, false);
                LogManager.Error(ex);
                return false;
            }
        }

    }
}
