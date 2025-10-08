using Masuit.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NameCube.WarningWindows
{
    /// <summary>
    /// ErrorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorWindow
    {
        public ErrorWindow(Exception exception)
        {
            InitializeComponent();
            string errorMessage = $"发生时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                                $"异常类型: {exception.GetType().Name}\n\n" +
                                $"异常消息:\n{exception.Message}\n\n" +
                                $"堆栈跟踪:\n{exception.StackTrace}\n\n" +
                                $"完整异常信息:\n{exception}";
            LogManager.Error(exception);
            ErrorTextBox.Text = errorMessage;
        }
        public ErrorWindow(string errorMessage, string title = "错误")
        {
            InitializeComponent();
            this.Title = title;
            ErrorTextBox.Text = errorMessage;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            AppFunction.Restart();
        }
    }
}
