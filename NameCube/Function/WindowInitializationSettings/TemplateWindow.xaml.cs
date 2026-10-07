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
using System.Windows.Shapes;

namespace NameCube.Function.WindowInitializationSettings
{
    /// <summary>
    /// TemplateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TemplateWindow : Wpf.Ui.Controls.FluentWindow
    {
        public TemplateWindow()
        {
            InitializeComponent();
            if(GlobalVariablesData.config.MainWindowSettings.Height!=-1)
            {
                Height = GlobalVariablesData.config.MainWindowSettings.Height;
                Width = GlobalVariablesData.config.MainWindowSettings.Width;
                Top=GlobalVariablesData.config.MainWindowSettings.Top;
                Left = GlobalVariablesData.config.MainWindowSettings.Left;
            }
        }
        private string LocationToString()
        {
            return $"Height:{Height} Width:{Width}\nTop:{Top} Left:{Left}";
        }

        private void FluentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DescriptionTextBlock.Text = LocationToString();
        }

        private void FluentWindow_LocationChanged(object sender, EventArgs e)
        {
            DescriptionTextBlock.Text = LocationToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariablesData.config.MainWindowSettings.Height = Height;
            GlobalVariablesData.config.MainWindowSettings.Width = Width;
            GlobalVariablesData.config.MainWindowSettings.Top = Top;
            GlobalVariablesData.config.MainWindowSettings.Left = Left;
            GlobalVariablesData.SaveConfig();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
    
}
