using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NameCube.Function;
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
    /// ClearPage.xaml 的交互逻辑
    /// </summary>
    public partial class ClearPage : Page
    {
        private bool debug;
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

        public ClearPage(bool isDubug = false, bool show = true)
        {
            Log.Information("初始化清理内存页面 - 调试模式: {Debug}, 显示: {Show}", isDubug, show);
            InitializeComponent();
            debug = isDubug;
            if (!show)
            {
                Log.Debug("页面设置为不显示模式，自动触发加载");
                Page_Loaded(null, null);
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Information("开始清理内存");
            await Task.Delay(1000);
            try
            {
                Log.Debug("调用Windows内存清理API");
                Masuit.Tools.Win32.Windows.ClearMemory();
                Log.Information("内存清理完成");
                if (debug)
                {
                    CallParentMethodDebug("清理完成");
                }
                else
                {
                    CallEndThePage();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "清理内存时发生错误");
                MessageBoxFunction.ShowMessageBoxError(ex.Message, true, ex);
                if (debug)
                {
                    CallParentMethodDebug(ex.Message);
                }
                else
                {
                    CallEndThePage();
                }
            }
        }
    }
}