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
            bool isDebug = false,
            bool show = true
        )
        {
            Log.Information("初始化就绪页面 - 流程名称: {ProcessName}, 准备时间: {Time}秒, 信息: {Info}, 可取消: {CanCancel}, 调试模式: {Debug}, 显示: {Show}",
                processName, time, info ?? "无", canCancle, isDebug, show);
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
                Log.Debug("取消按钮已禁用");
            }
            Loaded += ReadyPage_Loaded;
            if (!show)
            {
                Log.Debug("页面设置为不显示模式，自动触发加载");
                ReadyPage_Loaded(null, null);
            }
        }

        private async void ReadyPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Information("开始就绪等待，时间: {Time}秒", Time);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击执行按钮");
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
            Log.Information("用户点击取消按钮");
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