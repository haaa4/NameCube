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

namespace NameCube.Setting.ModeSetting
{
    /// <summary>
    /// BatchModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class BatchModeSetting : Page
    {
        bool CanChange;
        public BatchModeSetting()
        {
            InitializeComponent();
            CanChange = false;
            LockedCheck.IsChecked = GlobalVariables.json.BatchModeSettings.Locked;
            CanChange = true;
        }

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange) 
            {
                GlobalVariables.json.BatchModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }
    }
}
