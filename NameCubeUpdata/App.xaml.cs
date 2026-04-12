using NameCubeSetup;
using System.Windows;

//接受这里的拼写错误，反正也不影响什么，哈哈哈
namespace NameCubeUpdata
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0 && e.Args[0] == "UpdateMode")
            {
                // 如果启动参数包含 "UpdateMode"，则直接打开更新窗口
                UpdateGuideWindow updateWindow = new UpdateGuideWindow();
                updateWindow.Show();
            }
            else
            {
                // 否则正常启动应用程序
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }
}