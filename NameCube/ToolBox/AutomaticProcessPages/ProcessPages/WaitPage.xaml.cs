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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Serilog;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessPages
{
    /// <summary>
    /// WaitPage.xaml 的交互逻辑
    /// </summary>
    public partial class WaitPage : Page
    {
        private int _time = 0;
        private bool _debug = false;
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

        public WaitPage(int time, bool debug = false, bool show = true)
        {
            Log.Information("初始化等待页面 - 等待时间: {Time}秒, 调试模式: {Debug}, 显示: {Show}",
                time, debug, show);
            InitializeComponent();
            _time = time;
            _debug = debug;
            if (!show)
            {
                Log.Debug("页面设置为不显示模式，自动触发加载");
                Page_Loaded(null, null);
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Information("开始等待 {Time} 秒", _time);
            timeText.Text = _time.ToString();
            while (_time > 0)
            {
                await Task.Delay(1000);
                _time--;
                timeText.Text = _time.ToString();
                Log.Debug("等待倒计时: {Time}秒", _time);
            }
            Log.Information("等待结束");
            if (_debug)
            {
                CallParentMethodDebug("等待结束");
            }
            else
            {
                CallEndThePage();
            }
        }
    }
}