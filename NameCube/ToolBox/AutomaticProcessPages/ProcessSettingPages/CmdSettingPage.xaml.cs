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
    /// CmdSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class CmdSettingPage : Page
    {
        public string cmd;
        public bool? visibility;
        bool canChange = false;
        public CmdSettingPage(ProcessData processData)
        {
            InitializeComponent();
            cmd = processData.stringData1;
            visibility = processData.boolData;
            CmdTextBox.Text = cmd; 
            VisibilityCheckBox.IsChecked = visibility;
            canChange= true;
        }

        private void CmdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(canChange)
            {
                cmd = CmdTextBox.Text;
            }

        }

        private void VisibilityCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(canChange)
            {
                visibility = VisibilityCheckBox.IsChecked.Value;
            }

        }
    }
}
