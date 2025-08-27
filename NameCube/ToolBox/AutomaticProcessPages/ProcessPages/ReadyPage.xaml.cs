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
    /// ReadyPage.xaml 的交互逻辑
    /// </summary>
    public partial class ReadyPage : Page
    {
        /// <summary>
        /// ReadyPage
        /// </summary>
        /// <param name="processName">自动流程名称</param>
        /// <param name="time">准备时间</param>
        /// <param name="info">提醒信息(默认为"即将执行")</param>
        /// <param name="canCancle">是否可取消</param>
        /// <param name="isDebug">是否为Debug模式</param>
        public ReadyPage(
            string processName,
            int time,
            string info = null,
            bool canCancle = true,
            bool isDebug = false
        )
        {
            InitializeComponent();
            this.DataContext = this;
            ProcessName = processName;
            debug = isDebug;
            Time = time;
            if (info != null && info != "")
            {
                Info = info;
            }
            if (!canCancle)
            {
                CancleButton.IsEnabled = false;
                CancleButton.Content = "不可取消";
            }
            Loaded += ReadyPage_Loaded;
        }

        private async void ReadyPage_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(Time * 1000);
            if (debug)
            {
                CallParentMethodDebug("时间已到");
            }
            else
            {
                CallEndThePage();
            }
        }

        public string ProcessName { get; set; } = "自动流程";
        public int Time { get; set; } = 5;
        public string Info { get; set; } = "即将执行";
        public bool debug = false;
        public event Action<string> RequestParentAction;
        public event Action<int> EndThePageAction;
        private void CallEndThePage(int ret=0)
        {
            EndThePageAction?.Invoke(ret);
        }

        private void CallParentMethodDebug(string data)
        {
            RequestParentAction?.Invoke(data);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (debug)
            {
                CallParentMethodDebug("执行");
            }
            else
            {
                CallEndThePage();
            }
        }

        private void CancleButton_Click(object sender, RoutedEventArgs e)
        {
            if (debug)
            {
                CallParentMethodDebug("取消");
            }
            else
            {
                CallEndThePage(1);
            }
        }
    }
}
