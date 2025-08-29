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

namespace NameCube.ToolBox.AutomaticProcessPages
{
    /// <summary>
    /// Debug.xaml 的交互逻辑
    /// </summary>
    public partial class Debug : Page
    {
        public Debug()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.automaticProcess.debug = false;
            GlobalVariables.SaveJson();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DebugWindows debugWindows=null;
            Page targetPage = null; 

            switch (SelectedData.SelectedIndex)
            {
                case 0:
                    targetPage = new ProcessPages.ReadyPage(Data1.Text, (int)NumericData.Value, Data2.Text, CheckedData.IsChecked.Value, true);
                    break;
                case 1:
                    targetPage = new ProcessPages.StartFiles(Data1.Text, true);
                    break;
                case 2:
                    targetPage = new ProcessPages.AudioPage(Data1.Text,(int)NumericData.Value, true);
                    break;
                case 3:
                    targetPage=new ProcessPages.ReadPage(Data1.Text,(int)NumericData.Value,CheckedData.IsChecked.Value,true);
                    break;
                case 4:
                    targetPage = new ProcessPages.CmdPage(Data1.Text, CheckedData.IsChecked.Value, true);
                    break;
                case 5:
                    targetPage = new ProcessPages.WaitPage((int)NumericData.Value, true);
                    break;
                case 6:
                    targetPage = new ProcessPages.ClearPage(true);
                    break;
                case 7:
                    targetPage = new ProcessPages.PowerOffPage((int)NumericData.Value, true); 
                    break;
                default:
                    targetPage = null;
                    break;
            }
            if (targetPage != null)
            {
                debugWindows = new DebugWindows(targetPage);
                debugWindows.Show();
            }
        }
    }
}
