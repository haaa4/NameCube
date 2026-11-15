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
using NameCube.ToolBox.AutomaticProcessPages.ProcessPages;
using Serilog;
using System;
using System.Windows.Controls;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessSettingPages
{
    /// <summary>
    /// ShutDownSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class ShutDownSettingPage : Page
    {
        private bool canChange = false;

        /// <summary>
        /// 关机方式(0:立刻关机,1:一般关机,2:强制关机)
        /// </summary>
        public int shutDownWay;

        /// <summary>
        /// 全屏提醒内容，空字符串表示使用默认文案
        /// </summary>
        public string remindText = "";

        /// <summary>
        /// 倒计时秒数
        /// </summary>
        public int countdownSeconds = PowerOffPage.DefaultCountdownSeconds;

        public ShutDownSettingPage(ProcessData processData)
        {
            Log.Information("初始化关机设置页面");
            try
            {
                InitializeComponent();

                // 验证并设置关机方式
                if (processData.doubleData != double.NaN && processData.doubleData >= 0 && processData.doubleData <= 3)
                {
                    shutDownWay = (int)processData.doubleData;
                    Log.Debug("从流程数据获取关机方式: {ShutDownWay}", shutDownWay);
                }
                else
                {
                    shutDownWay = 0;
                    Log.Warning("流程数据中的关机方式无效，使用默认值 0");
                }

                remindText = processData.stringData1 ?? "";
                if (processData.shutDownCountdownSeconds >= 5)
                {
                    countdownSeconds = Math.Min(processData.shutDownCountdownSeconds, 3600);
                }

                ShutDownWayComboBox.SelectedIndex = shutDownWay;
                RemindTextBox.Text = remindText;
                CountdownNumberBox.Value = countdownSeconds;

                UpdateRemindHint();
                UpdateWayHint();
                canChange = true;

                Log.Information("关机设置页面初始化完成，关机方式: {ShutDownWay}, 提醒长度: {RemindLength}, 倒计时: {Countdown}秒",
                    shutDownWay, remindText.Length, countdownSeconds);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化关机设置页面时发生错误");
                throw;
            }
        }

        private static readonly string[] WayHints =
        {
            "立刻关机：倒计时结束后马上发送关机命令。",
            "一般关机：走系统默认关机流程，给正在运行的程序留出响应时间。",
            "强制关机：不等其他程序关闭，直接强制结束它们再关机（只是关得更干脆，不代表这次关机不能取消）。",
        };

        private void UpdateWayHint()
        {
            if (ShutDownWayHintText == null)
            {
                return;
            }

            int index = Math.Max(0, Math.Min(shutDownWay, WayHints.Length - 1));
            ShutDownWayHintText.Text = WayHints[index] + "\n能不能取消由流程组的「可取消」设置决定，和关机方式无关。";
        }

        private void UpdateRemindHint()
        {
            if (string.IsNullOrWhiteSpace(RemindTextBox.Text))
            {
                RemindHintText.Text = "当前使用默认提醒：" + PowerOffPage.DefaultRemindText;
            }
            else
            {
                RemindHintText.Text = $"已输入 {RemindTextBox.Text.Trim().Length} 个字符，会全屏居中显示";
            }
        }

        private void ShutDownWayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canChange && ShutDownWayComboBox.SelectedIndex >= 0)
            {
                int oldShutDownWay = shutDownWay;
                shutDownWay = ShutDownWayComboBox.SelectedIndex;

                Log.Information("关机方式从 {OldWay} 修改为 {NewWay}",
                    GetShutDownWayName(oldShutDownWay), GetShutDownWayName(shutDownWay));
                UpdateWayHint();
            }
        }

        private void RemindTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!canChange)
            {
                return;
            }

            remindText = RemindTextBox.Text ?? "";
            UpdateRemindHint();
            Log.Debug("修改关机提醒内容，长度: {Length}", remindText.Trim().Length);
        }

        private void CountdownNumberBox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (!canChange)
            {
                return;
            }

            if (!(CountdownNumberBox.Value >= 5))
            {
                Log.Warning("倒计时输入无效，重置为默认值 {Default}", PowerOffPage.DefaultCountdownSeconds);
                CountdownNumberBox.Value = PowerOffPage.DefaultCountdownSeconds;
                return;
            }

            countdownSeconds = (int)CountdownNumberBox.Value;
            Log.Debug("修改关机倒计时，秒数: {Countdown}", countdownSeconds);
        }

        private string GetShutDownWayName(int way)
        {
            switch (way)
            {
                case 0: return "立即关机";
                case 1: return "一般关机";
                case 2: return "强制关机";
                case 3: return "重启";
                default: return $"未知({way})";
            }
        }
    }
}
