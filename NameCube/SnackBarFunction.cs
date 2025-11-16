using NameCube.Setting;
using NameCube.ToolBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls;

namespace NameCube
{
    internal class SnackBarFunction
    {
        public static void ShowSnackBarInMainWindow(string text, ControlAppearance ControlAppearance, string title=null,int time=3)
        {
            if(title==null)
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
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => ShowSnackBarInMainWindow(text, ControlAppearance,title , time));
                return;
            }
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow!=null)
            {
                var snackbar = new Snackbar(mainWindow.SnackbarPresenterHost)
                {
                    Content = text, // 设置提示内容
                    Title = title,
                    Appearance = ControlAppearance,
                    Timeout = TimeSpan.FromSeconds(time) // 显示时长
                };
                snackbar.Show();
            }
            
        }
        public static void ShowSnackBarInSettingWindow(string text, ControlAppearance ControlAppearance, string title = null, int time = 3)
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
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => ShowSnackBarInMainWindow(text, ControlAppearance, title, time));
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
            }

        }
        public static void ShowSnackBarInToolBoxWindow(string text, ControlAppearance ControlAppearance, string title = null, int time = 3)
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
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => ShowSnackBarInMainWindow(text, ControlAppearance, title, time));
                return;
            }
            var toolboxWindow = Application.Current.Windows.OfType<ToolboxWindow>().FirstOrDefault();

            if (toolboxWindow == null)
            {
                var snackbar = new Snackbar(toolboxWindow.SnackbarPresenterHost)
                {
                    Content = text, // 设置提示内容
                    Title = title,
                    Appearance = ControlAppearance,
                    Timeout = TimeSpan.FromSeconds(time) // 显示时长
                };
                snackbar.Show();
            }

        }
    }
}
