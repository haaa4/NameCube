using Masuit.Tools.Logging;
using Masuit.Tools.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Application = System.Windows.Application;
using WinForms = System.Windows.Forms;

namespace NameCube
{
    /// <summary>
    /// Bird.xaml 的交互逻辑
    /// </summary>
    public partial class Bird
    {
        private NotifyIcon _notifyIcon;
        private const double SnapThreshold = 60; // 吸附阈值
        private DispatcherTimer _longPressTimer;
        private Point _dragOffset;
        private bool _isDragging;

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }


        public Bird()
        {
            InitializeComponent();
            InitializeBehavior();
            InitializePosition();
            InitializeTrayIcon();
            if (!GlobalVariables.json.StartToDo.Ball)
            {
                this.Hide();
                ShowMainWindowAsync();
            }
            if (GlobalVariables.json.AllSettings.Dark)
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                Wpf.Ui.Appearance.ApplicationTheme.Dark, // Theme type
                 Wpf.Ui.Controls.WindowBackdropType.Auto,  // Background type
                 true                                      // Whether to change accents automatically
               );
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(600);
            _timer.Tick += Timer_Tick;
            if (GlobalVariables.json.StartToDo.AlwaysCleanMemory)
            {
                _timer.Start();
            }
        }
        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath),
                Visible = true,
                Text = "学号魔方"
            };

            // 添加右键菜单
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("显示窗口", null, (s, e) => ShowMainWindowAsync());
            contextMenu.Items.Add("退出", null, (s, e) => ExitApp());
            _notifyIcon.ContextMenuStrip = contextMenu;

            // 双击托盘图标显示窗口
            _notifyIcon.DoubleClick += (s, e) => ShowMainWindowAsync();
        }
        private DispatcherTimer _timer;
        private void ExitApp()
        {
            _notifyIcon.Dispose(); // 清理托盘图标
            Application.Current.Shutdown(); // 手动关闭应用
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            LogManager.Info("开始内存清理......");
            Windows.ClearMemorySilent();
        }

        private void InitializeBehavior()
        {
            // 窗口设置
            Topmost = true;
            ShowInTaskbar = false;

            // 长按计时器
            _longPressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _longPressTimer.Tick += LongPress_Tick;

            // 事件绑定
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseMove += OnMouseMove;
            MouseRightButtonDown += Bird_MouseRightButtonDown;

        }

        private void Bird_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowMainWindowAsync();
        }

        private void InitializePosition()
        {
            // 明确使用WinForms别名
            var screen = WinForms.Screen.PrimaryScreen.WorkingArea;
            Left = screen.Left;
            Top = screen.Height / 2 - Height / 2;
        }
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 开始拖动
            _dragOffset = e.GetPosition(this);
            _isDragging = true;
            CaptureMouse();

            // 启动长按计时
            _longPressTimer.Start();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 结束拖动
            _isDragging = false;
            ReleaseMouseCapture();
            _longPressTimer.Stop();

            // 自动吸附
            SnapToEdges();
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isDragging) return;
            var mousePos = PointToScreen(e.GetPosition(this));
            Left = mousePos.X - _dragOffset.X;
            Top = mousePos.Y - _dragOffset.Y;
        }


        private void LongPress_Tick(object sender, EventArgs e)
        {
            _longPressTimer.Stop();
            ShowMainWindowAsync();
        }

        private void SnapToEdges()
        {
            var screen = GetCurrentScreen();
            double rightEdge = Left + Width;
            double bottomEdge = Top + Height;

            // 水平吸附
            if (Math.Abs(Left - screen.Left) < SnapThreshold)
            {
                Left = screen.Left;
            }
            else if (Math.Abs(screen.Right - rightEdge) < SnapThreshold)
            {
                Left = screen.Right - Width;
            }

            // 垂直吸附
            if (Math.Abs(Top - screen.Top) < SnapThreshold)
            {
                Top = screen.Top;
            }
            else if (Math.Abs(screen.Bottom - bottomEdge) < SnapThreshold)
            {
                Top = screen.Bottom - Height;
            }
        }
        private Rect GetCurrentScreen()
        {
            // 获取窗口中心点对应的屏幕
            var centerX = Left + Width / 2;
            var centerY = Top + Height / 2;

            foreach (var screen in System.Windows.Forms.Screen.AllScreens)
            {
                if (screen.Bounds.Contains((int)centerX, (int)centerY))
                {
                    return new Rect(
                        screen.WorkingArea.Left,
                        screen.WorkingArea.Top,
                        screen.WorkingArea.Width,
                        screen.WorkingArea.Height);
                }
            }
            return SystemParameters.WorkArea; // 默认主屏幕
        }

        private async Task ShowMainWindowAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Show();
                    mainWindow.Activate();
                    mainWindow.NavigationMenu.Navigate(typeof(OnePeopleMode));
                }
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            _longPressTimer.Stop();
            base.OnClosed(e);
        }
    }
}
