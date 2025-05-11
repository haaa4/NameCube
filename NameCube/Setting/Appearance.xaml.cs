using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using FontFamily = System.Windows.Media.FontFamily;

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
            if (GlobalVariables.json.AllSettings.color == null)
            {
                GlobalVariables.json.AllSettings.color = (Brush)new BrushConverter().ConvertFromInvariantString("#30d7d7");
            }
            DarkLight.IsChecked = GlobalVariables.json.AllSettings.Dark;
            PreviewText.Foreground = GlobalVariables.json.AllSettings.color;
            PreviewText.FontFamily = GlobalVariables.json.AllSettings.Font;
            ColorTextBox.Foreground = GlobalVariables.json.AllSettings.color;
            ColorTextBox.Text = GlobalVariables.json.AllSettings.color.ToString();
            FontComboBox.SelectedItem = GlobalVariables.json.AllSettings.Font;
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

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {

                try
                {
                    GlobalVariables.json.AllSettings.color = (Brush)new BrushConverter().ConvertFromInvariantString(ColorTextBox.Text);
                    PreviewText.Foreground = GlobalVariables.json.AllSettings.color;
                    ColorTextBox.Foreground = GlobalVariables.json.AllSettings.color;
                }
                catch (Exception ex)
                {
                    MessageBoxFunction.ShowMessageBoxWarning(ex.Message);
                    ColorTextBox.Text = null;
                }



            }
        }

        private void ColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(CanChange)
            {
                try
                {
                    GlobalVariables.json.AllSettings.color = (Brush)new BrushConverter().ConvertFromString(ColorTextBox.Text);
                    PreviewText.Foreground = GlobalVariables.json.AllSettings.color;
                    ColorTextBox.Foreground = GlobalVariables.json.AllSettings.color;
                }
                catch
                {

                }
            }
        }

        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(CanChange)
            {
                if(FontComboBox.SelectedItem is FontFamily font)
                {
                    PreviewText.FontFamily = font;
                    GlobalVariables.json.AllSettings.Font= font;
                    GlobalVariables.SaveJson();
                }
            }
        }
    }
}
