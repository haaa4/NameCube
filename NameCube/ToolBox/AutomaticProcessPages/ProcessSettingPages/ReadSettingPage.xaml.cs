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
            InitializeComponent();
            text = processData.stringData1;
            time = (int)processData.doubleData;
            read = processData.boolData;
            TextTextBox.Text = text;
            TimeNumberBox.Value = time;
            if(read.HasValue)
            {
                ReadCheckBox.IsChecked = read;
            }
            canChange = true;
        }

        private void TextTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(canChange)
            {
                text = TextTextBox.Text;
            }
        }

        private void TimeNumberBox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if(canChange)
            {
                time= (int)TimeNumberBox.Value;
            }
        }

        private void TimeNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int get = TimeNumberBox.Text.ToInt32(-1);
            if(get!=-1)
            {
                TimeNumberBox.Value = get;
            }
            else
            {
                TimeNumberBox.Text = "5";
            }
        }

        private void ReadCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(canChange)
            {
                read = ReadCheckBox.IsChecked.Value;
            }
        }
    }
}
