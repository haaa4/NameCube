using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessPages
{
    /// <summary>
    /// PowerOffPage.xaml 的交互逻辑
    /// </summary>
    public partial class PowerOffPage : Page
    {
        int time = 30;
        System.Timers.Timer timer = new System.Timers.Timer();
        private int shutDown = 0;
        private bool debug = false;
        public event Action<string> RequestParentAction;
        public event Action<int> EndThePageAction;
        private void CallEnd(string debugText,bool debug,int ret=0)
        {
            if(debug)
            {
                CallParentMethodDebug(debugText);
            }
            else
            {
                CallEndThePage(ret);
            }
        }
        private void CallEndThePage(int ret = 0)
        {
            EndThePageAction?.Invoke(ret);
        }
        private void CallParentMethodDebug(string data)
        {
            RequestParentAction?.Invoke(data);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="shutDownWay">(0:立刻关机,1:一般关机，2:强制关机)</param>
        /// <param name="isDebug"></param>
        public PowerOffPage(int shutDownWay,bool isDebug=false,bool show = true)
        {
            InitializeComponent();
            shutDown = shutDownWay;
            debug=isDebug;
            debug = isDebug;
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                PowerOffButton.Content = "关机（" + time.ToString() + "s)";
            }));
            time--;
            if (time == -1)
            {
                timer.Stop();
                if(debug)
                {
                    if (shutDown == 0)
                    {
                        CallParentMethodDebug("立刻关机");
                    }
                    else if (shutDown == 1)
                    {
                        CallParentMethodDebug("一般关机");
                    }
                    else
                    {
                        CallParentMethodDebug("强制关机");
                    }
                    return;
                }
                //关机
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe"; // 使用cmd.exe
                if (shutDown == 0)
                {
                    startInfo.Arguments = "/c shutdown /s /t 0"; // 参数：/s 表示关机，/t 0 表示立即关机
                }
                else if (shutDown == 1)
                {
                    startInfo.Arguments = "/c shutdown /s"; // 参数：/s 表示关机，/t 0 表示立即关机
                }
                else
                {
                    startInfo.Arguments = "/c shutdown /s /f /t 0";
                }
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
            if (debug)
            {
                if (shutDown == 0)
                {
                    CallParentMethodDebug("立刻关机");
                }
                else if (shutDown == 1)
                {
                    CallParentMethodDebug("一般关机");
                }
                else
                {
                    CallParentMethodDebug("强制关机");
                }
                return;
            }
            //关机
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe"; // 使用cmd.exe
            if (shutDown == 0)
            {
                startInfo.Arguments = "/c shutdown /s /t 0"; // 参数：/s 表示关机，/t 0 表示立即关机
            }
            else if (shutDown == 1)
            {
                startInfo.Arguments = "/c shutdown /s"; // 参数：/s 表示关机，/t 0 表示立即关机
            }
            else
            {
                startInfo.Arguments = "/c shutdown /s /f /t 0";
            }
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
            CallEnd("用户没有关机", debug);
        }
    }
}
