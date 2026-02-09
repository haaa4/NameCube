using Serilog;
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

            try
            {
                Log.Debug("Appearance页面初始化");

                CanChange = false;

                if (GlobalVariablesData.config.AllSettings.color == null)
                {
                    Log.Debug("颜色设置为空，初始化为默认颜色 #30d7d7");
                    GlobalVariablesData.config.AllSettings.color = (Brush)new BrushConverter().ConvertFromInvariantString("#30d7d7");
                }

                //DarkLight.IsChecked = GlobalVariablesData.config.AllSettings.Dark;
                PreviewText.Foreground = GlobalVariablesData.config.AllSettings.color;
                //PreviewText.FontFamily = GlobalVariablesData.config.AllSettings.Font;
                ColorTextBox.Foreground = GlobalVariablesData.config.AllSettings.color;
                ColorTextBox.Text = GlobalVariablesData.config.AllSettings.color.ToString();
                //FontComboBox.SelectedItem = GlobalVariablesData.config.AllSettings.Font;

                CanChange = true;

                //Log.Debug("外观设置加载完成，当前主题: {Theme}, 颜色: {Color}, 字体: {Font}",
                //    GlobalVariablesData.config.AllSettings.Dark ? "深色" : "浅色",
                //    GlobalVariablesData.config.AllSettings.color.ToString(),
                //    GlobalVariablesData.config.AllSettings.Font?.Source);
                Log.Debug("外观设置加载完成，当前主题: {Theme}, 颜色: {Color}",
                   GlobalVariablesData.config.AllSettings.Dark ? "深色" : "浅色",
                   GlobalVariablesData.config.AllSettings.color.ToString()
                   );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Appearance页面初始化时发生异常");
                CanChange = true; // 确保后续可以修改
            }
        }

        //private void DarkLight_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (CanChange)
        //        {
        //            bool newTheme = DarkLight.IsChecked.Value;
        //            Log.Information("切换主题模式: {Theme}", newTheme ? "深色" : "浅色");

        //            GlobalVariablesData.config.AllSettings.Dark = newTheme;
        //            GlobalVariablesData.SaveConfig();

        //            if (DarkLight.IsChecked.Value)
        //            {
        //                Log.Warning("用户尝试启用深色模式");
        //                MessageBoxFunction.ShowMessageBoxWarning("当前黑暗模式存在较大问题，请勿使用");
        //                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
        //                    Wpf.Ui.Appearance.ApplicationTheme.Dark,
        //                    Wpf.Ui.Controls.WindowBackdropType.Auto,
        //                    true
        //                );
        //            }
        //            else
        //            {
        //                Log.Debug("应用浅色主题");
        //                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
        //                    Wpf.Ui.Appearance.ApplicationTheme.Light,
        //                    Wpf.Ui.Controls.WindowBackdropType.Auto,
        //                    true
        //                );
        //            }

        //            var settingsWindow = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();

        //            if (settingsWindow == null)
        //            {
        //                Log.Debug("未找到现有的设置窗口，创建新实例");
        //                settingsWindow = new SettingsWindow();
        //            }

        //            // 重新创建设置窗口以应用主题更改
        //            Log.Debug("重新创建设置窗口以应用主题更改");
        //            settingsWindow.Close();
        //            settingsWindow = new SettingsWindow();
        //            settingsWindow.Show();
        //            settingsWindow.Activate();
        //            settingsWindow.WindowState = WindowState.Normal;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "切换主题时发生异常");
        //    }
        //}

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    Log.Debug("通过回车键提交颜色值: {Color}", ColorTextBox.Text);

                    try
                    {
                        Brush newColor = (Brush)new BrushConverter().ConvertFromInvariantString(ColorTextBox.Text);
                        GlobalVariablesData.config.AllSettings.color = newColor;
                        PreviewText.Foreground = newColor;
                        ColorTextBox.Foreground = newColor;
                        GlobalVariablesData.SaveConfig();

                        Log.Information("颜色设置更新: {Color}", ColorTextBox.Text);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "颜色格式无效: {Color}", ColorTextBox.Text);
                        SnackBarFunction.ShowSnackBarInSettingWindow(ex.Message, Wpf.Ui.Controls.ControlAppearance.Caution);
                        ColorTextBox.Text = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理颜色输入时发生异常");
            }
        }

        private void ColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (CanChange)
                {
                    // 仅记录调试信息，不频繁保存
                    Log.Verbose("颜色文本框内容变化: {Color}", ColorTextBox.Text);

                    try
                    {
                        Brush newColor = (Brush)new BrushConverter().ConvertFromString(ColorTextBox.Text);
                        GlobalVariablesData.config.AllSettings.color = newColor;
                        PreviewText.Foreground = newColor;
                        ColorTextBox.Foreground = newColor;
                    }
                    catch 
                    {
                        Log.Verbose("颜色解析失败，可能正在输入中: {Color}", ColorTextBox.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理颜色文本框变化时发生异常");
            }
        }

        //private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    try
        //    {
        //        if (CanChange)
        //        {
        //            if (FontComboBox.SelectedItem is FontFamily font)
        //            {
        //                Log.Information("字体选择更改: {Font}", font.Source);

        //                PreviewText.FontFamily = font;
        //                GlobalVariablesData.config.AllSettings.Font = font;
        //                GlobalVariablesData.SaveConfig();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "处理字体选择更改时发生异常");
        //    }
        //}
    }
}