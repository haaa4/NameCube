using Masuit.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NameCube.Function;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Serilog;

namespace NameCube.WarningWindows
{
    /// <summary>
    /// ErrorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorWindow
    {
        public ErrorWindow(Exception exception)
        {
            Log.Information("创建错误窗口显示异常 - 异常类型: {ExceptionType}", exception.GetType().Name);
            InitializeComponent();
            string errorMessage = $"发生时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                                $"异常类型: {exception.GetType().Name}\n\n" +
                                $"异常消息:\n{exception.Message}\n\n" +
                                $"堆栈跟踪:\n{exception.StackTrace}\n\n" +
                                $"完整异常信息:\n{exception}";

            // 使用新的Serilog日志系统替换旧的LogManager
            Log.Error(exception, "显示错误窗口捕获的异常");
            ErrorTextBox.Text = errorMessage;
            Log.Debug("错误窗口初始化完成，异常消息已显示");
        }

        public ErrorWindow(string errorMessage, string title = "错误")
        {
            Log.Information("创建错误窗口显示错误消息 - 标题: {Title}, 消息长度: {MessageLength}",
                title, errorMessage?.Length ?? 0);
            InitializeComponent();
            this.Title = title;
            ErrorTextBox.Text = errorMessage;
            Log.Debug("错误窗口初始化完成，自定义错误消息已显示");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击关闭错误窗口按钮");
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击退出应用程序按钮");
            Environment.Exit(1);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击重启应用程序按钮");
            AppFunction.Restart();
        }
    }
}