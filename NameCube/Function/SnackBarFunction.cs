using NameCube.Setting;
using NameCube.ToolBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls;
using Serilog;  // 添加Serilog命名空间

namespace NameCube
{
    internal class SnackBarFunction
    {
        public static void ShowSnackBarInMainWindow(string text, ControlAppearance ControlAppearance, string title = null, int time = 3)
        {
            try
            {
                if (title == null)
                {
                    switch (ControlAppearance)
                    {
                        case ControlAppearance.Primary:
                            title = "提示";
                            break;
                        case ControlAppearance.Secondary:
                            title = "提示";
                            break;
                        case ControlAppearance.Info:
                            title = "信息";
                            break;
                        case ControlAppearance.Success:
                            title = "成功";
                            break;
                        case ControlAppearance.Caution:
                            title = "警告";
                            break;
                        case ControlAppearance.Danger:
                            title = "错误";
                            break;
                        default:
                            title = "信息";
                            break;
                    }
                }

                Log.Information("显示主窗口SnackBar: 标题={Title}，内容={Text}，外观={Appearance}，时长={Time}秒",
                    title, text, ControlAppearance, time);

                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.Invoke(() => ShowSnackBarInMainWindow(text, ControlAppearance, title, time));
                    return;
                }

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    var snackbar = new Snackbar(mainWindow.SnackbarPresenterHost)
                    {
                        Content = text, // 设置提示内容
                        Title = title,
                        Appearance = ControlAppearance,
                        Timeout = TimeSpan.FromSeconds(time) // 显示时长
                    };
                    snackbar.Show();
                    Log.Debug("主窗口SnackBar已显示");
                }
                else
                {
                    Log.Warning("无法获取主窗口实例，无法显示SnackBar");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "显示主窗口SnackBar时发生错误");
            }
        }

        public static void ShowSnackBarInSettingWindow(string text, ControlAppearance ControlAppearance, string title = null, int time = 3)
        {
            try
            {
                if (title == null)
                {
                    switch (ControlAppearance)
                    {
                        case ControlAppearance.Primary:
                            title = "提示";
                            break;
                        case ControlAppearance.Secondary:
                            title = "提示";
                            break;
                        case ControlAppearance.Info:
                            title = "信息";
                            break;
                        case ControlAppearance.Success:
                            title = "成功";
                            break;
                        case ControlAppearance.Caution:
                            title = "警告";
                            break;
                        case ControlAppearance.Danger:
                            title = "错误";
                            break;
                        default:
                            title = "信息";
                            break;
                    }
                }

                Log.Information("显示设置窗口SnackBar: 标题={Title}，内容={Text}，外观={Appearance}，时长={Time}秒",
                    title, text, ControlAppearance, time);

                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.Invoke(() => ShowSnackBarInSettingWindow(text, ControlAppearance, title, time));
                    return;
                }

                var settingsWindow = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();

                if (settingsWindow != null)
                {
                    var snackbar = new Snackbar(settingsWindow.SnackbarPresenterHost)
                    {
                        Content = text, // 设置提示内容
                        Title = title,
                        Appearance = ControlAppearance,
                        Timeout = TimeSpan.FromSeconds(time) // 显示时长
                    };
                    snackbar.Show();
                    Log.Debug("设置窗口SnackBar已显示");
                }
                else
                {
                    Log.Warning("无法获取设置窗口实例，无法显示SnackBar");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "显示设置窗口SnackBar时发生错误");
            }
        }

        public static void ShowSnackBarInToolBoxWindow(string text, ControlAppearance ControlAppearance, string title = null, int time = 3)
        {
            try
            {
                if (title == null)
                {
                    switch (ControlAppearance)
                    {
                        case ControlAppearance.Primary:
                            title = "提示";
                            break;
                        case ControlAppearance.Secondary:
                            title = "提示";
                            break;
                        case ControlAppearance.Info:
                            title = "信息";
                            break;
                        case ControlAppearance.Success:
                            title = "成功";
                            break;
                        case ControlAppearance.Caution:
                            title = "警告";
                            break;
                        case ControlAppearance.Danger:
                            title = "错误";
                            break;
                        default:
                            title = "信息";
                            break;
                    }
                }

                Log.Information("显示工具箱窗口SnackBar: 标题={Title}，内容={Text}，外观={Appearance}，时长={Time}秒",
                    title, text, ControlAppearance, time);

                if (!Application.Current.Dispatcher.CheckAccess())
                {
                    Application.Current.Dispatcher.Invoke(() => ShowSnackBarInToolBoxWindow(text, ControlAppearance, title, time));
                    return;
                }

                var toolboxWindow = Application.Current.Windows.OfType<ToolboxWindow>().FirstOrDefault();

                if (toolboxWindow != null)
                {
                    var snackbar = new Snackbar(toolboxWindow.SnackbarPresenterHost)
                    {
                        Content = text, // 设置提示内容
                        Title = title,
                        Appearance = ControlAppearance,
                        Timeout = TimeSpan.FromSeconds(time) // 显示时长
                    };
                    snackbar.Show();
                    Log.Debug("工具箱窗口SnackBar已显示");
                }
                else
                {
                    Log.Warning("无法获取工具箱窗口实例，无法显示SnackBar");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "显示工具箱窗口SnackBar时发生错误");
            }
        }
    }
}