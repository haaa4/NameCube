using Masuit.Tools;
using NameCube.GlobalVariables.DataClass;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessSettingPages
{
    /// <summary>
    /// WaitTimeSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class WaitTimeSettingPage : Page
    {
        public int waitTime;
        private bool canChange = false;

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