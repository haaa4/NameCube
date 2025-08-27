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
    /// WaitTimeSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class WaitTimeSettingPage : Page
    {
        public int waitTime;
        bool canChange = false;
        public WaitTimeSettingPage(ProcessData processData)
        {
            InitializeComponent();
            waitTime = (int)processData.doubleData;
            WaitTimeNumberBox.Value = waitTime;
            canChange = true;
        }

        private void WaitTimeNumberBox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if(canChange)
            {
                waitTime = (int)WaitTimeNumberBox.Value;
            }
        }

        private void WaitTimeNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int get = WaitTimeNumberBox.Text.ToInt32(-1);
            if (get != -1)
            {
                WaitTimeNumberBox.Value = get;
                waitTime = get;
            }
            else
            {
                WaitTimeNumberBox.Value = waitTime;
            }
        }
    }
}
