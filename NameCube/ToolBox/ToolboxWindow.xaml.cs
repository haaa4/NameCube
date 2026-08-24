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
using NameCube.Setting;
using Serilog;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Wpf.Ui.Controls;

namespace NameCube.ToolBox
{
    /// <summary>
    /// ToolboxWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ToolboxWindow
    {
        private static readonly ILogger _logger = Log.ForContext<ToolboxWindow>();
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        public ToolboxWindow()
        {
            InitializeComponent();
            _logger.Debug("工具箱窗口初始化开始");
            _logger.Information("工具箱窗口创建完成");

        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            border.Background = this.Background;
            _logger.Debug("工具箱窗口加载完成，开始显示动画");

            NavigationMenu.Navigate(typeof(ToolBox.Welcome));
            _logger.Debug("导航到语音工具箱页面");

            var showStoryBoard = FindResource("ShowStoryBoard") as Storyboard;
            showStoryBoard.Stop();
            showStoryBoard.Remove();
            border.Visibility = System.Windows.Visibility.Visible;

            showStoryBoard.Completed += (s, en) =>
            {
                border.Visibility = System.Windows.Visibility.Collapsed;
                _logger.Debug("工具箱窗口显示动画完成");
            };

            showStoryBoard.Begin();
            Item1.IsEnabled = true;
            Item2.IsEnabled = true;
            Item3.IsEnabled = true;
            if (!CredentialHelper.PermissionVerification())
            {
                Item1.IsEnabled = !GlobalVariablesData.config.PermissionManager.needAdmin[12];
                Item2.IsEnabled = !GlobalVariablesData.config.PermissionManager.needAdmin[13];
                Item3.IsEnabled = !GlobalVariablesData.config.PermissionManager.needAdmin[14];
                this.Title = "小工具（权限受限）";
                TitleBar.Title= "小工具（权限受限）";
            }
            else
            {
                Item1.IsEnabled = true;
                Item2.IsEnabled = true;
                Item3.IsEnabled = true;
                this.Title = "小工具";
                TitleBar.Title = "小工具";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("用户点击工具箱窗口的重启按钮");
            AppFunction.Restart();
        }

        private async void NavigationViewItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Wpf.Ui.Controls.ContentDialog();
            dialog = new ContentDialog()
            {
                CloseButtonText = "取消",
                PrimaryButtonText = "继续",
                Title = "即将打开网页，继续吗？",
            };
            var ToolBoxWindow = Application.Current.Windows.OfType<ToolboxWindow>().FirstOrDefault();
            dialog.DialogHostEx = ToolBoxWindow.RootContentDialogPresenter;
            Wpf.Ui.Controls.ContentDialogResult contentDialogResult = await dialog.ShowAsync();
            if (contentDialogResult == Wpf.Ui.Controls.ContentDialogResult.None)
            {
                Log.Debug("用户取消打开网页操作");
                dialog.Hide();
            }
            else
            {
                ProcessStartInfo processStartInfo = new();
                processStartInfo.FileName = "https://github.com/haaa4/DeskSweeper";
                processStartInfo.UseShellExecute= true;
                Process.Start(processStartInfo);
            }
        }
    }
}