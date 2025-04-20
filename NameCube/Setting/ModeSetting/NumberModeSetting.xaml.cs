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
    /// NumberModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class NumberModeSetting : Page
    {
        public NumberModeSetting()
        {
            InitializeComponent();
            CanChange = false;
            LockedCheck.IsChecked = GlobalVariables.json.NumberModeSettings.Locked;
            Speed.Value = GlobalVariables.json.NumberModeSettings.Speed - 10;
            CanChange = true;
        }
        bool CanChange;

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.NumberModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.NumberModeSettings.Speed = (int)Speed.Value + 10;
                GlobalVariables.SaveJson();
            }
        }
    }
}
