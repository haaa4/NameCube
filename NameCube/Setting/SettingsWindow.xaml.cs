using System;
using System.Windows.Media.Animation;
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
            if(GlobalVariables.json.AllSettings.Top)
            {
                Topmost = true;
            }
        }

        private void NavigationMenu_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationMenu.Navigate(typeof(Setting.Appearance));
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AppFunction.Restart();
        }

        private void FluentWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var showStoryBoard = FindResource("ShowStoryBoard") as Storyboard;
            showStoryBoard.Stop();
            showStoryBoard.Remove();
            border.Visibility = System.Windows.Visibility.Visible;
            showStoryBoard.Completed += (s, en) =>
            {
                border.Visibility = System.Windows.Visibility.Collapsed;
            };
            showStoryBoard.Begin();
        }
    }
}
