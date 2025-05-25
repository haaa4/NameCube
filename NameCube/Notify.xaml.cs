using NameCube.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Wpf.Ui.Controls;
using Wpf.Ui.Tray.Controls;
using Application = System.Windows.Application;
using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace NameCube
{
    /// <summary>
    /// Notify.xaml 的交互逻辑
    /// </summary>
    public partial class Notify:Window
    {
        private NotifyIcon _notifyIcon;
        private Storyboard _currentStoryboard;
        public Notify()
        {
            InitializeComponent();
            if (GlobalVariables.json.AllSettings.NameCubeMode == 0)
            {
                InitializeTrayIcon();
            }
            VersonText.Text = GlobalVariables.Version;
            if (_currentStoryboard != null)
            {
                _currentStoryboard.Stop();
            }
            if (this.IsVisible)
            {
                this.Hide();
                // 强制重置到屏幕外
                this.Top = SystemParameters.PrimaryScreenHeight + 300;
            }

            // 获取DPI和鼠标位置
            var dpiScale = VisualTreeHelper.GetDpi(this);
            Point mousePos = GetLogicalPosition(this);

            // 计算最终目标位置
            double targetLeft = mousePos.X - this.ActualWidth + 10;
            double targetTop = mousePos.Y - this.ActualHeight - 10;

            // 获取实际屏幕边界（考虑DPI和多显示器）
            var screenWidth = SystemParameters.WorkArea.Width / dpiScale.DpiScaleX;
            var screenHeight = SystemParameters.WorkArea.Height / dpiScale.DpiScaleY;

            // 边界检查
            // 水平边界
            if (targetLeft + this.ActualWidth > screenWidth)
                targetLeft = screenWidth - this.ActualWidth - 10;
            if (targetLeft < 0)
                targetLeft = 10;

            // 垂直边界
            if (targetTop < 0)
                targetTop = 10;
            if (targetTop + this.ActualHeight > screenHeight)
                targetTop = screenHeight - this.ActualHeight - 10;

            // 设置初始位置（必须从屏幕外开始）
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = targetLeft;
            this.Top = screenHeight + 300; // 从屏幕底部外开始
            this.Visibility = Visibility.Visible;

            // 创建动画
            var animation = new DoubleAnimation
            {
                From = screenHeight + 300, // 明确指定起始位置
                To = targetTop,
                Duration = TimeSpan.FromSeconds(0.6),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };

            // 动画控制
            if (_currentStoryboard != null)
            {
                _currentStoryboard.Stop();
                this.Top = screenHeight + 300; // 强制重置位置
            }

            _currentStoryboard = new Storyboard();
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(TopProperty));
            _currentStoryboard.Children.Add(animation);

            // 动画完成后保持位置
            _currentStoryboard.Completed += (s, e) =>
            {
                this.Top = targetTop;
                this.Left = targetLeft;
            };
            // 使用工作区高度而不是主屏幕高度
            dpiScale = VisualTreeHelper.GetDpi(this);
            double screenBottom = SystemParameters.WorkArea.Bottom / dpiScale.DpiScaleY;

            this.Top = screenBottom + 300; // 重置到屏幕外
            this.Left = -this.Width - 100; // 同时水平方向移出

            this.Hide();
        }
        public void InitializeLocation()
        {

            // 如果窗口已显示，先隐藏重置
            if (this.IsVisible)
            {
                this.Hide();
                // 强制重置到屏幕外
                this.Top = SystemParameters.PrimaryScreenHeight + 300;
            }

            // 获取DPI和鼠标位置
            var dpiScale = VisualTreeHelper.GetDpi(this);
            Point mousePos = GetLogicalPosition(this);

            // 计算最终目标位置
            double targetLeft = mousePos.X - this.ActualWidth + 10;
            double targetTop = mousePos.Y - this.ActualHeight - 10;

            // 获取实际屏幕边界（考虑DPI和多显示器）
            var screenWidth = SystemParameters.WorkArea.Width / dpiScale.DpiScaleX;
            var screenHeight = SystemParameters.WorkArea.Height / dpiScale.DpiScaleY;

            // 边界检查
            // 水平边界
            if (targetLeft + this.ActualWidth > screenWidth)
                targetLeft = screenWidth - this.ActualWidth - 10;
            if (targetLeft < 0)
                targetLeft = 10;

            // 垂直边界
            if (targetTop < 0)
                targetTop = 10;
            if (targetTop + this.ActualHeight > screenHeight)
                targetTop = screenHeight - this.ActualHeight - 10;

            // 设置初始位置（必须从屏幕外开始）
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            this.Left = targetLeft;
            this.Top = screenHeight + 300; // 从屏幕底部外开始
            this.Visibility = Visibility.Visible;

            // 创建动画
            var animation = new DoubleAnimation
            {
                From = screenHeight + 300, // 明确指定起始位置
                To = targetTop,
                Duration = TimeSpan.FromSeconds(0.6),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };

            // 动画控制
            if (_currentStoryboard != null)
            {
                _currentStoryboard.Stop();
                this.Top = screenHeight + 300; // 强制重置位置
            }

            _currentStoryboard = new Storyboard();
            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath(TopProperty));
            _currentStoryboard.Children.Add(animation);

            // 动画完成后保持位置
            _currentStoryboard.Completed += (s, e) =>
            {
                this.Top = targetTop;
                this.Left = targetLeft;
            };

            this.Show();
            this.Activate();
            _currentStoryboard.Begin();
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out NativePoint lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct NativePoint
        {
            public int X;
            public int Y;
        }

        // 获取屏幕物理坐标
        public static Point GetScreenPosition()
        {
            NativePoint nativePoint;
            GetCursorPos(out nativePoint);
            return new Point(nativePoint.X, nativePoint.Y);
        }

        // 转换为考虑DPI缩放的逻辑坐标
        public static Point GetLogicalPosition(Visual visual)
        {
            Point screenPos = GetScreenPosition();
            PresentationSource source = PresentationSource.FromVisual(visual);
            if (source != null)
            {
                Matrix transform = source.CompositionTarget.TransformFromDevice;
                return transform.Transform(screenPos);
            }
            return screenPos;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // 重置位置到屏幕外
            // 停止动画并强制重置位置
            if (_currentStoryboard != null)
            {
                _currentStoryboard.Stop();
            }

            // 使用工作区高度而不是主屏幕高度
            var dpiScale = VisualTreeHelper.GetDpi(this);
            double screenBottom = SystemParameters.WorkArea.Bottom / dpiScale.DpiScaleY;

            this.Top = screenBottom + 300; // 重置到屏幕外
            this.Left = -this.Width - 100; // 同时水平方向移出

            this.Hide();
        }
        private async void ShowMainWindowAsync(object sender, RoutedEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Show();
                    mainWindow.Activate();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.NavigationMenu.Navigate(typeof(Mode.Home));
                }
            });
        }
        private async void ShowSettingsWindowAsync(object sender, RoutedEventArgs e)
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
        private async void ShowToolboxWindowAsync(object sender, RoutedEventArgs e)
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
        private void ExitApp(object sender, RoutedEventArgs e)
        {
            _notifyIcon.Dispose();
            Application.Current.Shutdown(); // 手动关闭应用
        }


        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath),
                Visible = true,
                Text = "点鸣魔方"
            };

            _notifyIcon.Click += (s, e) =>
            {
                // 确保在主线程操作
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!this.IsVisible)
                    {
                        InitializeLocation();
                    }
                    else
                    {
                        this.Hide();
                    }
                });
            };
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _notifyIcon.Dispose();
            AppFunction.Restart();
        }
    }
}
