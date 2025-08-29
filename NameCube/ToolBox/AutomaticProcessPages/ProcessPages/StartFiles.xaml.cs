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
    /// StartFiles.xaml 的交互逻辑
    /// </summary>
    public partial class StartFiles : Page
    {
        private string pathToStart;
        private bool debug;
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

        public StartFiles(string path, bool isDebug = false, bool show = true)
        {
            InitializeComponent();
            pathToStart = path;
            debug = isDebug;
            if(!show)
            {
                Page_Loaded(null,null);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(pathToStart);
                if(debug)
                {
                    CallParentMethodDebug("启动文件成功");
                }
                else
                {
                    CallEndThePage();
                }

            }
            catch (Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError(ex.Message, true, ex);
                if (debug)
                {
                    CallParentMethodDebug($"启动文件失败: {ex.Message}");
                }
                else
                {
                    //无视风险，继续运行！（bushi
                    CallEndThePage();
                }
            }
        }
    }
}
