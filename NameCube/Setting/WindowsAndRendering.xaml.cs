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

using NameCube.Function.WindowInitializationSettings;
using Serilog;
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

namespace NameCube.Setting
{
    /// <summary>
    /// WindowsAndRendering.xaml 的交互逻辑
    /// </summary>
    public partial class WindowsAndRendering : Page
    {
        bool CanChange = false;
        public WindowsAndRendering()
        {
            InitializeComponent();
            DisabledAnimationCheck.IsChecked = GlobalVariablesData.config.AllSettings.DisableTheDisplayAnimationOfTheMainWindow;
            MaxSizeCheck.IsChecked = GlobalVariablesData.config.AllSettings.DefaultToMaximumSize;
            TopCheck.IsChecked = GlobalVariablesData.config.AllSettings.Top;
            if (GlobalVariablesData.config.AllSettings.NameCubeMode == 1)
            {
                TopActionCard.Visibility = Visibility.Collapsed;
            }
            CanChange = true;
        }
        private void TopCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.Top = TopCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                Log.Information("窗口置顶修改为: {Top}", TopCheck.IsChecked.Value);
            }
        }
        private void DisabledAnimationCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.DisableTheDisplayAnimationOfTheMainWindow = DisabledAnimationCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                Log.Information("主窗口显示动画修改为: {Disabled}", DisabledAnimationCheck.IsChecked.Value);
            }
        }

        private void MaxSizeCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.DefaultToMaximumSize = MaxSizeCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                Log.Information("默认最大化已改为{value}", MaxSizeCheck.IsChecked.Value);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var window = new TemplateWindow();
            window.ShowDialog();
        }
    }
}
