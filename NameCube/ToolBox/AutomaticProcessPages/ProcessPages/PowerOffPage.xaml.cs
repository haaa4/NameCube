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
using Serilog;

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

        private void CallEnd(string debugText, bool debug, int ret = 0)
        {
            Log.Debug("调用结束方法 - 调试文本: {DebugText}, 调试模式: {Debug}, 返回码: {ReturnCode}",
                debugText, debug, ret);
            if (debug)
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
            Log.Debug("调用结束页面，返回码: {ReturnCode}", ret);
            EndThePageAction?.Invoke(ret);
        }

        private void CallParentMethodDebug(string data)
        {
            Log.Debug("调用父页面调试方法，数据: {Data}", data);
            RequestParentAction?.Invoke(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shutDownWay">(0:立刻关机,1:一般关机，2:强制关机)</param>
        /// <param name="isDebug"></param>
        public PowerOffPage(int shutDownWay, bool isDebug = false, bool show = true)
        {
            Log.Information("初始化关机页面 - 关机方式: {ShutDownWay}, 调试模式: {Debug}, 显示: {Show}",
                GetShutDownWayName(shutDownWay), isDebug, show);
            InitializeComponent();
            shutDown = shutDownWay;
            debug = isDebug;
            debug = isDebug;
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            Log.Information("关机倒计时开始，初始时间: {Time}秒", time);
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
                Log.Information("关机倒计时结束，执行关机操作 - 关机方式: {ShutDownWay}",
                    GetShutDownWayName(shutDown));
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

                Log.Information("执行关机命令 - 参数: {Arguments}", startInfo.Arguments);

                // 创建并启动进程
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit(); // 等待进程退出
                    Log.Information("关机命令执行完成");
                }
            }
        }

        private void PowerOffButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击关机按钮 - 关机方式: {ShutDownWay}", GetShutDownWayName(shutDown));
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

            Log.Information("执行关机命令 - 参数: {Arguments}", startInfo.Arguments);

            // 创建并启动进程
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit(); // 等待进程退出
                Log.Information("关机命令执行完成");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击取消关机按钮");
            timer.Stop();
            CallEnd("用户没有关机", debug);
        }

        private string GetShutDownWayName(int way)
        {
            switch (way)
            {
                case 0: return "立刻关机";
                case 1: return "一般关机";
                case 2: return "强制关机";
                case 3: return "重启";
                default: return $"未知({way})";
            }
        }
    }
}