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
            EndThePageAction?.Invoke(ret);
        }
        private void CallParentMethodDebug(string data)
        {
            RequestParentAction?.Invoke(data);
        }
        public WaitPage(int time,bool debug=false,bool show=true)
        {
            InitializeComponent();
            _time = time;
            _debug = debug;
            if(!show)
            {
                Page_Loaded(null,null);
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            timeText.Text = _time.ToString();
            while(_time>0)
            {
                await Task.Delay(1000);
                _time--;
                timeText.Text=_time.ToString();
            }
            if(_debug)
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
