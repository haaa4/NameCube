using System.Windows;
using System.Windows.Controls;

namespace NameCube.Setting
{
    /// <summary>
    /// MemoryFactorModeSettings.xaml 的交互逻辑
    /// </summary>
    public partial class MemoryFactorModeSettings : Page
    {
        bool CanChange;
        public MemoryFactorModeSettings()
        {
            InitializeComponent();
            CanChange = false;
            LockedCheck.IsChecked = GlobalVariables.json.MemoryFactorModeSettings.Locked;
            Speed.Value = GlobalVariables.json.MemoryFactorModeSettings.Speed - 10;
            CanChange = true;
        }

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.MemoryFactorModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.MemoryFactorModeSettings.Speed = (int)Speed.Value + 10;
                GlobalVariables.SaveJson();
            }
        }
    }
}
