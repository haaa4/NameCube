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
using Serilog;  // 添加Serilog命名空间

namespace NameCube
{
    /// <summary>
    /// Notify.xaml 的交互逻辑
    /// </summary>
    public partial class Notify : FluentWindow
    {
        private NotifyIcon _notifyIcon;
        private Storyboard _currentStoryboard;
        public Notify()
        {
            try
            {
                Log.Information("初始化通知窗口");
                InitializeComponent();
                if (GlobalVariables.json.AllSettings.NameCubeMode == 0)
                {
                    Log.Debug("悬浮球模式，初始化托盘图标");
                    InitializeTrayIcon();
                }

                VersonText.Text = GlobalVariables.Version;
                Log.Debug("设置版本文本: {Version}", GlobalVariables.Version);

                if (_currentStoryboard != null)
                {
                    _currentStoryboard.Stop();
                }

                if (this.IsVisible)
                {
                    this.Hide();
                    // 强制重置到屏幕外
                    this.Top = SystemParameters.PrimaryScreenHeight + 300;
                    Log.Debug("窗口已隐藏并重置位置");
                }

                // 获取DPI和鼠标位置
                var dpiScale = VisualTreeHelper.GetDpi(this);
                Point mousePos = GetLogicalPosition(this);
                Log.Debug("DPI缩放: {DpiScale}，鼠标位置: X={MouseX}, Y={MouseY}", dpiScale.DpiScaleX, mousePos.X, mousePos.Y);

                // 计算最终目标位置
                double targetLeft = mousePos.X - this.ActualWidth + 10;
                double targetTop = mousePos.Y - this.ActualHeight - 10;

                // 获取实际屏幕边界（考虑DPI和多显示器）
                var screenWidth = SystemParameters.WorkArea.Width / dpiScale.DpiScaleX;
                var screenHeight = SystemParameters.WorkArea.Height / dpiScale.DpiScaleY;

                // 边界检查
                // 水平边界
                if (targetLeft + this.ActualWidth > screenWidth)
                {
                    targetLeft = screenWidth - this.ActualWidth - 10;
                    Log.Debug("水平位置超出右边界，调整为: {TargetLeft}", targetLeft);
                }
                if (targetLeft < 0)
                {
                    targetLeft = 10;
                    Log.Debug("水平位置超出左边界，调整为: {TargetLeft}", targetLeft);
                }

                // 垂直边界
                if (targetTop < 0)
                {
                    targetTop = 10;
                    Log.Debug("垂直位置超出上边界，调整为: {TargetTop}", targetTop);
                }
                if (targetTop + this.ActualHeight > screenHeight)
                {
                    targetTop = screenHeight - this.ActualHeight - 10;
                    Log.Debug("垂直位置超出下边界，调整为: {TargetTop}", targetTop);
                }

                // 设置初始位置（必须从屏幕外开始）
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = targetLeft;
                this.Top = screenHeight + 300; // 从屏幕底部外开始
                this.Visibility = Visibility.Visible;
                Log.Debug("窗口初始位置设置: X={Left}, Y={Top} (屏幕外)", this.Left, this.Top);

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
                    Log.Debug("动画完成，窗口最终位置: X={Left}, Y={Top}", this.Left, this.Top);
                };

                // 使用工作区高度而不是主屏幕高度
                dpiScale = VisualTreeHelper.GetDpi(this);
                double screenBottom = SystemParameters.WorkArea.Bottom / dpiScale.DpiScaleY;

                this.Top = screenBottom + 300; // 重置到屏幕外
                this.Left = -this.Width - 100; // 同时水平方向移出
                Log.Debug("窗口位置重置到屏幕外");

                this.Hide();
                Log.Information("通知窗口初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化通知窗口时发生错误");
            }
        }

