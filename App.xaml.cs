using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace NameCube
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    /// 
    public class Json
    {
        public List<string> Name { get; set; }
        public bool Speech { get; set; }
        public bool Dark { get; set; }
        public int Volume { get; set; }
        public int Speed { get; set; }
        public bool Wait { get; set; }
        public bool Ball { get; set; }
        public bool Start {  get; set; }
        public bool AlwaysCleanMemory { get; set; }
    }
    public partial class App : Application
    {
        Mutex mutex;
        public App()
        {
            this.Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            bool ret;
            mutex = new Mutex(true, "NameCube", out ret);
            if (!ret)
            {
                MessageBox.Show("哥们，你已经启动一个实例了,看看系统托盘吧（笑");
                Environment.Exit(0);
            }
            MainWindow mainWindow = new MainWindow();
        }
    }
}
