using Masuit.Tools.Logging;
using Masuit.Tools.Win32;
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

namespace NameCube.ToolBox
{
    /// <summary>
    /// MemoryToolbox.xaml 的交互逻辑
    /// </summary>
    public partial class MemoryToolbox : Page
    {
        public MemoryToolbox()
        {
            InitializeComponent();
            MemoryCheck.IsChecked = GlobalVariables.json.StartToDo.AlwaysCleanMemory;
        }
        private void MemoryCheck_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.StartToDo.AlwaysCleanMemory = MemoryCheck.IsChecked.Value;
            GlobalVariables.SaveJson();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LogManager.Info("开始内存清理......");
            Windows.ClearMemorySilent();
        }
    }
}
