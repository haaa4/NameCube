// /*
//  * NameCube-<点名器>
//  * Copyright (C) 2025-2026 haaa4
//  *
//  * This program is free software: you can redistribute it and/or modify
//  * it under the terms of the GNU General Public License as published by
//  * the Free Software Foundation, either version 3 of the License, or
//  * (at your option) any later version.
//  *
//  * This program is distributed in the hope that it will be useful,
//  * but WITHOUT ANY WARRANTY

using Masuit.Tools;
using NameCube.Function;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Brush = System.Windows.Media.Brush;
using Color = System.Drawing.Color;

namespace NameCube.Setting
{
    /// <summary>
    /// Appearance.xaml 的交互逻辑
    /// </summary>
    public partial class Appearance : Page
    {
        private bool CanChange;

        public Appearance()
        {
            InitializeComponent();
        }

        private void DarkLight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CanChange)
                {
                    bool newTheme = DarkLight.IsChecked.Value;
                    Log.Information("切换主题模式: {Theme}", newTheme ? "深色" : "浅色");

                    GlobalVariablesData.config.AllSettings.Dark = newTheme;
                    GlobalVariablesData.SaveConfig();

                    SnackBarFunction.ShowSnackBarInSettingWindow("设置已保存，重启后应用主题", Wpf.Ui.Controls.ControlAppearance.Info);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "切换主题时发生异常");
            }
        }

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
                        ApplicationAccentColorManager.Apply(((SolidColorBrush)newColor).Color,ApplicationTheme.Light,true);
                        PreviewText.Foreground = newColor;
                        ColorTextBox.Foreground = newColor;
                        GlobalVariablesData.SaveConfig();
                        SnackBarFunction.ShowSnackBarInSettingWindow("设置已保存，可能需要重启以应用", Wpf.Ui.Controls.ControlAppearance.Info);
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
        //                Application.Current.Resources["AppFont"] = font;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Title = "选择图片";
            openFileDialog.Filter = "图片 (*.png*.jpg*.jpeg)|*.png;*.jpg;*.jpeg";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            MenuItem_Click(null, null);
            Log.Information("切换背景图片：{Image}",openFileDialog.FileName);
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Log.Information("选择悬浮球自定义图片: {FilePath}", openFileDialog.FileName);
                Ring.Visibility = Visibility.Visible;
                CopyImage(openFileDialog.FileName);
                GlobalVariablesData.config.AllSettings.HaveBackgroundImage = true;
                GlobalVariablesData.SaveConfig();
            }
        }
        private async void CopyImage(string Filename)
        {
            Log.Debug("开始复制图片: {Filename}", Filename);

            await Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(GlobalVariablesData.userDataDir, "Image"));
                    if(File.Exists(Path.Combine(GlobalVariablesData.userDataDir, "Image", "Background.png")))
                    File.Delete(Path.Combine(GlobalVariablesData.userDataDir, "Image", "Background.png"));//虽然格式有多种，但Image能自己识别格式，所以一律保存为png格式，保持代码简洁（其实也没简洁到哪里）
                    File.Copy(Filename, Path.Combine(GlobalVariablesData.userDataDir, "Image", "Background.png"));
                    Log.Information("图片复制完成");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "复制图片时发生异常");
                    MessageBoxFunction.ShowMessageBoxError("复制图片时发生异常" + ex.Message);
                }
            });
            BitmapImage bitmap = new BitmapImage();
            var mainWindow = System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
                mainWindow.ReFreshBackgroundImage();
            Ring.Visibility = Visibility.Collapsed;
            SnackBarFunction.ShowSnackBarInSettingWindow("切换成功，若无反应，尝试重启应用", ControlAppearance.Info);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.HaveBackgroundImage = false;
                GlobalVariablesData.SaveConfig();
                Log.Information("恢复默认背景");
                ReFreshBackgroundImage();
                if (e!=null)
                SnackBarFunction.ShowSnackBarInSettingWindow("切换成功", ControlAppearance.Info);
            }
        }

        private void StretchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(CanChange)
            {
                Log.Information("切换背景图片拉伸方式：{Index}", StretchComboBox.SelectedIndex.ToString());
                GlobalVariablesData.config.AllSettings.BackgroundImageStretch = (Stretch)StretchComboBox.SelectedIndex;
                GlobalVariablesData.SaveConfig();
                ReFreshBackgroundImage();
            }
        }
        private void ReFreshBackgroundImage()
        {
            var mainWindow = System.Windows.Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
                mainWindow.ReFreshBackgroundImage();
        }

        private void BackgroundOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(CanChange)
            {
                Log.Debug("切换背景图片不透明度：{Value}", BackgroundOpacitySlider.Value);
                GlobalVariablesData.config.AllSettings.BackgroundOpacity = BackgroundOpacitySlider.Value.ToInt32();
                GlobalVariablesData.SaveConfig();
                ReFreshBackgroundImage();
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            ColorTextBox.Text = "#FF005493";
            Log.Debug("恢复颜色值: {Color}", ColorTextBox.Text);

            try
            {
                Brush newColor = (Brush)new BrushConverter().ConvertFromInvariantString(ColorTextBox.Text);
                GlobalVariablesData.config.AllSettings.color = newColor;
                ApplicationAccentColorManager.Apply(((SolidColorBrush)newColor).Color, ApplicationTheme.Light, true);
                PreviewText.Foreground = newColor;
                ColorTextBox.Foreground = newColor;
                GlobalVariablesData.SaveConfig();
                SnackBarFunction.ShowSnackBarInSettingWindow("设置已保存，可能需要重启以应用", Wpf.Ui.Controls.ControlAppearance.Info);
                Log.Information("颜色设置更新: {Color}", ColorTextBox.Text);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "颜色格式无效: {Color}", ColorTextBox.Text);
                SnackBarFunction.ShowSnackBarInSettingWindow(ex.Message, Wpf.Ui.Controls.ControlAppearance.Caution);
                ColorTextBox.Text = null;
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if(GlobalVariablesData.config.AllSettings.HaveBackgroundImage)
            {
                Bitmap bitmap = new Bitmap(Path.Combine(GlobalVariablesData.userDataDir, "Image", "Background.png"));
                Color color =GetDominantColor(bitmap);
                ColorTextBox.Text = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
                try
                {
                    Brush newColor = (Brush)new BrushConverter().ConvertFromInvariantString(ColorTextBox.Text);
                    GlobalVariablesData.config.AllSettings.color = newColor;
                    ApplicationAccentColorManager.Apply(((SolidColorBrush)newColor).Color, ApplicationTheme.Light, true);
                    PreviewText.Foreground = newColor;
                    ColorTextBox.Foreground = newColor;
                    GlobalVariablesData.SaveConfig();
                    SnackBarFunction.ShowSnackBarInSettingWindow("设置已保存，可能需要重启以应用", Wpf.Ui.Controls.ControlAppearance.Info);
                    Log.Information("颜色设置更新: {Color}", ColorTextBox.Text);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "颜色格式无效: {Color}", ColorTextBox.Text);
                    SnackBarFunction.ShowSnackBarInSettingWindow(ex.Message, Wpf.Ui.Controls.ControlAppearance.Caution);
                    ColorTextBox.Text = null;
                }
            }
            else
            {
                Log.Warning("没有可用的图片");
                MessageBoxFunction.ShowMessageBoxWarning("没有可用的图片");
            }
        }
        public static Color GetDominantColor(Bitmap bitmap)
        {
            // 1. 统计颜色频率
            var colorCounts = new Dictionary<int, int>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    // 2. 颜色量化：将颜色离散化，这里简单地将RGB各通道除以一个因子
                    int quantizedColor = QuantizeColor(color, 64);
                    if (colorCounts.ContainsKey(quantizedColor))
                        colorCounts[quantizedColor]++;
                    else
                        colorCounts[quantizedColor] = 1;
                }
            }

            // 3. 找出出现次数最多的颜色
            int maxCount = 0;
            int dominantColorValue = 0;
            foreach (var pair in colorCounts)
            {
                if (pair.Value > maxCount)
                {
                    maxCount = pair.Value;
                    dominantColorValue = pair.Key;
                }
            }

            // 将量化后的颜色值转换回Color对象
            return Color.FromArgb(dominantColorValue);
        }

        private static int QuantizeColor(Color color, int divisor)
        {
            int r = color.R / divisor;
            int g = color.G / divisor;
            int b = color.B / divisor;
            // 将RGB组合成一个整数
            return (r << 16) | (g << 8) | b;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

            try
            {
                Log.Debug("Appearance页面初始化");
                CanChange = false;
                DarkLight.IsChecked = GlobalVariablesData.config.AllSettings.Dark;
                PreviewText.Foreground = GlobalVariablesData.config.AllSettings.color;
                ColorTextBox.Foreground = GlobalVariablesData.config.AllSettings.color;
                ColorTextBox.Text = GlobalVariablesData.config.AllSettings.color.ToString();
                StretchComboBox.SelectedIndex = ((int)(GlobalVariablesData.config.AllSettings.BackgroundImageStretch ?? Stretch.Fill));
                BackgroundOpacitySlider.Value = GlobalVariablesData.config.AllSettings.BackgroundOpacity ?? 100;
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
    }
}