using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using NameCube.Function;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using File = System.IO.File;
using Serilog; // 添加Serilog引用

namespace NameCube.Setting
{
    /// <summary>
    /// Other.xaml 的交互逻辑
    /// </summary>
    public partial class Other : Page
    {
        private static readonly ILogger _logger = Log.ForContext<Other>(); // 添加Serilog日志实例

        bool CanChange;
        public Other()
        {
            InitializeComponent();
            _logger.Debug("Other 页面初始化开始");

            CanChange = false;
            StartCheck.IsChecked = IsStartupApplication("NameCube");
            TopCheck.IsChecked = GlobalVariablesData.config.AllSettings.Top;
            ModeCombox.SelectedIndex = GlobalVariablesData.config.AllSettings.NameCubeMode;
            DisabledAnimationCheck.IsChecked = GlobalVariablesData.config.AllSettings.DisableTheDisplayAnimationOfTheMainWindow;
            MaxSizeCheck.IsChecked = GlobalVariablesData.config.AllSettings.DefaultToMaximumSize;

            if (MaxSizeCheck.IsChecked.Value)
            {
                DisabledAnimationCardContorl.IsEnabled = false;
            }
            else
            {
                DisabledAnimationCardContorl.IsEnabled = true;
            }

            if (GlobalVariablesData.config.AllSettings.Recommend == "None")
            {
                RecommendCheck.IsChecked = true;
            }
            else
            {
                RecommendCheck.IsChecked = false;
            }

            if (GlobalVariablesData.config.AllSettings.NameCubeMode == 1)
            {
                StartActionCard.Visibility = Visibility.Collapsed;
                TopActionCard.Visibility = Visibility.Collapsed;
            }

            CanChange = true;
            _logger.Information("其他设置加载完成，开机启动: {Startup}, 窗口置顶: {Top}, 模式: {Mode}",
                StartCheck.IsChecked,
                TopCheck.IsChecked,
                ModeCombox.SelectedIndex);
        }

        private void StartCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                if (StartCheck.IsChecked.Value)
                {
                    _logger.Information("启用开机自启动");
                    EnableAutoStart();
                }
                else
                {
                    _logger.Information("禁用开机自启动");
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
                    _logger.Information("开机自启动已启用，路径: {AppPath}", appPath);
                }
            }
            catch (Exception ex)
            {
                // 处理异常（如权限不足）
                _logger.Error(ex, "启用自启动失败");
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
                        _logger.Information("开机自启动已禁用");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "禁用自启动失败");
                Debug.WriteLine($"禁用自启动失败: {ex.Message}");
            }
        }

        private void TopCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.Top = TopCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                _logger.Information("窗口置顶修改为: {Top}", TopCheck.IsChecked.Value);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.NameCubeMode = ModeCombox.SelectedIndex;
                GlobalVariablesData.SaveConfig();
                _logger.Information("应用程序模式修改为: {Mode}，程序即将重启", ModeCombox.SelectedIndex);
                AppFunction.Restart();
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
                                _logger.Debug("在注册表中找到启动项: {ValueName}", valueName);
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "检查开机启动项时发生异常");
                MessageBoxFunction.ShowMessageBoxError(ex.Message, false);
                return false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("创建桌面快捷方式");
            CreateDesktopShortcut();
        }

        public static bool CreateDesktopShortcut()
        {
            WshShell shell = null;
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = Path.Combine(desktop, "点鸣魔方.lnk");
                string appPath = Assembly.GetExecutingAssembly().Location;

                if (!File.Exists(appPath))
                {
                    _logger.Error("应用程序路径无效: {AppPath}", appPath);
                    throw new FileNotFoundException("应用程序路径无效");
                }

                shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

                shortcut.TargetPath = appPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(appPath);
                shortcut.IconLocation = $"{appPath},0";
                shortcut.Description = "抽学号，点名器";
                shortcut.Save();

                SnackBarFunction.ShowSnackBarInSettingWindow("创建成功", Wpf.Ui.Controls.ControlAppearance.Success);
                _logger.Information("桌面快捷方式创建成功: {ShortcutPath}", shortcutPath);
                return true;
            }
            catch (COMException comEx)
            {
                _logger.Error(comEx, "创建桌面快捷方式时发生COM错误");
                MessageBoxFunction.ShowMessageBoxError($"COM 错误: {comEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "创建桌面快捷方式时发生异常");
                MessageBoxFunction.ShowMessageBoxError($"错误: {ex.Message}");
                return false;
            }
            finally
            {
                // 可选：手动释放 COM 对象
                if (shell != null)
                    Marshal.ReleaseComObject(shell);
            }
        }

        private void RecommendCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                if (RecommendCheck.IsChecked.Value)
                {
                    GlobalVariablesData.config.AllSettings.Recommend = "None";
                    _logger.Information("推荐设置已禁用");
                }
                else
                {
                    GlobalVariablesData.config.AllSettings.Recommend = null;
                    _logger.Information("推荐设置已启用");
                }
                GlobalVariablesData.SaveConfig();
            }
        }

        private void DisabledAnimationCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.DisableTheDisplayAnimationOfTheMainWindow = DisabledAnimationCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                _logger.Information("主窗口显示动画修改为: {Disabled}", DisabledAnimationCheck.IsChecked.Value);
            }
        }

        private void MaxSizeCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.DefaultToMaximumSize = MaxSizeCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();

                if (MaxSizeCheck.IsChecked.Value)
                {
                    DisabledAnimationCardContorl.IsEnabled = false;
                    DisabledAnimationCheck.IsChecked = true;
                    DisabledAnimationCheck_Click(null, null);
                    _logger.Information("默认最大化已启用，同时禁用显示动画");
                }
                else
                {
                    DisabledAnimationCardContorl.IsEnabled = true;
                    _logger.Information("默认最大化已禁用");
                }
            }
        }
    }
}