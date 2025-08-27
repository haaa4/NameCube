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
            EndThePageAction?.Invoke(ret);
        }
        private void CallParentMethodDebug(string data)
        {
            RequestParentAction?.Invoke(data);
        }
        public CmdPage(string cmd,bool visibility,bool debug = false)
        {
            InitializeComponent();
            _cmd = cmd;
            _visibility = visibility;
            _debug = debug;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建ProcessStartInfo对象
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe"; // 设置文件名
                startInfo.Arguments = $"/k {_cmd}"; // 设置cmd命令和参数
                startInfo.RedirectStandardOutput = false; // 启用重定向输出
                startInfo.UseShellExecute = false; // 不使用Shell执行
                startInfo.CreateNoWindow = !_visibility; // 创建新窗口

                // 创建Process对象并启动
                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();

                }
            }
            catch(Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError(ex.Message, true, ex);
                if(_debug)
                {
                    CallParentMethodDebug("执行失败" + ex.Message);
                }
                else
                {
                    CallEndThePage();
                }
            }
            if(_debug)
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
