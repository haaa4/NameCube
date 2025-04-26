using System;
using Wpf.Ui;

namespace NameCube.Setting
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            if(GlobalVariables.json.AllSettings.NameCubeMode==1)
            {
                BallSetting.Visibility=System.Windows.Visibility.Collapsed;
            }
        }

        private void NavigationMenu_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationMenu.Navigate(typeof(Setting.Appearance));
        }

    }
}
