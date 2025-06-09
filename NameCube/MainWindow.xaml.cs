using NameCube.Setting;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using System.Windows.Threading;
using System.ComponentModel;
using System;
using Wpf.Ui.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Diagnostics;
using Masuit.Tools.Logging;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;


namespace NameCube
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private Dictionary<string, Dictionary<Key, DateTime>> _shortcutKeyTimes = new Dictionary<string, Dictionary<Key, DateTime>>();
        private NotifyIcon _notifyIcon=new NotifyIcon();
        Timer Timer=new Timer();
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        public bool CanUseShortCutKey = true;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private readonly object _lock = new object();
        private Dictionary<Key, DateTime> _keyPressTimes = new Dictionary<Key, DateTime>();
        public bool isChoosing = false;

        public void OnShowAfterLongPress()
        {
            NavigationMenu.Navigate(typeof(Mode.Home));
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeKeyboardHook();
            UpdateHotkeys();
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            if(GlobalVariables.json.AllSettings.NameCubeMode==1)
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
                 Wpf.Ui.Controls.WindowBackdropType.Auto,  // Background type
                 true                                      // Whether to change accents automatically
               );
            }
            Loaded += (sender, args) =>
            {               // 导航到第一个菜单项
                NavigationMenu.Navigate(typeof(Mode.Home));
            };

        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            this.Topmost = GlobalVariables.json.AllSettings.Top;
        }


        private void ExitApp()
        {
            _notifyIcon.Dispose(); // 清理托盘图标
            Application.Current.Shutdown(); // 手动关闭应用
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(GlobalVariables.json.AllSettings.NameCubeMode==1)
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
            var settingsWindow = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();

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
            var toolboxWindow = Application.Current.Windows.OfType<ToolBox.ToolboxWindow>().FirstOrDefault();

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
                _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
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
                            Dispatcher.BeginInvoke((Action)(() =>
                            {
                                HandleHotkeyPressed(key, shortcut);
                            }));
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

            if (!_shortcutKeyTimes.ContainsKey(groupId)) return;
            if (shortcut.keys.Count == 0) return;

            DateTime now = DateTime.Now;
            DateTime oneSecondAgo = now.AddSeconds(-1);

            bool allPressed = true;
            foreach (var key in shortcut.keys)
            {
                if (!_shortcutKeyTimes[groupId].TryGetValue(key, out DateTime pressTime) ||
                    pressTime < oneSecondAgo)
                {
                    allPressed = false;
                    break;
                }
            }

            if (allPressed)
            {
                // 执行该组快捷键对应的操作
                ExecuteShortcutAction(shortcut.openWay);

                // 清除该组记录
                _shortcutKeyTimes.Remove(groupId);
            }
        }

        private void ExecuteShortcutAction(int openWay)
        {
            
            if (!isChoosing&&openWay != 0)
            {
                ShowThisWindow();
                switch (openWay)
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
                        Exception exception = new Exception("用户执行了不存在的命令" + openWay.ToString());
                        LogManager.Error(exception);
                        break;
                }
            }

        }
        public void UpdateHotkeys()
        {
            lock (_lock)
            {
                // 清理无效组的记录
                var validGroupIds = GlobalVariables.json.ShortCutKey.keysGrounp
                    .Select(s => s.LastChangeTime)
                    .ToList();

                var keysToRemove = _shortcutKeyTimes.Keys
                    .Where(id => !validGroupIds.Contains(id))
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
        private void ReleaseManagedResources()
        {
            _notifyIcon?.Dispose();
            _notifyIcon = null;

            Timer?.Stop();
            Timer?.Dispose();
            Timer = null;
        }

        private void ReleaseUnmanagedResources()
        {
            // 确保钩子完全释放
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        private void UnsubscribeEvents()
        {
            Loaded -= (sender, args) => { NavigationMenu.Navigate(typeof(Mode.Home)); };
            Timer=null;
            Closing -= FluentWindow_Closing;
        }

        private void ClearCollections()
        {
            _keyPressTimes?.Clear();
            _keyPressTimes = null;
        }
        double LastTop=-1;
        public void ShowThisWindow()
        {
            if(LastTop==-1)
            {
                if (!IsActive && (!(this.Visibility == Visibility.Hidden || this.Visibility == Visibility.Collapsed) || this.WindowState == WindowState.Minimized))
                {
                    this.Activate();
                    this.WindowState=WindowState.Normal;
                }
                else
                {
                    var dpiScale = VisualTreeHelper.GetDpi(this);
                    var workArea = SystemParameters.WorkArea;
                    double screenHeight = workArea.Height * dpiScale.DpiScaleY;
                    if (!(Top<1&&Top==1&&Top>1))//这里不知道为什么，总是不能判断为NaN,就用这个数字代替了
                    {
                        Top = 200;
                        LastTop = Top;
                    }
                    LastTop = Top;
                    Top= screenHeight + 300;
                    this.BeginAnimation(Window.TopProperty, null);
                    DoubleAnimation animation1 = new DoubleAnimation
                    {
                        From = screenHeight + 300,
                        To = LastTop,
                        Duration = new Duration(TimeSpan.FromSeconds(0.6)),
                        EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
                    };
                    animation1.Completed += (sender, e) =>
                    {
                        LastTop = -1;
                    };
                    this.WindowState = WindowState.Normal;
                    Top = screenHeight + 300;
                    Left = 200;
                    this.Show();
                    this.Activate();
                    this.BeginAnimation(Window.TopProperty, animation1);

                }

            }

        }


    }
}
