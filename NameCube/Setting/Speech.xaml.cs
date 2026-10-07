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

using Serilog; // 添加Serilog引用
using System;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.Setting
{
    /// <summary>
    /// Speech.xaml 的交互逻辑
    /// </summary>
    public partial class Speech : Page
    {
        private static readonly ILogger _logger = Log.ForContext<Speech>(); // 添加Serilog日志实例
        private bool CanChange;

        public Speech()
        {
            InitializeComponent();
            _logger.Debug("Speech 页面初始化开始");

            CanChange = false;
            VolumeSlider.Value = GlobalVariablesData.config.AllSettings.Volume;
            SpeedSlider.Value = GlobalVariablesData.config.AllSettings.Speed + 10;
            SystemSpeechCheck.IsChecked = GlobalVariablesData.config.AllSettings.SystemSpeech;
            SpeedSlider.IsEnabled = !GlobalVariablesData.config.AllSettings.SystemSpeech;
            VolumeSlider.IsEnabled = !GlobalVariablesData.config.AllSettings.SystemSpeech;
            CanChange = true;

            _logger.Information("语音设置加载完成，音量: {Volume}, 语速: {Speed}, 系统语音: {SystemSpeech}",
                GlobalVariablesData.config.AllSettings.Volume,
                GlobalVariablesData.config.AllSettings.Speed,
                GlobalVariablesData.config.AllSettings.SystemSpeech);
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.Volume = (int)VolumeSlider.Value;
                GlobalVariablesData.SaveConfig();
                _logger.Debug("音量设置修改为: {Volume}", (int)VolumeSlider.Value);
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.Speed = (int)SpeedSlider.Value - 10;
                GlobalVariablesData.SaveConfig();
                _logger.Debug("语速设置修改为: {Speed}", (int)SpeedSlider.Value - 10);
            }
        }

        private void SystemSpeechCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.SystemSpeech = SystemSpeechCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                SpeedSlider.IsEnabled = !GlobalVariablesData.config.AllSettings.SystemSpeech;
                VolumeSlider.IsEnabled = !GlobalVariablesData.config.AllSettings.SystemSpeech;
                _logger.Information("系统语音设置修改为: {SystemSpeech}", SystemSpeechCheck.IsChecked.Value);
            }
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("尝试打开系统朗读人设置");
            try
            {
                // 使用系统协议直接打开 "设置" 应用中的朗读人界面
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:easeofaccess-narrator", // Win10/11 专用 URI
                    UseShellExecute = true // 必须启用 Shell 执行
                });
                _logger.Debug("成功打开系统朗读人设置");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "无法打开系统朗读人设置界面");
                Console.WriteLine($"无法打开设置界面: {ex.Message}");
            }
        }
        SpeechSynthesizer _speechSynthesizer = new();
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!GlobalVariablesData.config.AllSettings.SystemSpeech)
            {
                try
                {
                    _speechSynthesizer = new SpeechSynthesizer();
                    _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                    _speechSynthesizer.Rate = GlobalVariablesData.config.AllSettings.Speed;
                    _speechSynthesizer.Volume = GlobalVariablesData.config.AllSettings.Volume;
                }
                catch (Exception ex)
                {
                    Log.Warning("Win7 语音组件初始化失败，将禁用语音播报: " + ex.Message);
                    _speechSynthesizer = null; // 标记为 null，后续调用直接跳过
                    WarningBar.Visibility = Visibility.Visible;
                    this.IsEnabled = false;
                }
            }
            else
            {
                _logger.Debug("使用系统语音合成器");
            }
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                try
                {
                    _speechSynthesizer.SpeakAsync(TestTextBox.Text ?? "");
                }
                catch (Exception ex)
                {
                    Log.Warning("Win7 语音组件初始化失败，将禁用语音播报: " + ex.Message);
                    _speechSynthesizer = null; // 标记为 null，后续调用直接跳过
                    WarningBar.Visibility = Visibility.Visible;
                    this.IsEnabled = false;
                }
            }
        }
    }
}
