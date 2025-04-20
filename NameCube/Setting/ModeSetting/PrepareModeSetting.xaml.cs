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
    /// PrepareModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class PrepareModeSetting : Page
    {
        public PrepareModeSetting()
        {
            InitializeComponent();
            CanChange = false;
            LockedCheck.IsChecked = GlobalVariables.json.PrepareModeSetting.Locked;
            Speed.Value = GlobalVariables.json.PrepareModeSetting.Speed - 10;
            CanChange = true;
        }
        bool CanChange;

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.PrepareModeSetting.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.PrepareModeSetting.Speed = (int)Speed.Value + 10;
                GlobalVariables.SaveJson();
            }
        }
    }
}
