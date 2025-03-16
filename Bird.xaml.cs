using Masuit.Tools.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace NameCube
{
    /// <summary>
    /// Bird.xaml 的交互逻辑
    /// </summary>
    public partial class Bird
    {
        private const double SnapThreshold = 20; // 吸附阈值
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
        Json json = new Json();
        public void SaveJson()
        {
            string jsonString = JsonSerializer.Serialize(json);
            File.WriteAllText("config.json", jsonString);
        }
        public Bird()
        {
            InitializeComponent();
            InitializeBehavior();
            InitializePosition();
            if (!File.Exists("config.json"))
            {
                json = new Json
                {
                    Name = new List<string>(),
                    Speech = true,
                    Dark = false,
                    Volume = 100,
                    Speed = 0,
                    Ball = true,
                    Start=false,
                    AlwaysCleanMemory = false,
                };
                json.Name.Add("张三");

                SaveJson();
            }
            else
            {
                string jsonstring = File.ReadAllText("config.json");
                json = JsonSerializer.Deserialize<Json>(jsonstring);
                if (json.Dark)
                {
                    Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                    Wpf.Ui.Appearance.ApplicationTheme.Dark, // Theme type
                     Wpf.Ui.Controls.WindowBackdropType.Auto,  // Background type
                     true                                      // Whether to change accents automatically
                   );
                }
            }
            if (!json.Ball)
            {
                this.Hide();
            }
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(600); 
            _timer.Tick += Timer_Tick;
            if(json.AlwaysCleanMemory)
            {
                _timer.Start();
            }
        }
        private DispatcherTimer _timer;
        private void Timer_Tick(object sender, EventArgs e)
        {
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
            ShowMainWindow();
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

        private void ShowMainWindow()
        {
            // 明确使用WPF的Application
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
                mainWindow.OnShowAfterLongPress();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _longPressTimer.Stop();
            base.OnClosed(e);
        }
    }
}
