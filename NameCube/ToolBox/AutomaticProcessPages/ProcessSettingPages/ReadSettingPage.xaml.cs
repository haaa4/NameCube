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
using System.Windows.Shapes;
using NameCube.GlobalVariables.DataClass;
using Serilog;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessSettingPages
{
    /// <summary>
    /// ReadSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class ReadSettingPage : Page
    {
        public string text;
        public int time;
        public bool? read;
        bool canChange = false;

        public ReadSettingPage(ProcessData processData)
        {
            Log.Information("初始化阅读设置页面");
            try
            {
                InitializeComponent();
                text = processData.stringData1;
                time = (int)processData.doubleData;
                read = processData.boolData;
                TextTextBox.Text = text;
                TimeNumberBox.Value = time;

                if (read.HasValue)
                {
                    ReadCheckBox.IsChecked = read;
                }

                Log.Debug("阅读设置初始化 - 文本长度: {TextLength}, 时间: {Time}秒, 朗读: {Read}",
                    text?.Length ?? 0, time, read);

                canChange = true;
                Log.Information("阅读设置页面初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化阅读设置页面时发生错误");
                throw;
            }
        }

        private void TextTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (canChange)
            {
                string oldText = text;
                text = TextTextBox.Text;

                if (oldText != text)
                {
                    Log.Debug("阅读文本从长度 {OldLength} 修改为长度 {NewLength}",
                        oldText?.Length ?? 0, text?.Length ?? 0);
                }
            }
        }

        private void TimeNumberBox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (canChange)
            {
                int oldTime = time;
                time = (int)TimeNumberBox.Value;

                if (oldTime != time)
                {
                    Log.Debug("阅读时间从 {OldTime}秒 修改为 {NewTime}秒", oldTime, time);
                }
            }
        }

        private void TimeNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Log.Debug("时间输入框失去焦点");
            int get = TimeNumberBox.Text.ToInt32(-1);
            if (get != -1)
            {
                int oldTime = time;
                time = get;
                TimeNumberBox.Value = get;

                Log.Debug("阅读时间从 {OldTime}秒 修改为 {NewTime}秒", oldTime, time);
            }
            else
            {
                Log.Warning("时间输入无效，重置为5秒");
                TimeNumberBox.Text = "5";
                time = 5;
            }
        }

        private void ReadCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (canChange)
            {
                bool? oldRead = read;
                read = ReadCheckBox.IsChecked.Value;

                if (oldRead != read)
                {
                    Log.Information("朗读选项从 {OldRead} 修改为 {NewRead}", oldRead, read);
                }
            }
        }
    }
}