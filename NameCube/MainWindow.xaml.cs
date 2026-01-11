using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Masuit.Tools.Logging;
using NameCube.Mode;
using NameCube.Setting;
using NameCube.ToolBox.AutomaticProcessPages;
using NAudio.CoreAudioApi;
using Wpf.Ui.Controls;
using Application = System.Windows.Application;
using Timer = System.Windows.Forms.Timer;
using Serilog;  // 添加Serilog命名空间

namespace NameCube
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private Dictionary<string, Dictionary<Key, DateTime>> _shortcutKeyTimes =
            new Dictionary<string, Dictionary<Key, DateTime>>();
        private NotifyIcon _notifyIcon = new NotifyIcon();
        Timer Timer = new Timer();
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        public bool CanUseShortCutKey = true;
        public string verson { get; set; }
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelKeyboardProc lpfn,
            IntPtr hMod,
            uint dwThreadId
        );

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private MMDeviceEnumerator deviceEnumerator;
        private MMDevice defaultDevice;
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private readonly object _lock = new object();
        private Dictionary<Key, DateTime> _keyPressTimes = new Dictionary<Key, DateTime>();
        public bool isChoosing = false;

        public void OnShowAfterLongPress()
        {
            try
            {
                Log.Information("长按后显示主窗口，导航到主页");
                NavigationMenu.Navigate(typeof(Mode.Home));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "长按后显示主窗口时发生错误");
            }
        }

        /// <summary>
        /// 获取当前音量百分比
        /// </summary>
        /// <returns></returns>
        public float GetCurrentVolume()
        {
            try
            {
                float volume = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                Log.Debug("获取当前音量: {Volume:F1}%", volume);
                return volume;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取当前音量时发生错误");
                return 0;
            }
        }

        /// <summary>
        /// 获取是否静音
        /// </summary>
        /// <returns></returns>
        public bool IsMuted()
        {
            try
            {
                bool isMuted = defaultDevice.AudioEndpointVolume.Mute;
                Log.Debug("获取静音状态: {IsMuted}", isMuted);
                return isMuted;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取静音状态时发生错误");
                return false;
            }
        }

        /// <summary>
        /// 监听音量变化事件
        /// </summary>
        public void StartMonitoring()
        {
            try
            {
                Log.Information("开始监听音量变化");
                defaultDevice.AudioEndpointVolume.OnVolumeNotification += (data) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            if (IsMuted())
                            {
                                VolumeInfoBar.Visibility = Visibility.Visible;
                                Log.Debug("检测到静音，显示音量信息栏");
                            }
                            else
                            {
                                VolumeInfoBar.Visibility = Visibility.Collapsed;
                                Log.Debug("取消静音，隐藏音量信息栏");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "处理音量变化通知时发生错误");
                        }
                    });
                };
                Log.Information("音量监听已启动");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "启动音量监听时发生错误");
            }
        }

        public MainWindow()
        {
            try
            {
                Log.Information("开始初始化主窗口");
                InitializeComponent();
                InitializeKeyboardHook();
                UpdateHotkeys();
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                if (GlobalVariables.json.AllSettings.NameCubeMode == 1)
                {
                    Log.Debug("传统窗口模式，隐藏工具箱卡片");
                    ToolBoxCardAction.Visibility = Visibility.Hidden;
                }

                DataContext = this;
                Timer.Interval = 2000;
                Timer.Tick += Timer_Tick;
                Timer.Start();
                Log.Debug("窗口置顶检查定时器已启动");

                if (GlobalVariables.json.AllSettings.Dark)
                {
                    Log.Debug("应用深色主题");
                    Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                        Wpf.Ui.Appearance.ApplicationTheme.Dark, // Theme type
                        Wpf.Ui.Controls.WindowBackdropType.Auto, // Background type
                        true // Whether to change accents automatically
                    );
                }

                Loaded += (sender, args) =>
                {
                    // 导航到第一个菜单项
                    NavigationMenu.Navigate(typeof(Mode.Home));
                    Log.Debug("主窗口加载完成，导航到主页");
                };
                this.DataContext= this;
                if(GlobalVariables.IsBeta)
                {
                    verson = "测试版:" + GlobalVariables.Version;
                }
                else
                {
                    verson = "正式版:" + GlobalVariables.Version;
                }
                    Log.Information("主窗口初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化主窗口时发生错误");
                throw;
            }
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            try
            {
                this.Topmost = GlobalVariables.json.AllSettings.Top;
                if (GlobalVariables.json.AllSettings.Top)
                {
                    Log.Debug("窗口置顶状态检查: 已置顶");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "窗口置顶检查定时器处理时发生错误");
            }
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (GlobalVariables.json.AllSettings.NameCubeMode == 1)
                {
                    Log.Information("传统窗口模式，直接退出应用");
                    Application.Current.Shutdown();
                }
                else
                {
                    Log.Information("悬浮球模式，隐藏主窗口");
                    e.Cancel = true;
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理主窗口关闭事件时发生错误");
            }
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("点击设置按钮");
                var settingsWindow = Application
                    .Current.Windows.OfType<SettingsWindow>()
                    .FirstOrDefault();

                if (settingsWindow == null)
                {
                    Log.Debug("设置窗口不存在，创建新实例");
                    // 创建新实例
                    settingsWindow = new SettingsWindow();
                }

                // 确保窗口可见并激活
                settingsWindow.Show();
                settingsWindow.Activate();
                settingsWindow.WindowState = WindowState.Normal;
                Log.Information("设置窗口已显示");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "打开设置窗口时发生错误");
            }
        }

        private void CardAction_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("点击工具箱按钮");
                var toolboxWindow = Application
                    .Current.Windows.OfType<ToolBox.ToolboxWindow>()
                    .FirstOrDefault();

                if (toolboxWindow == null)
                {
                    Log.Debug("工具箱窗口不存在，创建新实例");
                    // 创建新实例
                    toolboxWindow = new ToolBox.ToolboxWindow();
                }

                // 确保窗口可见并激活
                toolboxWindow.Show();
                toolboxWindow.Activate();
                toolboxWindow.WindowState = WindowState.Normal;
                Log.Information("工具箱窗口已显示");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "打开工具箱窗口时发生错误");
            }
        }

        public void InitializeKeyboardHook()
        {
            try
            {
                Log.Information("开始初始化键盘钩子");
                _proc = HookCallback;
                using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
                using (var curModule = curProcess.MainModule)
                {
                    _hookID = SetWindowsHookEx(
                        WH_KEYBOARD_LL,
                        _proc,
                        GetModuleHandle(curModule.ModuleName),
                        0
                    );
                    Log.Information("键盘钩子已安装，钩子ID: {HookID}", _hookID);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化键盘钩子时发生错误");
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                    lock (_lock)
                    {
                        // 检查所有快捷键组
                        foreach (var shortcut in GlobalVariables.json.ShortCutKey.keysGrounp)
                        {
                            if (shortcut.keys.Contains(key))
                            {
                                Log.Debug("检测到快捷键按键: {Key}，属于快捷键组: {GroupId}", key, shortcut.LastChangeTime);
                                Dispatcher.BeginInvoke(
                                    (Action)(
                                        () =>
                                        {
                                            HandleHotkeyPressed(key, shortcut);
                                        }
                                    )
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "键盘钩子回调处理时发生错误");
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void HandleHotkeyPressed(Key key, ShortCut shortcut)
        {
            try
            {
                string groupId = shortcut.LastChangeTime; // 使用最后修改时间作为唯一标识

                // 初始化或获取该组的按键时间记录
                if (!_shortcutKeyTimes.ContainsKey(groupId))
                {
                    _shortcutKeyTimes[groupId] = new Dictionary<Key, DateTime>();
                    Log.Debug("创建新的快捷键组记录: {GroupId}", groupId);
                }

                // 记录当前按键时间
                _shortcutKeyTimes[groupId][key] = DateTime.Now;
                Log.Debug("记录按键时间: 组{GroupId}，按键{Key}，时间{PressTime}",
                    groupId, key, DateTime.Now);

                // 检查该组所有快捷键是否在1秒内按下
                CheckShortcutGroupPressed(shortcut);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理快捷键按键时发生错误");
            }
        }

        private void CheckShortcutGroupPressed(ShortCut shortcut)
        {
            try
            {
                string groupId = shortcut.LastChangeTime;

                if (!_shortcutKeyTimes.ContainsKey(groupId))
                {
                    Log.Debug("快捷键组不存在: {GroupId}", groupId);
                    return;
                }
                if (shortcut.keys.Count == 0)
                {
                    Log.Debug("快捷键组为空: {GroupId}", groupId);
                    return;
                }

                DateTime now = DateTime.Now;
                DateTime oneSecondAgo = now.AddSeconds(-1);

                bool allPressed = true;
                foreach (var key in shortcut.keys)
                {
                    if (!_shortcutKeyTimes[groupId].TryGetValue(key, out DateTime pressTime) || pressTime < oneSecondAgo)
                    {
                        allPressed = false;
                        Log.Debug("快捷键组{GroupId}的按键{Key}未在1秒内按下", groupId, key);
                        break;
                    }
                }

                if (allPressed)
                {
                    Log.Information("检测到完整快捷键组合: {GroupId}，执行对应操作", groupId);
                    // 执行该组快捷键对应的操作
                    ExecuteShortcutAction(shortcut);

                    // 清除该组记录
                    _shortcutKeyTimes.Remove(groupId);
                    Log.Debug("已清除快捷键组记录: {GroupId}", groupId);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "检查快捷键组合时发生错误");
            }
        }

        private void ExecuteShortcutAction(ShortCut shortCut)
        {
            try
            {
                if (!isChoosing)
                {
                    Log.Information("执行快捷键操作，打开方式: {OpenWay}", shortCut.openWay);
                    ShowThisWindow();
                    switch (shortCut.openWay)
                    {
                        case 1:
                            NavigationMenu.Navigate(typeof(Mode.OnePeopleMode));
                            Log.Information("导航到单人模式");
                            break;
                        case 2:
                            NavigationMenu.Navigate(typeof(Mode.MemoryFactorMode));
                            Log.Information("导航到记忆因子模式");
                            break;
                        case 3:
                            NavigationMenu.Navigate(typeof(Mode.BatchMode));
                            Log.Information("导航到批量模式");
                            break;
                        case 4:
                            NavigationMenu.Navigate(typeof(Mode.NumberMode));
                            Log.Information("导航到数字模式");
                            break;
                        case 5:
                            NavigationMenu.Navigate(typeof(Mode.PrepareMode));
                            Log.Information("导航到准备模式");
                            break;
                        case 6:
                            NavigationMenu.Navigate(typeof(Mode.MemoryMode));
                            Log.Information("导航到记忆模式");
                            break;
                        case 7:
                            NavigationMenu.Navigate(typeof(Mode.Home));
                            Log.Information("导航到主页");
                            break;
                        default:
                            if (shortCut.ProcessGroup == null)
                            {
                                Exception exception = new Exception(
                                    "用户执行了不存在的命令" + shortCut.openWay.ToString()
                                );
                                Log.Error(exception, "执行了不存在的快捷键命令: {OpenWay}", shortCut.openWay);
                                break;
                            }
                            else
                            {
                                Log.Information("执行进程组操作: {ProcessGroup}", shortCut.ProcessGroup);
                                RunProcessesGroup(shortCut.ProcessGroup);
                            }
                            break;
                    }
                }
                else
                {
                    Log.Debug("当前正在选择中，忽略快捷键操作");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "执行快捷键操作时发生错误");
            }
        }

        private static void RunProcessesGroup(ProcessGroup processGroup)
        {
            try
            {
                Log.Information("运行进程组: {ProcessGroupName}", processGroup);
                ProcessesRunningWindow processesRunningWindow = new ProcessesRunningWindow(
                    processGroup
                );
                if (processGroup.show)
                {
                    processesRunningWindow.Show();
                    processesRunningWindow.Activate();
                    Log.Information("进程组窗口已显示");
                }
                else
                {
                    Log.Debug("进程组配置为不显示窗口");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "运行进程组时发生错误");
            }
        }

        public void UpdateHotkeys()
        {
            try
            {
                Log.Debug("开始更新热键");
                lock (_lock)
                {
                    // 清理无效组的记录
                    var validGroupIds = GlobalVariables
                        .json.ShortCutKey.keysGrounp.Select(s => s.LastChangeTime)
                        .ToList();

                    var keysToRemove = _shortcutKeyTimes
                        .Keys.Where(id => !validGroupIds.Contains(id))
                        .ToList();

                    foreach (var key in keysToRemove)
                    {
                        _shortcutKeyTimes.Remove(key);
                        Log.Debug("移除无效的快捷键组记录: {GroupId}", key);
                    }
                    Log.Information("热键更新完成，当前有效组数: {ValidGroupCount}", validGroupIds.Count);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "更新热键时发生错误");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                Log.Information("主窗口正在关闭，卸载键盘钩子");
                UnhookWindowsHookEx(_hookID);
                base.OnClosed(e);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "主窗口关闭时发生错误");
            }
        }

        double LastTop = -1;

        public void ShowThisWindow()
        {
            try
            {
                Log.Information("显示主窗口");
                //2025/8/29 回忆以前痛苦的动画制作经历
                if (LastTop == -1)
                {
                    if (!IsActive && (!(this.Visibility == Visibility.Hidden || this.Visibility == Visibility.Collapsed) || this.WindowState == WindowState.Minimized))
                    {
                        Log.Debug("窗口已存在但未激活，激活窗口");
                        this.Activate();
                        if (GlobalVariables.json.AllSettings.DefaultToMaximumSize)
                        {
                            this.WindowState = WindowState.Maximized;
                            Log.Debug("窗口最大化");
                        }
                        else
                        {
                            this.WindowState = WindowState.Normal;
                            Log.Debug("窗口恢复正常大小");
                        }
                    }
                    else
                    {
                        var dpiScale = VisualTreeHelper.GetDpi(this);
                        var workArea = SystemParameters.WorkArea;
                        double screenHeight = workArea.Height * dpiScale.DpiScaleY;
                        double screenWidth = workArea.Width * dpiScale.DpiScaleX;
                        var showStoryBoard = FindResource("ShowStoryBoard") as Storyboard;

                        if (!(Top < 1 && Top == 1 && Top > 1)) //这里不知道为什么，总是不能判断为NaN,就用这个方式代替了
                        {
                            Top = (screenHeight - this.Height) / 2 * dpiScale.DpiScaleY;
                            Left = (screenWidth - this.Width) / 2 * dpiScale.DpiScaleX;
                            LastTop = Top;
                            Log.Debug("计算窗口居中位置: X={Left}, Y={Top}", Left, Top);
                        }

                        if (GlobalVariables.json.AllSettings.DisableTheDisplayAnimationOfTheMainWindow)
                        {
                            Log.Debug("禁用主窗口显示动画");
                            this.Opacity = 1.0; // 确保窗口完全不透明
                            this.Show();
                            this.Activate();
                            if (GlobalVariables.json.AllSettings.DefaultToMaximumSize)
                            {
                                this.WindowState = WindowState.Maximized;
                            }
                            LastTop = -1;
                        }
                        else
                        {
                            Log.Debug("启用主窗口透明度渐变动画");

                            // 设置窗口初始状态为透明
                            this.Opacity = 0.0;

                            // 创建透明度渐变动画
                            DoubleAnimation opacityAnimation = new DoubleAnimation
                            {
                                From = 0.0,
                                To = 1.0,
                                Duration = new Duration(TimeSpan.FromSeconds(0.6)),
                                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
                            };

                            border2.Visibility = Visibility.Visible;

                            opacityAnimation.Completed += (sender, e) =>
                            {
                                LastTop = -1;
                                Log.Debug("主窗口透明度渐变动画完成");
                            };

                            this.WindowState = WindowState.Normal;
                            this.Show();
                            this.Activate();

                            // 开始透明度动画
                            this.BeginAnimation(Window.OpacityProperty, opacityAnimation);
                        }

                        showStoryBoard.Completed += (s, e) =>
                        {
                            border2.Visibility = Visibility.Collapsed;
                            Log.Debug("显示故事板动画完成");
                        };

                        showStoryBoard.Begin();
                        Log.Debug("显示故事板动画开始");
                    }
                }
                else
                {
                    Log.Debug("窗口正在显示动画中，跳过重复调用");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "显示主窗口时发生错误");
            }
        }

        private void CleanupEventHandlers(Storyboard storyboard)
        {
            try
            {
                if (storyboard != null)
                {
                    storyboard.Stop();
                    storyboard.Remove();
                    Log.Debug("清理故事板事件处理器");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "清理故事板事件处理器时发生错误");
            }
        }

        Storyboard loadPageStoryBoard;
        Storyboard loadedPageStoryBoard;

        public void LoadPage(Page page)
        {
            try
            {
                Log.Information("加载页面: {PageType}", page.GetType().Name);
                NavigationMenu.IsEnabled = false;
                NavigationMenu.ReplaceContent(null);
                // 清理之前的事件处理程序

                border.Visibility = Visibility.Visible;

                EventHandler loadCompletedHandler = null;
                EventHandler loadedCompletedHandler = null;

                loadCompletedHandler = (s, e) =>
                {
                    try
                    {
                        loadPageStoryBoard.Completed -= loadCompletedHandler;
                        NavigationMenu.Navigate(page.GetType());
                        Log.Debug("导航到页面: {PageType}", page.GetType().Name);

                        loadedCompletedHandler = (s1, e1) =>
                        {
                            loadedPageStoryBoard.Completed -= loadedCompletedHandler;
                            border.Visibility = Visibility.Collapsed;
                            NavigationMenu.IsEnabled = true;
                            Log.Debug("页面加载完成");
                        };

                        loadedPageStoryBoard.Completed += loadedCompletedHandler;
                        loadedPageStoryBoard.Begin();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "页面加载回调处理时发生错误");
                    }
                };

                loadPageStoryBoard.Completed += loadCompletedHandler;
                loadPageStoryBoard.Begin();

                if (GlobalVariables.json.OldMemoryFactorModeSettings.IsEnable ?? false)
                {
                    OldMemoryFactorItem.Visibility = Visibility.Visible;
                    Log.Debug("旧记忆因子模式已启用，显示菜单项");
                }
                else
                {
                    OldMemoryFactorItem.Visibility = Visibility.Collapsed;
                    Log.Debug("旧记忆因子模式已禁用，隐藏菜单项");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载页面时发生错误");
            }
        }

        private void NavigationViewItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("导航到主页");
                LoadPage(new Home());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "导航到主页时发生错误");
            }
        }

        private void NavigationViewItem_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("导航到单人模式");
                LoadPage(new OnePeopleMode());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "导航到单人模式时发生错误");
            }
        }

        private void NavigationViewItem_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("导航到记忆因子模式");
                LoadPage(new MemoryFactorMode());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "导航到记忆因子模式时发生错误");
            }
        }

        private void NavigationViewItem_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("导航到批量模式");
                LoadPage(new BatchMode());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "导航到批量模式时发生错误");
            }
        }

        private void NavigationViewItem_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("导航到数字模式");
                LoadPage(new NumberMode());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "导航到数字模式时发生错误");
            }
        }

        private void NavigationViewItem_Click_5(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("导航到准备模式");
                LoadPage(new PrepareMode());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "导航到准备模式时发生错误");
            }
        }

        private void NavigationViewItem_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("导航到记忆模式");
                LoadPage(new MemoryMode());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "导航到记忆模式时发生错误");
            }
        }

        private void fluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("主窗口加载完成");
                deviceEnumerator = new MMDeviceEnumerator();
                defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(
                    DataFlow.Render,
                    Role.Multimedia
                );

                if (IsMuted())
                {
                    VolumeInfoBar.Visibility = Visibility.Visible;
                    Log.Debug("初始状态: 静音，显示音量信息栏");
                }
                else
                {
                    VolumeInfoBar.Visibility = Visibility.Collapsed;
                    Log.Debug("初始状态: 未静音，隐藏音量信息栏");
                }

                StartMonitoring();
                loadPageStoryBoard = FindResource("LoadPageStoryBoard") as Storyboard;
                loadedPageStoryBoard = FindResource("LoadedPageStoryBoard") as Storyboard;
                Log.Debug("页面加载故事板资源已获取");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "主窗口加载事件处理时发生错误");
            }
        }

        private void AdjustLayoutForDpi(double scaleFactor)
        {
            try
            {
                Log.Debug("根据DPI缩放调整布局，缩放因子: {ScaleFactor}", scaleFactor);
                // 根据缩放因子调整边距、字体大小等
                this.Width = this.Width * scaleFactor;
                this.Height = this.Height * scaleFactor;
                Log.Debug("窗口尺寸调整: {Width}x{Height}", Width, Height);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "调整DPI布局时发生错误");
            }
        }

        private void fluentWindow_DpiChanged(object sender, System.Windows.DpiChangedEventArgs e)
        {
            try
            {
                Log.Information("DPI变化: 从{OldDpi}到{NewDpi}", e.OldDpi, e.NewDpi);
                AdjustLayoutForDpi(e.NewDpi.DpiScaleX);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理DPI变化事件时发生错误");
            }
        }

        private void NavigationViewItem_Click_7(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("导航到旧记忆因子模式");
                LoadPage(new OldMemoryFactorMode());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "导航到旧记忆因子模式时发生错误");
            }
        }
    }
}