        public void InitializeLocation()
        {
            try
            {
                Log.Information("初始化通知窗口位置");
                // 如果窗口已显示，先隐藏重置
                if (this.IsVisible)
                {
                    this.Hide();
                    // 强制重置到屏幕外
                    this.Top = SystemParameters.PrimaryScreenHeight + 300;
                    Log.Debug("窗口已隐藏并重置位置");
                }

                // 获取DPI和鼠标位置
                var dpiScale = VisualTreeHelper.GetDpi(this);
                Point mousePos = GetLogicalPosition(this);
                Log.Debug("DPI缩放: {DpiScale}，鼠标位置: X={MouseX}, Y={MouseY}", dpiScale.DpiScaleX, mousePos.X, mousePos.Y);

                // 计算最终目标位置
                double targetLeft = mousePos.X - this.ActualWidth + 10;
                double targetTop = mousePos.Y - this.ActualHeight - 10;

                // 获取实际屏幕边界（考虑DPI和多显示器）
                var screenWidth = SystemParameters.WorkArea.Width / dpiScale.DpiScaleX;
                var screenHeight = SystemParameters.WorkArea.Height / dpiScale.DpiScaleY;

                // 边界检查
                // 水平边界
                if (targetLeft + this.ActualWidth > screenWidth)
                {
                    targetLeft = screenWidth - this.ActualWidth - 10;
                    Log.Debug("水平位置超出右边界，调整为: {TargetLeft}", targetLeft);
                }
                if (targetLeft < 0)
                {
                    targetLeft = 10;
                    Log.Debug("水平位置超出左边界，调整为: {TargetLeft}", targetLeft);
                }

                // 垂直边界
                if (targetTop < 0)
                {
                    targetTop = 10;
                    Log.Debug("垂直位置超出上边界，调整为: {TargetTop}", targetTop);
                }
                if (targetTop + this.ActualHeight > screenHeight)
                {
                    targetTop = screenHeight - this.ActualHeight - 10;
                    Log.Debug("垂直位置超出下边界，调整为: {TargetTop}", targetTop);
                }

                // 设置初始位置（必须从屏幕外开始）
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = targetLeft;
                this.Top = screenHeight + 300; // 从屏幕底部外开始
                this.Visibility = Visibility.Visible;
                Log.Debug("窗口初始位置设置: X={Left}, Y={Top} (屏幕外)", this.Left, this.Top);

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
                    Log.Debug("停止现有动画并重置位置");
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
                    Log.Debug("动画完成，窗口最终位置: X={Left}, Y={Top}", this.Left, this.Top);
                };

                this.Show();
                this.Activate();
                _currentStoryboard.Begin();
                Log.Information("通知窗口显示动画开始");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化通知窗口位置时发生错误");
            }
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
            try
            {
                NativePoint nativePoint;
                GetCursorPos(out nativePoint);
                Log.Debug("获取屏幕物理坐标: X={NativeX}, Y={NativeY}", nativePoint.X, nativePoint.Y);
                return new Point(nativePoint.X, nativePoint.Y);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "获取屏幕物理坐标时发生错误");
                return new Point(0, 0);
            }
        }

        // 转换为考虑DPI缩放的逻辑坐标
        public static Point GetLogicalPosition(Visual visual)
        {
            try
            {
                Point screenPos = GetScreenPosition();
                PresentationSource source = PresentationSource.FromVisual(visual);
                if (source != null)
                {
                    Matrix transform = source.CompositionTarget.TransformFromDevice;
                    Point logicalPos = transform.Transform(screenPos);
                    Log.Debug("转换到逻辑坐标: 物理({ScreenX},{ScreenY}) -> 逻辑({LogicalX},{LogicalY})",
                        screenPos.X, screenPos.Y, logicalPos.X, logicalPos.Y);
                    return logicalPos;
                }
                Log.Warning("无法获取PresentationSource，返回原始屏幕坐标");
                return screenPos;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "转换到逻辑坐标时发生错误");
                return new Point(0, 0);
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            try
            {
                Log.Information("通知窗口失去焦点，隐藏窗口");
                // 重置位置到屏幕外
                // 停止动画并强制重置位置
                if (_currentStoryboard != null)
                {
                    _currentStoryboard.Stop();
                    Log.Debug("停止当前动画");
                }

                // 使用工作区高度而不是主屏幕高度
                var dpiScale = VisualTreeHelper.GetDpi(this);
                double screenBottom = SystemParameters.WorkArea.Bottom / dpiScale.DpiScaleY;

                this.Top = screenBottom + 300; // 重置到屏幕外
                this.Left = -this.Width - 100; // 同时水平方向移出
                Log.Debug("窗口位置重置到屏幕外: X={Left}, Y={Top}", this.Left, this.Top);

                this.Hide();
                Log.Information("通知窗口已隐藏");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理通知窗口失去焦点事件时发生错误");
            }
        }

        private async void ShowMainWindowAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("通知窗口：显示主窗口");
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
                        mainWindow.WindowState = WindowState.Normal;
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
                Log.Error(ex, "通知窗口显示主窗口时发生错误");
            }
        }

        private async void ShowSettingsWindowAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("通知窗口：显示设置窗口");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var settingsWindow = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();

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
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "通知窗口显示设置窗口时发生错误");
            }
        }

        private async void ShowToolboxWindowAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("通知窗口：显示工具箱窗口");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var toolboxWindow = Application.Current.Windows.OfType<ToolBox.ToolboxWindow>().FirstOrDefault();

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
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "通知窗口显示工具箱窗口时发生错误");
            }
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("通知窗口：退出应用");
                _notifyIcon.Dispose();
                Application.Current.Shutdown(); // 手动关闭应用
            }
            catch (Exception ex)
            {
                Log.Error(ex, "通知窗口退出应用时发生错误");
            }
        }

        private void InitializeTrayIcon()
        {
            try
            {
                Log.Information("初始化托盘图标");
                _notifyIcon = new NotifyIcon
                {
                    Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath),
                    Visible = true,
                    Text = "点鸣魔方"
                };
                Log.Debug("托盘图标设置: 可见={Visible}，文本={Text}", _notifyIcon.Visible, _notifyIcon.Text);

                _notifyIcon.Click += (s, e) =>
                {
                    try
                    {
                        Log.Information("托盘图标被点击");
                        // 确保在主线程操作
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (!this.IsVisible)
                            {
                                Log.Debug("通知窗口未显示，初始化位置并显示");
                                InitializeLocation();
                            }
                            else
                            {
                                Log.Debug("通知窗口已显示，隐藏窗口");
                                this.Hide();
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "处理托盘图标点击事件时发生错误");
                    }
                };

                Log.Information("托盘图标初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化托盘图标时发生错误");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("通知窗口：重启应用");
                _notifyIcon.Dispose();
                AppFunction.Restart();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "通知窗口重启应用时发生错误");
            }
        }
    }
}