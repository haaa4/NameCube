using System.Windows;
using System.Windows.Controls;

namespace NameCube.Setting.ModeSetting
{
    /// <summary>
    /// OnePeopleModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class OnePeopleModeSetting : Page
    {
        bool CanChange;
        public OnePeopleModeSetting()
        {
            InitializeComponent();
            CanChange = false;
            LockedCheck.IsChecked = GlobalVariables.json.OnePeopleModeSettings.Locked;
            Speed.Value = GlobalVariables.json.OnePeopleModeSettings.Speed - 10;
            CanChange = true;
        }

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.OnePeopleModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.OnePeopleModeSettings.Speed = (int)Speed.Value + 10;
                GlobalVariables.SaveJson();
            }
        }
    }
}
