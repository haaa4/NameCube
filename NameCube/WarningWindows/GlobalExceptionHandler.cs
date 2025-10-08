using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NameCube.WarningWindows
{
    public class GlobalExceptionHandler
    {
        public static void Initialize()
        {
            Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true; 
            ShowErrorWindow(e.Exception, "UI线程异常");
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                ShowErrorWindow(exception, "非UI线程异常");
            }
        }

        private static void ShowErrorWindow(Exception exception, string errorType)
        {
           
            Application.Current.Dispatcher.Invoke(() =>
            {
                var errorWindow = new ErrorWindow(exception)
                {
                    Title = $"错误 - {errorType}"
                };
                errorWindow.ShowDialog();
            });
        }
    }
}
