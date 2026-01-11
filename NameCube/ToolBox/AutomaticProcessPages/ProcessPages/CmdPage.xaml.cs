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
    /// CmdPage.xaml 的交互逻辑
    /// </summary>
    public partial class CmdPage : Page
    {
        private string _cmd;
        private bool _visibility;
        private bool _debug;
        public event Action<string> RequestParentAction;
        public event Action<int> EndThePageAction;

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

        public CmdPage(string cmd, bool visibility, bool debug = false, bool show = true)
        {
            Log.Information("初始化CMD页面 - 命令: {Cmd}, 可见性: {Visibility}, 调试模式: {Debug}, 显示: {Show}",
                cmd, visibility, debug, show);
            InitializeComponent();
            _cmd = cmd;
            _visibility = visibility;
            _debug = debug;
            if (!show)
            {
                Log.Debug("页面设置为不显示模式，自动触发加载");
                Page_Loaded(null, null);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Information("开始执行CMD命令: {Cmd}, 窗口可见性: {Visibility}", _cmd, _visibility);
            try
            {
                // 创建ProcessStartInfo对象
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe"; // 设置文件名
                startInfo.Arguments = $"/k {_cmd}"; // 设置cmd命令和参数
                startInfo.RedirectStandardOutput = false; // 启用重定向输出
                startInfo.UseShellExecute = false; // 不使用Shell执行
                startInfo.CreateNoWindow = !_visibility; // 创建新窗口

                Log.Debug("启动CMD进程 - 文件名: {FileName}, 参数: {Arguments}, 创建窗口: {CreateNoWindow}",
                    startInfo.FileName, startInfo.Arguments, startInfo.CreateNoWindow);

                // 创建Process对象并启动
                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();
                    Log.Information("CMD进程启动成功，进程ID: {ProcessId}", process.Id);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "执行CMD命令失败");
                MessageBoxFunction.ShowMessageBoxError(ex.Message, true, ex);
                if (_debug)
                {
                    CallParentMethodDebug("执行失败" + ex.Message);
                }
                else
                {
                    CallEndThePage();
                }
            }

            Log.Information("CMD命令执行完成");
            if (_debug)
            {
                CallParentMethodDebug("执行完成");
            }
            else
            {
                CallEndThePage();
            }
        }
    }
}