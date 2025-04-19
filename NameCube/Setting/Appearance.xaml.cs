using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.Setting
{
    /// <summary>
    /// Appearance.xaml 的交互逻辑
    /// </summary>
    public partial class Appearance : Page
    {
        bool CanChange;
        public Appearance()
        {
            InitializeComponent();
            CanChange = false;
            DarkLight.IsChecked = GlobalVariables.json.AllSettings.Dark;
            CanChange = true;
        }
        private void DarkLight_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.AllSettings.Dark = DarkLight.IsChecked.Value;
                GlobalVariables.SaveJson();
                if (DarkLight.IsChecked.Value)
                {
                    Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                        Wpf.Ui.Appearance.ApplicationTheme.Dark, // Theme type
                         Wpf.Ui.Controls.WindowBackdropType.Auto,  // Background type
                         true                                      // Whether to change accents automatically
                       );
                }
                else
                {
                    Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                        Wpf.Ui.Appearance.ApplicationTheme.Light, // Theme type
                         Wpf.Ui.Controls.WindowBackdropType.Auto,  // Background type
                         true                                      // Whether to change accents automatically
                       );
                }

                var settingsWindow = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();

                if (settingsWindow == null)
                {
                    // 创建新实例
                    settingsWindow = new SettingsWindow();
                }

                // 确保窗口可见并激活
                settingsWindow.Close();
                settingsWindow = new SettingsWindow();
                settingsWindow.Show();
                settingsWindow.Activate();
                settingsWindow.WindowState = WindowState.Normal;

            }
        }
    }
}
