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
        private NotifyIcon _notifyIcon;
        private double SnapThreshold = 60; // 吸附阈值
        private DispatcherTimer _longPressTimer;
        private Point _dragOffset;
        private bool _isDragging;
        System.Timers.Timer PowerOffTimer = new System.Timers.Timer();
        System.Timers.Timer Ab = new System.Timers.Timer();
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
            Initialize();
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
            PowerOffTimer.Interval = 5000;
            PowerOffTimer.Elapsed += PowerOffTimer_Elapsed;
            PowerOffTimer.Start();
            Ab.Interval = 10000;
            Ab.Elapsed += Ab_Elapsed;
            Directory.CreateDirectory(Path.Combine(GlobalVariables.configDir, "Bird_data", "Image"));
            if (GlobalVariables.json.StartToDo.AlwaysCleanMemory)
            {
                _timer.Start();
            }
        }

        private void Ab_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SnapThreshold = SystemParameters.PrimaryScreenWidth/2;

            this.Dispatcher.Invoke(new Action(() =>
            {
                SnapToEdges(0);
                SnapThreshold = GlobalVariables.json.BirdSettings.AdsorbValue;
            }));
            
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
            contextMenu.Items.Add("小工具", null, (s, e) => ShowToolboxWindowAsync());
            contextMenu.Items.Add("设置", null, (s, e) => ShowSettingsWindowAsync());
            contextMenu.Items.Add("重启", null, (s, e) => Restart());
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
            MouseLeftButtonDown += Bird_MouseLeftButtonDown;

        }

        private void Bird_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(GlobalVariables.json.BirdSettings.StartWay==0|| GlobalVariables.json.BirdSettings.StartWay == 3)
            {
                ShowMainWindowAsync();
            }
        }

        private void Bird_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(GlobalVariables.json.BirdSettings.StartWay==1|| GlobalVariables.json.BirdSettings.StartWay == 3|| GlobalVariables.json.BirdSettings.StartWay == 4)
            {
                ShowMainWindowAsync();
            }

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
            Ab.Stop();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 结束拖动
            _isDragging = false;
            ReleaseMouseCapture();
            _longPressTimer.Stop();

            // 自动吸附
            SnapToEdges(1);
            Ab.Start();
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
            if(GlobalVariables.json.BirdSettings.StartWay==2||GlobalVariables.json.BirdSettings.StartWay==4)
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

        private async Task ShowMainWindowAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Show();
                    mainWindow.Activate();
                    mainWindow.NavigationMenu.Navigate(typeof(Mode.OnePeopleMode));
                }
            });
        }
        private async Task ShowSettingsWindowAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
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
            });
        }
        private async Task ShowToolboxWindowAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
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
            });
        }
        private void Restart()
        {
            string[] args = Environment.GetCommandLineArgs();
            File.WriteAllText(Path.Combine(GlobalVariables.configDir, "START"), "The cake is a lie");
            Application.Current.Shutdown();
            Process.Start(Application.ResourceAssembly.Location, string.Join(" ", args.Skip(1)));
        }
        protected override void OnClosed(EventArgs e)
        {
            _longPressTimer.Stop();
            base.OnClosed(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
        }
        private void PowerOffTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DateTime.Now.ToString("H,m") ==
                GlobalVariables.json.StartToDo.HourPowerOff.ToString() + "," + GlobalVariables.json.StartToDo.MinPowerOff.ToString() &&
                GlobalVariables.json.StartToDo.PowerOff
                )
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    PowerOffWindow powerOffWindow = new PowerOffWindow();
                    powerOffWindow.Show();

                }));
                PowerOffTimer.Stop();

            }
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
                ImageBox.Source = new BitmapImage(new Uri("pack://application:,,,/icon.ico"));
            }
            SnapThreshold=GlobalVariables.json.BirdSettings.AdsorbValue;
            ImageBox.Opacity=GlobalVariables.json.BirdSettings.diaphaneity.ToDouble()/100;


        }
    }
}
