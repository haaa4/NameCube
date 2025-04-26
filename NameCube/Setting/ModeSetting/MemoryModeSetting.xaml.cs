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
    /// MemoryModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class MemoryModeSetting : Page
    {
        public MemoryModeSetting()
        {
            InitializeComponent();
            CanChange = false;
            LockedCheck.IsChecked = GlobalVariables.json.MemoryModeSettings.Locked;
            Speed.Value = GlobalVariables.json.MemoryModeSettings.Speed - 10;
            AddSwitch.IsChecked = GlobalVariables.json.MemoryModeSettings.AutoAddFile;
            CanChange = true;
        }

        bool CanChange;

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.MemoryModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.MemoryModeSettings.Speed = (int)Speed.Value + 10;
                GlobalVariables.SaveJson();
            }
        }

        private void AddSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.MemoryModeSettings.AutoAddFile=AddSwitch.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }
    }
}
