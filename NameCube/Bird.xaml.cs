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
using Serilog;  // 添加Serilog命名空间

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
            Log.Debug("开始从任务视图中隐藏窗口");
            try
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(this);
                var handle = helper.Handle;
                int extendedStyle = GetWindowLong(handle, GWL_EXSTYLE);
                SetWindowLong(handle, GWL_EXSTYLE, extendedStyle | WS_EX_TOOLWINDOW);
                Log.Debug("窗口已从任务视图中隐藏");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "从任务视图中隐藏窗口时发生错误");
            }
        }

        private double GetDpiScaleFactor()
        {
            try
            {
                Log.Debug("开始获取DPI缩放因子");
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    uint dpi = GetDpiForWindow(hwnd);
                    double scale = dpi / 96.0;
                    Log.Debug("获取到DPI缩放因子: {DpiScale}", scale);
                    return scale;
                }
                Log.Warning("无法获取窗口句柄，使用默认DPI缩放因子");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取DPI缩放因子时发生错误，使用备用方法");
                return SystemParameters.PrimaryScreenHeight / 1080.0;
            }
            return 1.0;
        }

        public Bird()
        {
            try
            {
                if (!GlobalVariablesData.ret)
                {
                    Log.Information("全局变量ret为false，跳过Bird窗口初始化");
                    this.Hide();
                    this.Close();
                    return;
                }

                Log.Information("开始初始化Bird窗口");
                InitializeComponent();
                InitializeBehavior();
                HideFromTaskView();
                InitializePosition();
                //InitializeTrayIcon();
                Initialize();

                if (!GlobalVariablesData.config.StartToDo.Ball || GlobalVariablesData.config.AllSettings.NameCubeMode == 1)
                {
                    Log.Information("配置为不显示悬浮球或使用传统模式，隐藏Bird窗口");
                    this.Hide();
                    ShowMainWindowAsync();
                }

                if (GlobalVariablesData.config.AllSettings.Dark)
                {
                    Log.Debug("应用深色主题");
                    Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                        Wpf.Ui.Appearance.ApplicationTheme.Dark,
                        Wpf.Ui.Controls.WindowBackdropType.Auto,
                        true
                    );
                }

                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(600);
                _timer.Tick += Timer_Tick;
                Ab.Interval = 10000;
                Ab.Elapsed += Ab_Elapsed;
                Hidetimer.Elapsed += HideTimer_Elapsed;
                Hidetimer.Interval = 3000;

                string imageDir = Path.Combine(GlobalVariablesData.userDataDir, "Bird_data", "Image");
                Directory.CreateDirectory(imageDir);
                Log.Debug("创建图片目录: {ImageDir}", imageDir);

                if (GlobalVariablesData.config.StartToDo.AlwaysCleanMemory)
                {
                    Log.Information("启用自动内存清理定时器");
                    _timer.Start();
                }

                Log.Information("Bird窗口初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化Bird窗口时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 获取系统可用物理内存
        /// </summary>
        public static long GetAvailablePhysicalMemory()
        {
            try
            {
                Log.Debug("开始获取可用物理内存");
                using (PerformanceCounter pc = new PerformanceCounter("Memory", "Available Bytes"))
                {
                    long memory = (long)pc.NextValue() / 1048576;
                    Log.Debug("可用物理内存: {AvailableMemory} MB", memory);
                    return memory;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取可用物理内存时发生错误");
                return -1;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            try
            {
                Log.Debug("Bird窗口源初始化");
                base.OnSourceInitialized(e);
                HideFromTaskView();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Bird窗口源初始化时发生错误");
            }
        }

        private void Ab_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Log.Debug("自动吸附定时器触发");
                SnapThreshold = SystemParameters.PrimaryScreenWidth / 2;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    SnapToEdges(0);
                    SnapThreshold = GlobalVariablesData.config.BirdSettings.AdsorbValue;
                    Log.Debug("自动吸附完成，吸附阈值恢复为: {AdsorbValue}", SnapThreshold);
                }));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "自动吸附定时器处理时发生错误");
            }
        }

        private DispatcherTimer _timer;
        private void ExitApp()
        {
            Log.Information("退出应用");
            /*            _notifyIcon.Dispose(); */// 清理托盘图标
            Application.Current.Shutdown(); // 手动关闭应用
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                long beforeMemory = GetAvailablePhysicalMemory();
                Log.Information("开始自动内存清理，当前可用: {BeforeMemory} MB", beforeMemory);
                Masuit.Tools.Win32.Windows.ClearMemory();
                long afterMemory = GetAvailablePhysicalMemory();
                Log.Information("内存清理结束，当前可用: {AfterMemory} MB", afterMemory);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "自动内存清理时发生错误");
            }
        }

        private void InitializeBehavior()
        {
            try
            {
                Log.Debug("开始初始化Bird窗口行为");
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

                Log.Debug("Bird窗口行为初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化Bird窗口行为时发生错误");
            }
        }

        private void Bird_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (GlobalVariablesData.config.BirdSettings.StartWay == 0 || GlobalVariablesData.config.BirdSettings.StartWay == 3)
                {
                    Log.Information("左键点击启动主窗口，启动方式: {StartWay}", GlobalVariablesData.config.BirdSettings.StartWay);
                    ShowMainWindowAsync();
                }
                else
                {
                    Log.Debug("左键点击，但启动方式配置不触发启动: {StartWay}", GlobalVariablesData.config.BirdSettings.StartWay);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理鼠标左键点击事件时发生错误");
            }
        }

        private void Bird_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (GlobalVariablesData.config.BirdSettings.StartWay == 1 || GlobalVariablesData.config.BirdSettings.StartWay == 3 || GlobalVariablesData.config.BirdSettings.StartWay == 4)
                {
                    Log.Information("右键点击启动主窗口，启动方式: {StartWay}", GlobalVariablesData.config.BirdSettings.StartWay);
                    ShowMainWindowAsync();
                }
                else
                {
                    Log.Debug("右键点击，但启动方式配置不触发启动: {StartWay}", GlobalVariablesData.config.BirdSettings.StartWay);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理鼠标右键点击事件时发生错误");
            }
        }

        private void InitializePosition()
        {
            try
            {
                Log.Debug("开始初始化Bird窗口位置");
                var screen = WinForms.Screen.PrimaryScreen.WorkingArea;
                if (GlobalVariablesData.config.BirdSettings.StartLocationWay == 0)
                {
                    Left = screen.Left;
                    Top = screen.Height / 2 - Height / 2;
                    Log.Debug("窗口位置: 左侧居中，X={Left}, Y={Top}", Left, Top);
                }
                else if (GlobalVariablesData.config.BirdSettings.StartLocationWay == 1)
                {
                    Left = screen.Right - this.Width;
                    Top = screen.Height / 2 - Height / 2;
                    Log.Debug("窗口位置: 右侧居中，X={Left}, Y={Top}", Left, Top);
                }
                else
                {
                    Left = GlobalVariablesData.config.BirdSettings.StartLocationX;
                    Top = GlobalVariablesData.config.BirdSettings.StartLocationY;
                    Log.Debug("窗口位置: 自定义位置，X={Left}, Y={Top}", Left, Top);
                }
                Log.Debug("Bird窗口位置初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化Bird窗口位置时发生错误");
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Log.Debug("开始拖动Bird窗口");
                // 开始拖动
                var dpiScale = GetDpiScaleFactor();
                _dragOffset = e.GetPosition(this);
                _dragOffset = new Point(_dragOffset.X * dpiScale, _dragOffset.Y * dpiScale);
                _isDragging = true;
                LongPressAnimation = (Storyboard)FindResource("LongPressAnimation");
                LastPosition = new POINT
                {
                    X = (int)Left,
                    Y = (int)Top,
                };
                Log.Debug("拖动开始位置: X={LastX}, Y={LastY}", LastPosition.X, LastPosition.Y);

                LongPressAnimation.Begin();
                CaptureMouse();

                // 启动长按计时
                _longPressTimer.Start();
                Ab.Stop();
                Log.Debug("长按计时器启动，自动吸附定时器停止");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理鼠标左键按下事件时发生错误");
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Log.Debug("结束拖动Bird窗口");
                // 结束拖动
                _isDragging = false;
                GlobalVariablesData.config.BirdSettings.StartLocationX = Left;
                GlobalVariablesData.config.BirdSettings.StartLocationY = Top;
                GlobalVariablesData.SaveConfig();
                Log.Information("保存窗口位置: X={Left}, Y={Top}", Left, Top);

                ReleaseMouseCapture();
                LongPressAnimation.Stop();
                LongPressAnimation.Remove();
                _longPressTimer.Stop();

                // 自动吸附
                SnapToEdges(1);
                Ab.Start();
                Log.Debug("拖动结束，启动自动吸附定时器");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理鼠标左键释放事件时发生错误");
            }
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_isDragging) return;

            try
            {
                var dpiScale = GetDpiScaleFactor();
                var mousePos = e.GetPosition(this);
                var screenPoint = PointToScreen(mousePos);

                Left = (screenPoint.X / dpiScale) - _dragOffset.X;
                Top = (screenPoint.Y / dpiScale) - _dragOffset.Y;

                Log.Debug("拖动中，新位置: X={Left}, Y={Top}", Left, Top);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理鼠标移动事件时发生错误");
            }
        }

        private void LongPress_Tick(object sender, EventArgs e)
        {
            try
            {
                Log.Debug("长按计时器触发");
                _longPressTimer.Stop();
                LongPressAnimation.Stop();
                LongPressAnimation.Remove();
                var endTheLongPressAnimation = FindResource("EndTheLongPressAnimation") as Storyboard;
                endTheLongPressAnimation.Begin();

                double moveDistance = Math.Max(Math.Abs(LastPosition.X - Left), Math.Abs(LastPosition.Y - Top));
                Log.Debug("长按移动距离: {MoveDistance}，误判阈值: {Misjudgment}", moveDistance, GlobalVariablesData.config.BirdSettings.LongPressMisjudgment);

                if ((GlobalVariablesData.config.BirdSettings.StartWay == 2 || GlobalVariablesData.config.BirdSettings.StartWay == 4) && moveDistance <= GlobalVariablesData.config.BirdSettings.LongPressMisjudgment)
                {
                    Log.Information("长按触发启动主窗口，启动方式: {StartWay}", GlobalVariablesData.config.BirdSettings.StartWay);
                    ShowMainWindowAsync();
                }
                else
                {
                    Log.Debug("长按未触发启动（条件不满足）");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理长按计时器事件时发生错误");
            }
        }

        private void SnapToEdges(int get)
        {
            try
            {
                Log.Debug("开始吸附到边缘，模式: {SnapMode}", get);
                var screen = GetCurrentScreen();
                double rightEdge = Left + Width;
                double bottomEdge = Top + Height;

                // 水平吸附
                if (Math.Abs(Left - screen.Left) < SnapThreshold)
                {
                    Left = screen.Left;
                    Log.Debug("吸附到左侧边缘");
                }
                else if (Math.Abs(screen.Right - rightEdge) < SnapThreshold)
                {
                    Left = screen.Right - Width;
                    Log.Debug("吸附到右侧边缘");
                }

                if (get == 1)
                {
                    // 垂直吸附
                    if (Math.Abs(Top - screen.Top) < SnapThreshold)
                    {
                        Top = screen.Top;
                        Log.Debug("吸附到顶部边缘");
                    }
                    else if (Math.Abs(screen.Bottom - bottomEdge) < SnapThreshold)
                    {
                        Top = screen.Bottom - Height;
                        Log.Debug("吸附到底部边缘");
                    }
                }

                Log.Debug("吸附完成，最终位置: X={Left}, Y={Top}", Left, Top);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "吸附到边缘时发生错误");
            }
        }

        private Rect GetCurrentScreen()
        {
            try
            {
                // 获取窗口中心点对应的屏幕
                var centerX = Left + Width / 2;
                var centerY = Top + Height / 2;

                foreach (var screen in System.Windows.Forms.Screen.AllScreens)
                {
                    if (screen.Bounds.Contains((int)centerX, (int)centerY))
                    {
                        Log.Debug("找到当前屏幕: {ScreenName}", screen.DeviceName);
                        return new Rect(
                            screen.WorkingArea.Left,
                            screen.WorkingArea.Top,
                            screen.WorkingArea.Width,
                            screen.WorkingArea.Height);
                    }
                }
                Log.Warning("未找到对应屏幕，使用主屏幕工作区");
                return SystemParameters.WorkArea; // 默认主屏幕
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取当前屏幕时发生错误");
                return SystemParameters.WorkArea;
            }
        }

        private async void ShowMainWindowAsync()
        {
            try
            {
                Log.Information("开始显示主窗口");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow == null)
                    {
                        Log.Debug("主窗口不存在，创建新实例");
                        mainWindow = new MainWindow();
                    }

                    if (mainWindow != null)
                    {
                        mainWindow.ShowThisWindow();
                        mainWindow.NavigationMenu.Navigate(typeof(Mode.Home));
                        Log.Information("主窗口已显示并导航到主页");
                    }
                    else
                    {
                        Log.Error("无法创建或获取主窗口实例");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "显示主窗口时发生错误");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                Log.Information("Bird窗口正在关闭");
                if (_longPressTimer != null)
                {
                    _longPressTimer.Stop();
                    Log.Debug("长按计时器已停止");
                }
                base.OnClosed(e);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Bird窗口关闭时发生错误");
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Log.Information("Bird窗口正在关闭，已取消关闭操作");
                e.Cancel = true;
                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理Bird窗口关闭事件时发生错误");
            }
        }

        private void HideTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Log.Debug("隐藏矩形定时器触发");
                this.Dispatcher.Invoke(new Action(() =>
                {
                    rec1.Visibility = Visibility.Collapsed;
                    rec2.Visibility = Visibility.Collapsed;
                    rec3.Visibility = Visibility.Collapsed;
                    rec4.Visibility = Visibility.Collapsed;
                    Log.Debug("矩形已隐藏");
                }));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "隐藏矩形定时器处理时发生错误");
            }
        }

        public void Initialize()
        {
            try
            {
                Log.Information("开始初始化Bird窗口配置");
                if (GlobalVariablesData.config.BirdSettings.UseDefinedImage)
                {
                    Log.Debug("使用自定义图片");
                    string imagePath = Path.Combine(GlobalVariablesData.userDataDir, "Bird_data", "Image", "image.png");

                    if (File.Exists(imagePath))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imagePath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        ImageBox.Source = bitmap;
                        Log.Debug("自定义图片加载成功: {ImagePath}", imagePath);
                    }
                    else
                    {
                        Log.Warning("自定义图片文件不存在: {ImagePath}，使用默认图片", imagePath);
                        ImageBox.Source = new BitmapImage(new Uri("pack://application:,,,/BallPicture.png"));
                        GlobalVariablesData.config.BirdSettings.UseDefinedImage = false;
                    }
                }
                else
                {
                    Log.Debug("使用默认图片");
                    ImageBox.Source = new BitmapImage(new Uri("pack://application:,,,/BallPicture.png"));
                }

                SnapThreshold = GlobalVariablesData.config.BirdSettings.AdsorbValue;
                ImageBox.Opacity = GlobalVariablesData.config.BirdSettings.diaphaneity.ToDouble() / 100;
                ImageBox.Height = GlobalVariablesData.config.BirdSettings.Height;
                ImageBox.Width = GlobalVariablesData.config.BirdSettings.Width;
                progressRing.Height = Math.Min(ImageBox.Height, ImageBox.Width);
                progressRing.Width = Math.Min(ImageBox.Height, ImageBox.Width);
                Width = GlobalVariablesData.config.BirdSettings.Width + 20;
                Height = GlobalVariablesData.config.BirdSettings.Height + 20;

                Log.Information("Bird窗口配置初始化完成，尺寸: {Width}x{Height}，透明度: {Opacity}%，吸附阈值: {AdsorbValue}",
                    Width, Height, GlobalVariablesData.config.BirdSettings.diaphaneity, SnapThreshold);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化Bird窗口配置时发生错误");
            }
        }

        public void ShowReRectangle()
        {
            try
            {
                Log.Information("显示矩形框");
                Hidetimer.Stop();
                rec1.Visibility = Visibility.Visible;
                rec2.Visibility = Visibility.Visible;
                rec3.Visibility = Visibility.Visible;
                rec4.Visibility = Visibility.Visible;
                ImageBox.Height = GlobalVariablesData.config.BirdSettings.Height;
                ImageBox.Width = GlobalVariablesData.config.BirdSettings.Width;
                Width = GlobalVariablesData.config.BirdSettings.Width + 20;
                Height = GlobalVariablesData.config.BirdSettings.Height + 20;
                progressRing.Height = Math.Min(ImageBox.Height, ImageBox.Width);
                progressRing.Width = Math.Min(ImageBox.Height, ImageBox.Width);
                Hidetimer.Interval = 3000;
                Hidetimer.Start();
                Log.Debug("矩形框已显示，3秒后自动隐藏");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "显示矩形框时发生错误");
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("菜单点击：打开主窗口");
                ShowMainWindowAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理菜单打开事件时发生错误");
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("菜单点击：退出应用");
                ExitApp();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理菜单退出事件时发生错误");
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Log.Debug("Bird窗口已关闭");
        }
    }
}