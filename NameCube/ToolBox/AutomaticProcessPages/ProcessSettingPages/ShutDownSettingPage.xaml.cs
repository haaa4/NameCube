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
    /// ShutDownSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class ShutDownSettingPage : Page
    {
        bool canChange=false;
        public int shutDownWay;
        public ShutDownSettingPage(ProcessData processData)
        {
            InitializeComponent();
            if(processData.doubleData!=double.NaN&&processData.doubleData>=0&&processData.doubleData<=3)
            {
                shutDownWay = (int)processData.doubleData;
            }
            else
            {
                shutDownWay = 0;
            }
                ShutDownWayComboBox.SelectedIndex = shutDownWay;
            canChange = true;
        }

        private void ShutDownWayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(canChange)
            {
                shutDownWay=ShutDownWayComboBox.SelectedIndex;
            }
        }
    }
}
