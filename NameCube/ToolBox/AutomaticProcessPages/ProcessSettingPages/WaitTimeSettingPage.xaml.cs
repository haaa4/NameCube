using Masuit.Tools;
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
using NameCube.GlobalVariables.DataClass;
using System.Windows.Shapes;
using Serilog;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessSettingPages
{
    /// <summary>
    /// WaitTimeSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class WaitTimeSettingPage : Page
    {
        public int waitTime;
        bool canChange = false;

        public WaitTimeSettingPage(ProcessData processData)
        {
            Log.Information("初始化等待时间设置页面");
            try
            {
                InitializeComponent();
                waitTime = (int)processData.doubleData;
                WaitTimeNumberBox.Value = waitTime;

                Log.Debug("等待时间初始化: {WaitTime} 秒", waitTime);
                canChange = true;
                Log.Information("等待时间设置页面初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化等待时间设置页面时发生错误");
                throw;
            }
        }

        private void WaitTimeNumberBox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (canChange)
            {
                int oldWaitTime = waitTime;
                waitTime = (int)WaitTimeNumberBox.Value;

                if (oldWaitTime != waitTime)
                {
                    Log.Debug("等待时间从 {OldWaitTime}秒 修改为 {NewWaitTime}秒",
                        oldWaitTime, waitTime);
                }
            }
        }

        private void WaitTimeNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Log.Debug("等待时间输入框失去焦点");
            int get = WaitTimeNumberBox.Text.ToInt32(-1);
            if (get != -1)
            {
                int oldWaitTime = waitTime;
                waitTime = get;
                WaitTimeNumberBox.Value = get;

                Log.Debug("等待时间从 {OldWaitTime}秒 修改为 {NewWaitTime}秒",
                    oldWaitTime, waitTime);
            }
            else
            {
                Log.Warning("等待时间输入无效，保持原值: {WaitTime}秒", waitTime);
                WaitTimeNumberBox.Value = waitTime;
            }
        }
    }
}