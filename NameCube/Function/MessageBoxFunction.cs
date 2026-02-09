using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NameCube.Function
{
    internal class MessageBoxFunction
    {
        private static readonly ILogger _logger = Log.ForContext<MessageBoxFunction>();
        /// <summary>
        /// 创建一个提示信息框
        /// </summary>
        /// <param name="message">提醒消息</param>
        public static void ShowMessageBoxInfo(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Content = message;
                messageBox.Title = "提示";
                messageBox.CloseButtonText = "知道了";
                messageBox.ShowDialogAsync();
            });
        }

        /// <summary>
        /// 创建一个警告信息框
        /// </summary>
        /// <param name="message">警告消息</param>
        public static void ShowMessageBoxWarning(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Content = message;
                messageBox.Title = "警告";
                messageBox.CloseButtonText = "知道了";
                messageBox.ShowDialogAsync();
            });
        }

        /// <summary>
        /// 创建一个报错信息框
        /// </summary>
        /// <param name="message">报错信息</param>
        /// <param name="Log">是否写入日志</param
        /// <param name="exception">输入日志的报错（可选）</param>
        public static void ShowMessageBoxError(
            string message,
            bool Log = true,
            Exception exception = null
        )
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Content = message;
                if (Log)
                {
                    if (exception != null)
                    {
                        _logger.Error("严重错误", exception);
                    }
                    else
                    {
                        _logger.Error("严重错误", message);
                    }
                }
                messageBox.Title = "错误";
                messageBox.CloseButtonText = "知道了";
                messageBox.ShowDialogAsync();
            });
        }
    }
}
