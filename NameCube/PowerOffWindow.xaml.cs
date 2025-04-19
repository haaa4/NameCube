using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NameCube
{
    /// <summary>
    /// PowerOffWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PowerOffWindow
    {
        int time = 30;
        System.Timers.Timer timer=new System.Timers.Timer();
        public PowerOffWindow()
        {
            InitializeComponent();
            this.Topmost = true;
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                PowerOffButton.Content = "继续（" + time.ToString() + "s)";
            }));
            time--;
            if(time==-1)
            {
                timer.Stop();
                //关机
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe"; // 使用cmd.exe
                startInfo.Arguments = "/c shutdown /s /t 0"; // 参数：/s 表示关机，/t 0 表示立即关机
                startInfo.CreateNoWindow = true; // 不显示命令行窗口
                startInfo.UseShellExecute = false; // 不使用Shell启动进程

                // 创建并启动进程
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit(); // 等待进程退出
                }
            }
        }

        private void PowerOffButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            //关机
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe"; // 使用cmd.exe
            startInfo.Arguments = "/c shutdown /s /t 0 "; // 参数：/s 表示关机，/t 0 表示立即关机
            startInfo.CreateNoWindow = true; // 不显示命令行窗口
            startInfo.UseShellExecute = false; // 不使用Shell启动进程

            // 创建并启动进程
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit(); // 等待进程退出
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            this.Close();
        }
    }
}
