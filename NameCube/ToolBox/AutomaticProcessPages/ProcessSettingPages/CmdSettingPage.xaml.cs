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

using NameCube.GlobalVariables.DataClass;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessSettingPages
{
    /// <summary>
    /// CmdSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class CmdSettingPage : Page
    {
        public string cmd;
        public bool? visibility;
        private bool canChange = false;

        public CmdSettingPage(ProcessData processData)
        {
            Log.Information("初始化CMD设置页面");
            try
            {
                InitializeComponent();
                cmd = processData.stringData1;
                visibility = processData.boolData;
                CmdTextBox.Text = cmd;
                VisibilityCheckBox.IsChecked = visibility;

                Log.Debug("CMD设置初始化 - 命令长度: {CmdLength}, 可见性: {Visibility}",
                    cmd?.Length ?? 0, visibility);

                canChange = true;
                Log.Information("CMD设置页面初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化CMD设置页面时发生错误");
                throw;
            }
        }

        private void CmdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (canChange)
            {
                string oldCmd = cmd;
                cmd = CmdTextBox.Text;

                if (oldCmd != cmd)
                {
                    Log.Debug("CMD命令从 {OldCmd} 修改为 {NewCmd}",
                        oldCmd ?? "空", cmd ?? "空");
                }
            }
        }

        private void VisibilityCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (canChange)
            {
                bool? oldVisibility = visibility;
                visibility = VisibilityCheckBox.IsChecked.Value;

                if (oldVisibility != visibility)
                {
                    Log.Information("CMD窗口可见性从 {OldVisibility} 修改为 {NewVisibility}",
                        oldVisibility, visibility);
                }
            }
        }
    }
}