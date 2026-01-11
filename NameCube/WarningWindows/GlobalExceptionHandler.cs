using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Serilog;

namespace NameCube.WarningWindows
{
    public class GlobalExceptionHandler
    {
        public static void Initialize()
        {
            Log.Information("初始化全局异常处理器");
            try
            {
                Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                Log.Information("全局异常处理器初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化全局异常处理器时发生错误");
            }
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "捕获到UI线程未处理异常，类型: {ExceptionType}", e.Exception.GetType().Name);
            e.Handled = true;
            ShowErrorWindow(e.Exception, "UI线程异常");
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                Log.Error(exception, "捕获到非UI线程未处理异常，类型: {ExceptionType}", exception.GetType().Name);
                ShowErrorWindow(exception, "非UI线程异常");
            }
            else
            {
                Log.Error("捕获到未知类型的未处理异常对象: {ExceptionObject}", e.ExceptionObject?.ToString() ?? "null");
            }
        }

        private static void ShowErrorWindow(Exception exception, string errorType)
        {
            Log.Debug("显示错误窗口，错误类型: {ErrorType}", errorType);

            // 在UI线程中显示错误窗口
            if (Application.Current.Dispatcher.CheckAccess())
            {
                ShowErrorWindowInternal(exception, errorType);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowErrorWindowInternal(exception, errorType);
                });
            }
        }

        private static void ShowErrorWindowInternal(Exception exception, string errorType)
        {
            try
            {
                var errorWindow = new ErrorWindow(exception)
                {
                    Title = $"错误 - {errorType}"
                };
                errorWindow.ShowDialog();
                Log.Debug("错误窗口已显示");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "显示错误窗口时发生错误");
                // 如果无法显示错误窗口，至少记录到日志
                MessageBox.Show($"发生严重错误: {exception.Message}\n\n无法显示错误窗口: {ex.Message}",
                    "致命错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}