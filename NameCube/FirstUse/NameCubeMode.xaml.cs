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

using NameCube.Function;
using Serilog;  // 添加Serilog命名空间
using System;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.FirstUse
{
    /// <summary>
    /// NameCubeMode.xaml 的交互逻辑
    /// </summary>
    public partial class NameCubeMode : Page
    {
        public NameCubeMode()
        {
            InitializeComponent();
            Log.Information("NameCubeMode页面初始化");
        }

        private void Media1_MediaEnded(object sender, RoutedEventArgs e)
        {
            Log.Debug("演示视频1播放结束，重新开始播放");
            Media1.Position = TimeSpan.Zero;
            Media1.Play();
        }

        private void Media2_MediaEnded(object sender, RoutedEventArgs e)
        {
            Log.Debug("演示视频2播放结束，重新开始播放");
            Media2.Position = TimeSpan.Zero;
            Media2.Play();
        }

        private void Media2_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Log.Error(e.ErrorException, "播放演示视频2时发生错误");
            MessageBoxFunction.ShowMessageBoxError(e.ErrorException.ToString());
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RadioButton1.IsChecked.Value)
                {
                    GlobalVariablesData.config.AllSettings.NameCubeMode = 0;
                    Log.Information("用户选择了悬浮球模式");
                }
                else
                {
                    GlobalVariablesData.config.AllSettings.NameCubeMode = 1;
                    Log.Information("用户选择了传统窗口模式");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "设置NameCube模式时发生错误");
            }
        }
    }
}