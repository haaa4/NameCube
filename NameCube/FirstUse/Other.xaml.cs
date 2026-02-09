using Masuit.Tools.Logging;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Serilog;  // 添加Serilog命名空间

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
            Log.Information("Other页面初始化");

            bool isStartup = IsStartupApplication(AppName);
            StartCheck.IsChecked = isStartup;
            TopCheck.IsChecked = GlobalVariablesData.config.AllSettings.Top;

            Log.Information("开机自启动状态: {StartupStatus}", isStartup);
            Log.Information("窗口置顶状态: {TopStatus}", GlobalVariablesData.config.AllSettings.Top);

            if (GlobalVariablesData.config.AllSettings.NameCubeMode == 1)
            {
                Log.Debug("传统窗口模式，隐藏悬浮球相关设置");
                StartActionCard.Visibility = Visibility.Collapsed;
                TopActionCard.Visibility = Visibility.Collapsed;
            }

            CanChange = true;
            Log.Information("Other页面初始化完成");
        }

        private void StartCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                try
                {
                    if (StartCheck.IsChecked.Value)
                    {
                        Log.Information("用户启用开机自启动");
                        EnableAutoStart();
                    }
                    else
                    {
                        Log.Information("用户禁用开机自启动");
                        DisableAutoStart();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "更改开机自启动设置时发生错误");
                }
            }
        }

        private const string RegistryRunPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "NameCube";

        /// <summary>
        /// 启用开机自启动
        /// </summary>
        public static void EnableAutoStart()
        {
            try
            {
                Log.Information("开始启用开机自启动");
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, true))
                {
                    var appPath = Process.GetCurrentProcess().MainModule.FileName;
                    Log.Debug("应用路径: {AppPath}", appPath);

                    key.SetValue(AppName, $"\"{appPath}\"");
                    Log.Information("开机自启动已成功启用");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "启用开机自启动失败");
            }
        }

        /// <summary>
        /// 禁用开机自启动
        /// </summary>
        public static void DisableAutoStart()
        {
            try
            {
                Log.Information("开始禁用开机自启动");
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryRunPath, true))
                {
                    if (key.GetValue(AppName) != null)
                    {
                        key.DeleteValue(AppName);
                        Log.Information("开机自启动已成功禁用");
                    }
                    else
                    {
                        Log.Warning("注册表中未找到自启动项，无需删除");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "禁用开机自启动失败");
            }
        }

        private void TopCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                try
                {
                    bool isTop = TopCheck.IsChecked.Value;
                    GlobalVariablesData.config.AllSettings.Top = isTop;
                    Log.Information("窗口置顶状态更改为: {TopStatus}", isTop);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "更改窗口置顶状态时发生错误");
                }
            }
        }

        static bool IsStartupApplication(string appName)
        {
            try
            {
                Log.Debug("检查开机自启动状态，应用名: {AppName}", appName);
                using (RegistryKey runKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
                {
                    if (runKey != null)
                    {
                        string[] valueNames = runKey.GetValueNames();
                        foreach (string valueName in valueNames)
                        {
                            if (valueName.EndsWith(appName, StringComparison.OrdinalIgnoreCase))
                            {
                                Log.Debug("在自启动项中找到: {ValueName}", valueName);
                                return true;
                            }
                        }
                        Log.Debug("未在自启动项中找到应用");
                    }
                    else
                    {
                        Log.Warning("无法打开注册表自启动项");
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "检查开机自启动状态时发生错误");
                return false;
            }
        }
    }
}