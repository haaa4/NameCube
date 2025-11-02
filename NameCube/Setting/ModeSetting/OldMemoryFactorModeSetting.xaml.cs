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
    /// OldMemoryFactorModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class OldMemoryFactorModeSetting : Page
    {
        bool CanChange;
        public OldMemoryFactorModeSetting()
        {
            InitializeComponent();
            CanChange = false;
            LockedCheck.IsChecked = GlobalVariables.json.OldMemoryFactorModeSettings.Locked;
            Speed.Value = (double)(GlobalVariables.json.OldMemoryFactorModeSettings.Speed - 10);
            EnableCheck.IsChecked = GlobalVariables.json.OldMemoryFactorModeSettings.IsEnable;
            CanChange = true;
        }

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.OldMemoryFactorModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.OldMemoryFactorModeSettings.Speed = (int)Speed.Value + 10;
                GlobalVariables.SaveJson();
            }
        }

        private void EnableCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.OldMemoryFactorModeSettings.IsEnable = EnableCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }
    }
}
