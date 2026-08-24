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
using System.Windows;

namespace NameCube.Setting.EasterEgg
{
    /// <summary>
    /// Media.xaml 的交互逻辑
    /// </summary>
    public partial class Media : Window
    {
        // 视频 URL（可根据需要修改）
        private string VideoUrl;

        public Media(string url)
        {
            InitializeComponent();
            VideoUrl = url;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        // 视频成功打开时触发
        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {

        }

        // 视频播放失败时触发
        private void VideoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show($"视频播放失败: {e.ErrorException?.Message}\n请检查网络或视频格式是否支持。",
                            "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            VideoPlayer.Source = null;
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartButton.Visibility = Visibility.Collapsed;
            VideoPlayer.Source = new Uri(VideoUrl);
        }
    }
}