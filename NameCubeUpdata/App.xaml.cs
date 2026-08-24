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

using NameCubeSetup;
using System.Windows;

//接受这里的拼写错误，反正也不影响什么，哈哈哈
namespace NameCubeUpdata
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0 && e.Args[0] == "UpdateMode")
            {
                // 如果启动参数包含 "UpdateMode"，则直接打开更新窗口
                UpdateGuideWindow updateWindow = new UpdateGuideWindow();
                updateWindow.Show();
            }
            else
            {
                // 否则正常启动应用程序
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
        }
    }
}