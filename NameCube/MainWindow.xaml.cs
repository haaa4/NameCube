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
            NavigationMenu.Navigate(typeof(Mode.Home));
        }

        /// <summary>
        /// 获取当前音量百分比
        /// </summary>
        /// <returns></returns>
        public float GetCurrentVolume()
        {
            return defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
        }

        /// <summary>
        /// 获取是否静音
        /// </summary>
        /// <returns></returns>
        public bool IsMuted()
        {
            return defaultDevice.AudioEndpointVolume.Mute;
        }

        /// <summary>
        /// 监听音量变化事件
        /// </summary>
        public void StartMonitoring()
        {
            defaultDevice.AudioEndpointVolume.OnVolumeNotification += (data) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (IsMuted())
                    {
                        VolumeInfoBar.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        VolumeInfoBar.Visibility = Visibility.Collapsed;
                    }
                });
            };
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeKeyboardHook();
            UpdateHotkeys();
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            if (GlobalVariables.json.AllSettings.NameCubeMode == 1)
            {
                ToolBoxCardAction.Visibility = Visibility.Hidden;
            }
            DataContext = this;
            Timer.Interval = 2000;
            Timer.Tick += Timer_Tick;
            Timer.Start();
            if (GlobalVariables.json.AllSettings.Dark)
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                    Wpf.Ui.Appearance.ApplicationTheme.Dark, // Theme type
                    Wpf.Ui.Controls.WindowBackdropType.Auto, // Background type
                    true // Whether to change accents automatically
                );
            }
            Loaded += (sender, args) =>
            { // 导航到第一个菜单项
                NavigationMenu.Navigate(typeof(Mode.Home));
            };
        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            this.Topmost = GlobalVariables.json.AllSettings.Top;
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (GlobalVariables.json.AllSettings.NameCubeMode == 1)
            {
                Application.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = Application
                .Current.Windows.OfType<SettingsWindow>()
                .FirstOrDefault();

            if (settingsWindow == null)
            {
                // 创建新实例
                settingsWindow = new SettingsWindow();
            }

            // 确保窗口可见并激活
            settingsWindow.Show();
            settingsWindow.Activate();
            settingsWindow.WindowState = WindowState.Normal;
        }

        private void CardAction_Click_1(object sender, RoutedEventArgs e)
        {
            var toolboxWindow = Application
                .Current.Windows.OfType<ToolBox.ToolboxWindow>()
                .FirstOrDefault();

            if (toolboxWindow == null)
            {
                // 创建新实例
                toolboxWindow = new ToolBox.ToolboxWindow();
            }

            // 确保窗口可见并激活
            toolboxWindow.Show();
            toolboxWindow.Activate();
            toolboxWindow.WindowState = WindowState.Normal;
        }

        public void InitializeKeyboardHook()
        {
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
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
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
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void HandleHotkeyPressed(Key key, ShortCut shortcut)
        {
            string groupId = shortcut.LastChangeTime; // 使用最后修改时间作为唯一标识

            // 初始化或获取该组的按键时间记录
            if (!_shortcutKeyTimes.ContainsKey(groupId))
            {
                _shortcutKeyTimes[groupId] = new Dictionary<Key, DateTime>();
            }

            // 记录当前按键时间
            _shortcutKeyTimes[groupId][key] = DateTime.Now;

            // 检查该组所有快捷键是否在1秒内按下
            CheckShortcutGroupPressed(shortcut);
        }

        private void CheckShortcutGroupPressed(ShortCut shortcut)
        {
            string groupId = shortcut.LastChangeTime;

            if (!_shortcutKeyTimes.ContainsKey(groupId))
                return;
            if (shortcut.keys.Count == 0)
                return;

            DateTime now = DateTime.Now;
            DateTime oneSecondAgo = now.AddSeconds(-1);

            bool allPressed = true;
            foreach (var key in shortcut.keys)
            {
                if (
                    !_shortcutKeyTimes[groupId].TryGetValue(key, out DateTime pressTime)
                    || pressTime < oneSecondAgo
                )
                {
                    allPressed = false;
                    break;
                }
            }

            if (allPressed)
            {
                // 执行该组快捷键对应的操作
                ExecuteShortcutAction(shortcut);

                // 清除该组记录
                _shortcutKeyTimes.Remove(groupId);
            }
        }

        private void ExecuteShortcutAction(ShortCut shortCut)
        {
            if (!isChoosing)
            {
                ShowThisWindow();
                switch (shortCut.openWay)
                {
                    case 1:
                        NavigationMenu.Navigate(typeof(Mode.OnePeopleMode));
                        break;
                    case 2:
                        NavigationMenu.Navigate(typeof(Mode.MemoryFactorMode));
                        break;
                    case 3:
                        NavigationMenu.Navigate(typeof(Mode.BatchMode));
                        break;
                    case 4:
                        NavigationMenu.Navigate(typeof(Mode.NumberMode));
                        break;
                    case 5:
                        NavigationMenu.Navigate(typeof(Mode.PrepareMode));
                        break;
                    case 6:
                        NavigationMenu.Navigate(typeof(Mode.MemoryMode));
                        break;
                    case 7:
                        NavigationMenu.Navigate(typeof(Mode.Home));
                        break;
                    default:
                        if (shortCut.ProcessGroup == null)
                        {
                            Exception exception = new Exception(
                                "用户执行了不存在的命令" + shortCut.openWay.ToString()
                            );
                            LogManager.Error(exception);
                            break;
                        }
                        else
                        {
                            RunProcessesGroup(shortCut.ProcessGroup);
                        }
                        break;
                }
            }
        }

        private static void RunProcessesGroup(ProcessGroup processGroup)
        {
            ProcessesRunningWindow processesRunningWindow = new ProcessesRunningWindow(
                processGroup
            );
            if (processGroup.show)
            {
                processesRunningWindow.Show();
                processesRunningWindow.Activate();
            }
        }

        public void UpdateHotkeys()
        {
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
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnClosed(e);
        }

        double LastTop = -1;

        public void ShowThisWindow()
        {
            //2025/8/29 回忆以前痛苦的动画制作经历
            if (LastTop == -1)
            {
                if (
                    !IsActive
                    && (
                        !(
                            this.Visibility == Visibility.Hidden
                            || this.Visibility == Visibility.Collapsed
                        )
                        || this.WindowState == WindowState.Minimized
                    )
                )
                {
                    this.Activate();
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    var dpiScale = VisualTreeHelper.GetDpi(this);
                    var workArea = SystemParameters.WorkArea;
                    double screenHeight = workArea.Height * dpiScale.DpiScaleY;
                    double screenWidth = workArea.Width * dpiScale.DpiScaleX;
                    if (!(Top < 1 && Top == 1 && Top > 1)) //这里不知道为什么，总是不能判断为NaN,就用这个数字代替了
                    {
                        Top = (screenHeight - this.Height)/ 2 * dpiScale.DpiScaleY;
                        Left=(screenWidth - this.Width)/ 2 * dpiScale.DpiScaleX;
                        LastTop = Top;
                    }
                    LastTop = Top;
                    Top = screenHeight + 300;
                    this.BeginAnimation(Window.TopProperty, null);
                    DoubleAnimation animation1 = new DoubleAnimation
                    {
                        From = screenHeight + 300,
                        To = LastTop,
                        Duration = new Duration(TimeSpan.FromSeconds(0.6)),
                        EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut },
                    };
                    var showStoryBoard = FindResource("ShowStoryBoard") as Storyboard;
                    border2.Visibility = Visibility.Visible;
                    showStoryBoard.Completed += (s, e) =>
                    {
                        border2.Visibility = Visibility.Collapsed;
                    };

                    animation1.Completed += (sender, e) =>
                    {
                        LastTop = -1;
                    };
                    this.WindowState = WindowState.Normal;
                    Top = screenHeight + 300;
                    this.Show();
                    this.Activate();
                    this.BeginAnimation(Window.TopProperty, animation1);
                    showStoryBoard.Begin();
                }
            }
        }

        private void CleanupEventHandlers(Storyboard storyboard)
        {
            if (storyboard != null)
            {
                storyboard.Stop();
                storyboard.Remove();
            }
        }
        Storyboard loadPageStoryBoard;
        Storyboard loadedPageStoryBoard;
        public void LoadPage(Page page)
        {
            NavigationMenu.IsEnabled = false;
            NavigationMenu.ReplaceContent(null);
            // 清理之前的事件处理程序

            border.Visibility = Visibility.Visible;

            EventHandler loadCompletedHandler = null;
            EventHandler loadedCompletedHandler = null;

            loadCompletedHandler = (s, e) =>
            {
                loadPageStoryBoard.Completed -= loadCompletedHandler;
                NavigationMenu.Navigate(page.GetType());

                loadedCompletedHandler = (s1, e1) =>
                {
                    loadedPageStoryBoard.Completed -= loadedCompletedHandler;
                    border.Visibility = Visibility.Collapsed;
                    NavigationMenu.IsEnabled = true;
                };

                loadedPageStoryBoard.Completed += loadedCompletedHandler;
                loadedPageStoryBoard.Begin();
            };

            loadPageStoryBoard.Completed += loadCompletedHandler;
            loadPageStoryBoard.Begin();
            if (GlobalVariables.json.OldMemoryFactorModeSettings.IsEnable ?? false)
            {
                OldMemoryFactorItem.Visibility = Visibility.Visible;
            }
            else
            {
                OldMemoryFactorItem.Visibility = Visibility.Collapsed;
            }
        }

        private void NavigationViewItem_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(new Home());
        }

        private void NavigationViewItem_Click_1(object sender, RoutedEventArgs e)
        {
            LoadPage(new OnePeopleMode());
        }

        private void NavigationViewItem_Click_2(object sender, RoutedEventArgs e)
        {
            LoadPage(new MemoryFactorMode());
        }

        private void NavigationViewItem_Click_3(object sender, RoutedEventArgs e)
        {
            LoadPage(new BatchMode());
        }

        private void NavigationViewItem_Click_4(object sender, RoutedEventArgs e)
        {
            LoadPage(new NumberMode());
        }

        private void NavigationViewItem_Click_5(object sender, RoutedEventArgs e)
        {
            LoadPage(new PrepareMode());
        }

        private void NavigationViewItem_Click_6(object sender, RoutedEventArgs e)
        {
            LoadPage(new MemoryMode());
        }

        private void fluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            deviceEnumerator = new MMDeviceEnumerator();
            defaultDevice = deviceEnumerator.GetDefaultAudioEndpoint(
                DataFlow.Render,
                Role.Multimedia
            );
            if (IsMuted())
            {
                VolumeInfoBar.Visibility = Visibility.Visible;
            }
            else
            {
                VolumeInfoBar.Visibility = Visibility.Collapsed;
            }
            StartMonitoring();
            loadPageStoryBoard = FindResource("LoadPageStoryBoard") as Storyboard;
            loadedPageStoryBoard = FindResource("LoadedPageStoryBoard") as Storyboard;
        }

        private void AdjustLayoutForDpi(double scaleFactor)
        {
            // 根据缩放因子调整边距、字体大小等
            this.Width = this.Width * scaleFactor;
            this.Height = this.Height * scaleFactor;
        }

        private void fluentWindow_DpiChanged(object sender, System.Windows.DpiChangedEventArgs e)
        {
            AdjustLayoutForDpi(e.NewDpi.DpiScaleX);
        }

        private void NavigationViewItem_Click_7(object sender, RoutedEventArgs e)
        {
            LoadPage(new OldMemoryFactorMode());
        }
    }
}
