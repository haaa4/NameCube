using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessSettingPages
{
    /// <summary>
    /// StartSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class StartSettingPage : Page
    {
        public string path;
        public StartSettingPage(ProcessData processData)
        {
            InitializeComponent();
            DataContext = this;
            path= processData.stringData1;
            PathTextBox.Text = path;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            { 
                Filter=" 所有文件|*.*",
                Title= "选择启动文件",
                Multiselect= false
            };
            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog.FileName;
                PathTextBox.Text = path;
            }
        }

        private void PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            path = PathTextBox.Text;
        }

        private void PathTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
