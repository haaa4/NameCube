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

using NameCube.Setting.EasterEgg;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Wpf.Ui.Controls;
using Image = System.Windows.Controls.Image;
using StackPanel = System.Windows.Controls.StackPanel;

namespace NameCube.Setting
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class About : Page
    {
        private int clickTimes = 0;

        public About()
        {
            InitializeComponent();
            Log.Debug("About页面初始化完成");
        }

        private async void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                clickTimes++;
                Log.Verbose("关于图标被点击，次数: {ClickTimes}", clickTimes);

                if (clickTimes >= 10)
                {
                    Log.Information("触发Debug模式入口");

                    var dialog = new Wpf.Ui.Controls.ContentDialog();
                    dialog = new ContentDialog()
                    {
                        CloseButtonText = "取消",
                        PrimaryButtonText = "确定",
                        Title = "进入Debug模式",
                        Content = new StackPanel()
                        {
                            Children =
                            {
                                new System.Windows.Controls.TextBlock()
                                {
                                    Text = "请输入对应的密码，开启对应的Debug模式"
                                },
                                new Wpf.Ui.Controls.TextBox()
                            }
                        }
                    };
                    var mainWindow = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();
                    dialog.DialogHostEx = mainWindow.RootContentDialogPresenter;
                    Wpf.Ui.Controls.ContentDialogResult contentDialogResult = await dialog.ShowAsync();

                    if (contentDialogResult == Wpf.Ui.Controls.ContentDialogResult.None)
                    {
                        Log.Debug("用户取消Debug模式输入");
                        dialog.Hide();
                    }
                    else
                    {
                        if (dialog.Content is StackPanel panel)
                        {
                            var password = panel.Children.OfType<Wpf.Ui.Controls.TextBox>().FirstOrDefault();
                            string enteredPassword = password?.Text ?? string.Empty;

                            Log.Debug("用户输入Debug密码，长度: {Length}", enteredPassword.Length);

                            switch (enteredPassword)
                            {
                                case "我知道自动流程Debug可能带来的危害，我仍然要开启它":
                                    Log.Information("开启自动流程Debug模式");
                                    GlobalVariablesData.config.AutomaticProcess.debug = true;
                                    GlobalVariablesData.SaveConfig();
                                    SnackBarFunction.ShowSnackBarInSettingWindow("自动流程Debug模式已开启", ControlAppearance.Success);
                                    break;

                                case "我知道开发者调试可能带来的危害，我仍然要开启它":
                                    Log.Information("开启开发者调试");
                                    GlobalVariablesData.config.AllSettings.debug = true;
                                    GlobalVariablesData.SaveConfig();
                                    SnackBarFunction.ShowSnackBarInSettingWindow("开发者调试已开启", ControlAppearance.Success);
                                    break;

                                case "我知道势能模式调试可能带来的危害，我仍然要开启它":
                                    Log.Information("开启势能模式调试模式");
                                    GlobalVariablesData.config.MemoryFactorModeSettings.debug = true;
                                    GlobalVariablesData.SaveConfig();
                                    SnackBarFunction.ShowSnackBarInSettingWindow("势能模式调试已开启", ControlAppearance.Success);
                                    break;
                                //以下是彩蛋部分
                                case "philia093":
                                    Log.Information("触发彩蛋：philia093");
                                    Media media = new Media("https://launcher-webstatic.mihoyo.com/launcher-public/2025/10/31/49fab36b3317cbe36b673e9183ed22c3_4733825790845625523.webm");
                                    media.ShowDialog();
                                    break;

                                case "Columbina":
                                    Log.Information("触发彩蛋：Columbina");
                                    Media media2 = new Media("https://launcher-webstatic.mihoyo.com/launcher-public/2026/01/08/f3c44cd72c6214ed680afe5fe90b26fc_6413191254498564796.webm");
                                    media2.ShowDialog();
                                    break;

                                default:
                                    Log.Warning("Debug密码错误: {Password}", enteredPassword);
                                    SnackBarFunction.ShowSnackBarInSettingWindow("密码错误", Wpf.Ui.Controls.ControlAppearance.Caution);
                                    break;
                            }
                        }
                        else
                        {
                            Log.Warning("Debug对话框内容解析失败");
                        }
                    }
                    clickTimes = 0; // 重置点击次数
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理Debug模式入口时发生异常");
            }
        }

        private async Task LoadImageFromWebAsync(string imageUrl, Image targetImage)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // 异步获取图片字节流
                    byte[] imageData = await client.GetByteArrayAsync(imageUrl);

                    // 在内存流中创建图片
                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        // 设置源为内存流
                        bitmapImage.StreamSource = ms;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.DecodePixelWidth = 400;
                        bitmapImage.EndInit();
                        // 图片解码后，通过Dispatcher切换到UI线程更新控件
                        this.Dispatcher.Invoke(() => {
                            targetImage.Source = bitmapImage;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning("加载头像失败：{Error}", ex);
            }
        }
        /*
         *  if (GlobalVariables.json.GiteeMode ?? false)
            {
                await LoadImageFromWebAsync("https://foruda.gitee.com/avatar/1777196359746152210/15207534_haaa4_1777196359.png!avatar100", HeadImage);
                await LoadImageFromWebAsync("https://raw.giteeusercontent.com/haaa4/NameCube/raw/main/NameCube/icon.png?metadata=eyJyIjoibWFpbiIsImZwIjoiTmFtZUN1YmUvaWNvbi5wbmciLCJ1aWQiOjE1MjA3NTM0LCJwaWQiOjQ1MzY1MzYxLCJzdG8iOiJnaXQtc2hhcmRpbmctc3RvLTQydC0wMTQiLCJycCI6InJlcG9zLzhmL2RiLzhmZGJiNGM1MDdhYzQ1ZWY5NmIyZmY1ODU4ZDM3NTVhOGM3MjVkMDQ5MzQyM2I5OWQwZTE5M2QwOTE1MzExZmQuZ2l0IiwiaXNwIjp0cnVlLCJleHBpcmVfYXQiOjE3ODY5NTY2MDB9&signature=vIuuYREbx9tXE51ysZ8Sdjr-d96rAS3VgABcw1nG-vI", NameCubeIcon);
                await LoadImageFromWebAsync("https://raw.giteeusercontent.com/haaa4/DeskSweeper/raw/main/DeskSweeper.png?metadata=eyJyIjoibWFpbiIsImZwIjoiRGVza1N3ZWVwZXIucG5nIiwidWlkIjoxNTIwNzUzNCwicGlkIjo0OTc0NTg3NSwic3RvIjoiZ2l0LXNoYXJkaW5nLXN0by00MnQtMDE0IiwicnAiOiJyZXBvcy9hMS81OC9hMTU4YWUyYWU4ZTJmNWJkZmQxOWI5YTFmMmJlMTQ5OWNjN2FhZjM0ZDM3MWI0MjM1NWNmY2ZkNDhhYjcyMmRhLmdpdCIsImlzcCI6dHJ1ZSwiZXhwaXJlX2F0IjoxNzg2OTU3MjAwfQ&signature=hAgvqtSg5jMESkzD-IScmskcVQVGWLn9N0aOfYnoZq4", DeskSweeperIcon);
            }
            else
            {
                await LoadImageFromWebAsync("https://avatars.githubusercontent.com/u/172395030?v=4", HeadImage);
                await LoadImageFromWebAsync("https://raw.githubusercontent.com/haaa4/NameCube/refs/heads/main/NameCube/icon.png", NameCubeIcon);
                await LoadImageFromWebAsync("https://raw.githubusercontent.com/haaa4/DeskSweeper/refs/heads/main/DeskSweeper.png", DeskSweeperIcon);
            }

         */
        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            VersionTextBlock.Text = GlobalVariablesData.VERSION;
            if (GlobalVariablesData.config.AllSettings.DownloadWay == 0)
            {
                await LoadImageFromWebAsync("https://avatars.githubusercontent.com/u/172395030?v=4", HeadImage);
                await LoadImageFromWebAsync("https://github.com/haaa4/SeatMapper/blob/master/SeatMapper/Image/icon.png?raw=true", SeatMapperIcon);
                await LoadImageFromWebAsync("https://raw.githubusercontent.com/haaa4/DeskSweeper/refs/heads/main/DeskSweeper.png", DeskSweeperIcon);
            }
            else
            {
                await LoadImageFromWebAsync("https://foruda.gitee.com/avatar/1777196359746152210/15207534_haaa4_1777196359.png!avatar200", HeadImage);
                await LoadImageFromWebAsync("https://raw.giteeusercontent.com/haaa4/SeatMapper/raw/master/SeatMapper/Image/icon.png?metadata=eyJyIjoibWFzdGVyIiwiZnAiOiJTZWF0TWFwcGVyL0ltYWdlL2ljb24ucG5nIiwidWlkIjoxNTIwNzUzNCwicGlkIjo0OTc0NTg3Mywic3RvIjoiZ2l0LXNoYXJkaW5nLXN0by00MnQtMDE0IiwicnAiOiJyZXBvcy8wNC81MS8wNDUxYzVhNTFkYjIxY2Y2Njg5MTYwODVlNWIzODhmODg2MDBiNjVjNjhjMDgwMmRiZjNlZDYxZTkyYTlkODdkLmdpdCIsImlzcCI6dHJ1ZSwiZXhwaXJlX2F0IjoxNzg3NTcyMjAwfQ&signature=c43QSbBGFppGrpQ8DhE2rZJt0AYNgkqGS6eUp97uvtE", SeatMapperIcon);
                await LoadImageFromWebAsync("https://raw.giteeusercontent.com/haaa4/DeskSweeper/raw/main/DeskSweeper.png?metadata=eyJyIjoibWFpbiIsImZwIjoiRGVza1N3ZWVwZXIucG5nIiwidWlkIjoxNTIwNzUzNCwicGlkIjo0OTc0NTg3NSwic3RvIjoiZ2l0LXNoYXJkaW5nLXN0by00MnQtMDE0IiwicnAiOiJyZXBvcy9hMS81OC9hMTU4YWUyYWU4ZTJmNWJkZmQxOWI5YTFmMmJlMTQ5OWNjN2FhZjM0ZDM3MWI0MjM1NWNmY2ZkNDhhYjcyMmRhLmdpdCIsImlzcCI6dHJ1ZSwiZXhwaXJlX2F0IjoxNzg2OTU3MjAwfQ&signature=hAgvqtSg5jMESkzD-IScmskcVQVGWLn9N0aOfYnoZq4", DeskSweeperIcon);
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ThanksWindow thanksWindow = new ThanksWindow();
            thanksWindow.ShowDialog();
        }
    }
}