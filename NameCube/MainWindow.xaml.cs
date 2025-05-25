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


namespace NameCube
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
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

        public void OnShowAfterLongPress()
        {
            NavigationMenu.Navigate(typeof(Mode.Home));
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeKeyboardHook();
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

        private void ShowWindow()
        {

            this.Show();
            this.WindowState = WindowState.Normal; // 恢复窗口状态
            this.Activate(); // 激活窗口到前台
        }

        private void ExitApp()
        {
            _notifyIcon.Dispose(); // 清理托盘图标
            Application.Current.Shutdown(); // 手动关闭应用
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            if(GlobalVariables.json.AllSettings.NameCubeMode==1)
            {
                Application.Current.Shutdown();
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
                    if (GlobalVariables.json.ShortCutKey.keys.Contains(key))
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            HandleHotkeyPressed(key);
                        }));
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void HandleHotkeyPressed(Key key)
        {
            // 记录按下时间
            _keyPressTimes[key] = DateTime.Now;

            // 检查所有快捷键是否在1秒内按下
            CheckAllKeysPressedWithinSecond();
        }

        private void CheckAllKeysPressedWithinSecond()
        {
            lock (_lock)
            {
                if (GlobalVariables.json.ShortCutKey.keys.Count == 0) return;

                DateTime now = DateTime.Now;
                DateTime oneSecondAgo = now.AddSeconds(-1);

                bool allPressed = true;
                foreach (var key in GlobalVariables.json.ShortCutKey.keys)
                {
                    if (!_keyPressTimes.TryGetValue(key, out DateTime pressTime) || pressTime < oneSecondAgo)
                    {
                        allPressed = false;
                        break;
                    }
                }

                if (allPressed)
                {
                    if (CanUseShortCutKey)
                    {
                        if(GlobalVariables.json.ShortCutKey.Way!=0)
                        {
                            this.Show();
                            this.Activate();
                            this.WindowState = WindowState.Normal;
                        }
                        if(GlobalVariables.json.ShortCutKey.Way==1)
                        {
                            NavigationMenu.Navigate(typeof(Mode.Home));
                        }
                        else if (GlobalVariables.json.ShortCutKey.Way == 2)
                        {
                            NavigationMenu.Navigate(typeof(Mode.OnePeopleMode));
                        }
                        else if (GlobalVariables.json.ShortCutKey.Way == 3)
                        {
                            NavigationMenu.Navigate(typeof(Mode.MemoryFactorMode));
                        }
                    }
                    _keyPressTimes.Clear();
                }
            }
        }

        public void UpdateHotkeys(List<Key> newHotkeys)
        {
            lock (_lock)
            {
                GlobalVariables.json.ShortCutKey.keys = newHotkeys.Count > 5 ? newHotkeys.GetRange(0, 5) : new List<Key>(newHotkeys);
                CleanupKeyPressRecords();
            }
        }

        private void CleanupKeyPressRecords()
        {
            var keysToRemove = new List<Key>();
            foreach (var key in _keyPressTimes.Keys)
            {
                if (!GlobalVariables.json.ShortCutKey.keys.Contains(key)) keysToRemove.Add(key);
            }
            foreach (var key in keysToRemove)
            {
                _keyPressTimes.Remove(key);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnClosed(e);
        }
    }
}
