using Masuit.Tools;
using Masuit.Tools.Logging;
using Masuit.Tools.Win32;
using NameCube.Setting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wpf.Ui.Controls;
using Application = System.Windows.Application;
using WinForms = System.Windows.Forms;

namespace NameCube
{
    /// <summary>
    /// Bird.xaml 的交互逻辑
    /// </summary>
    public partial class Bird
    {
        //private NotifyIcon _notifyIcon;
        private double SnapThreshold = 60; // 吸附阈值
        private DispatcherTimer _longPressTimer;
        private Point _dragOffset;
        private bool _isDragging;
        private Storyboard LongPressAnimation;
        private POINT LastPosition;
        System.Timers.Timer Hidetimer = new System.Timers.Timer();
        System.Timers.Timer Ab = new System.Timers.Timer();

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int GetDpiForSystem();
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;


        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }
        private void HideFromTaskView()
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(this);
            var handle = helper.Handle;
            int extendedStyle = GetWindowLong(handle, GWL_EXSTYLE);
            SetWindowLong(handle, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW);
        }

        private double GetDpiScaleFactor()
        {
            try
            {
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    uint dpi = GetDpiForWindow(hwnd);
                    return dpi / 96.0;
                }
            }
            catch
            {
                // 如果API调用失败，使用系统DPI
                return SystemParameters.PrimaryScreenHeight / 1080.0;
            }
            return 1.0;
        }

        public Bird()
        {
            if(!GlobalVariables.ret)
            {
                this.Hide();
                this.Close();
                return;
            }
            InitializeComponent();
            InitializeBehavior();
            HideFromTaskView();
            InitializePosition();
            //InitializeTrayIcon();
            Initialize();
            if (!GlobalVariables.json.StartToDo.Ball||GlobalVariables.json.AllSettings.NameCubeMode==1)
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
            Ab.Interval = 10000;
            Ab.Elapsed += Ab_Elapsed;
            Hidetimer.Elapsed+= HideTimer_Elapsed;
            Hidetimer.Interval = 3000;
            Directory.CreateDirectory(Path.Combine(GlobalVariables.configDir, "Bird_data", "Image"));
            if (GlobalVariables.json.StartToDo.AlwaysCleanMemory)
            {
                _timer.Start();
            }
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HideFromTaskView();
        }

        private void Ab_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SnapThreshold = SystemParameters.PrimaryScreenWidth / 2;

            this.Dispatcher.Invoke(new Action(() =>
            {
                SnapToEdges(0);
                SnapThreshold = GlobalVariables.json.BirdSettings.AdsorbValue;
            }));

        }
      
        private DispatcherTimer _timer;
        private void ExitApp()
        {
/*            _notifyIcon.Dispose(); */// 清理托盘图标
            Application.Current.Shutdown(); // 手动关闭应用
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            LogManager.Info("开始内存清理......");
            Masuit.Tools.Win32.Windows.ClearMemory();
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
            MouseLeftButtonDown += Bird_MouseLeftButtonDown;

        }

        private void Bird_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GlobalVariables.json.BirdSettings.StartWay == 0 || GlobalVariables.json.BirdSettings.StartWay == 3)
            {
                ShowMainWindowAsync();
            }
        }

        private void Bird_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GlobalVariables.json.BirdSettings.StartWay == 1 || GlobalVariables.json.BirdSettings.StartWay == 3 || GlobalVariables.json.BirdSettings.StartWay == 4)
            {
                ShowMainWindowAsync();
            }

        }

        private void InitializePosition()
        {
            var screen = WinForms.Screen.PrimaryScreen.WorkingArea;
            if (GlobalVariables.json.BirdSettings.StartLocationWay == 0)
            {
                Left = screen.Left;
                Top = screen.Height / 2 - Height / 2;
            }
            else if (GlobalVariables.json.BirdSettings.StartLocationWay == 1)
            {
                Left = screen.Right - this.Width;
                Top = screen.Height / 2 - Height / 2;
            }
            else
            {
                Left = GlobalVariables.json.BirdSettings.StartLocationX;
                Top = GlobalVariables.json.BirdSettings.StartLocationY;
            }

        }
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 开始拖动
            var dpiScale = GetDpiScaleFactor();
            _dragOffset = e.GetPosition(this);
            _dragOffset = new Point(_dragOffset.X * dpiScale, _dragOffset.Y * dpiScale);
            _isDragging = true;
            LongPressAnimation= (Storyboard)FindResource("LongPressAnimation");
            LastPosition = new POINT
            {
                X = (int)Left,
                Y = (int)Top,
            };

            LongPressAnimation.Begin();
            CaptureMouse();

            // 启动长按计时
            _longPressTimer.Start();
            Ab.Stop();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 结束拖动
            _isDragging = false;
            GlobalVariables.json.BirdSettings.StartLocationX = Left;
            GlobalVariables.json.BirdSettings.StartLocationY = Top;
            GlobalVariables.SaveJson();
            ReleaseMouseCapture();
            LongPressAnimation.Stop();
            LongPressAnimation.Remove();
            _longPressTimer.Stop();

            // 自动吸附
            SnapToEdges(1);
            Ab.Start();
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isDragging) return;

            var dpiScale = GetDpiScaleFactor();
            var mousePos = e.GetPosition(this);
            var screenPoint = PointToScreen(mousePos);

            Left = (screenPoint.X / dpiScale) - _dragOffset.X;
            Top = (screenPoint.Y / dpiScale) - _dragOffset.Y;
        }


        private void LongPress_Tick(object sender, EventArgs e)
        {
            _longPressTimer.Stop();
            LongPressAnimation.Stop();
            LongPressAnimation.Remove();
            var endTheLongPressAnimation=FindResource("EndTheLongPressAnimation") as Storyboard;
            endTheLongPressAnimation.Begin();
            if ((GlobalVariables.json.BirdSettings.StartWay == 2 || GlobalVariables.json.BirdSettings.StartWay == 4)&&Math.Max(Math.Abs(LastPosition.X-Left), Math.Abs(LastPosition.Y - Top))<= GlobalVariables.json.BirdSettings.LongPressMisjudgment)
            {
                ShowMainWindowAsync();
            }

        }

        private void SnapToEdges(int get)
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
            if (get == 1)
            {
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

        private async void ShowMainWindowAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if(mainWindow==null)
                {
                    mainWindow = new MainWindow();
                }
                if (mainWindow != null)
                {
                    mainWindow.ShowThisWindow();
                    mainWindow.NavigationMenu.Navigate(typeof(Mode.Home));
                }
            });
        }
        
        protected override void OnClosed(EventArgs e)
        {
            if (_longPressTimer != null)
            {
                _longPressTimer.Stop();
            }
            base.OnClosed(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
        }
        private void HideTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

                this.Dispatcher.Invoke(new Action(() =>
                {
                    rec1.Visibility = Visibility.Collapsed;
                    rec2.Visibility = Visibility.Collapsed;
                    rec3.Visibility = Visibility.Collapsed;
                    rec4.Visibility = Visibility.Collapsed;

                }));
        }
        public void Initialize()
        {
            if (GlobalVariables.json.BirdSettings.UseDefinedImage)
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(Path.Combine(GlobalVariables.configDir, "Bird_data", "Image", "image.png"));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ImageBox.Source = bitmap;
            }
            else
            {
                ImageBox.Source = new BitmapImage(new Uri("pack://application:,,,/BallPicture.png"));
            }
            SnapThreshold = GlobalVariables.json.BirdSettings.AdsorbValue;
            ImageBox.Opacity = GlobalVariables.json.BirdSettings.diaphaneity.ToDouble() / 100;
            ImageBox.Height = GlobalVariables.json.BirdSettings.Height;
            ImageBox.Width = GlobalVariables.json.BirdSettings.Width;
            progressRing.Height=Math.Min(ImageBox.Height, ImageBox.Width);
            progressRing.Width = Math.Min(ImageBox.Height, ImageBox.Width);
            Width = GlobalVariables.json.BirdSettings.Width + 20;
            Height=GlobalVariables.json.BirdSettings.Height + 20;

        }
        public void ShowReRectangle()
        {
            Hidetimer.Stop();
            rec1.Visibility = Visibility.Visible;
            rec2.Visibility = Visibility.Visible;
            rec3.Visibility = Visibility.Visible;
            rec4.Visibility = Visibility.Visible;
            ImageBox.Height = GlobalVariables.json.BirdSettings.Height;
            ImageBox.Width = GlobalVariables.json.BirdSettings.Width;
            Width = GlobalVariables.json.BirdSettings.Width + 20;
            Height = GlobalVariables.json.BirdSettings.Height + 20;
            progressRing.Height = Math.Min(ImageBox.Height, ImageBox.Width);
            progressRing.Width = Math.Min(ImageBox.Height, ImageBox.Width);
            Hidetimer.Interval = 3000;
            Hidetimer.Start();
        }


        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            ShowMainWindowAsync();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            ExitApp();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
