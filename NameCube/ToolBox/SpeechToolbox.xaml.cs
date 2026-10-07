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

using Serilog;
using System;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NameCube.ToolBox
{
    /// <summary>
    /// SpeechToolbox.xaml 的交互逻辑
    /// </summary>
    public partial class SpeechToolbox : Page
    {
        private static readonly ILogger _logger = Log.ForContext<SpeechToolbox>();
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();

        public SpeechToolbox()
        {
            InitializeComponent();
            _logger.Debug("语音工具箱页面初始化");

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
                }
            }
            else
            {
                _logger.Debug("使用系统语音合成器");
            }
        }

        private void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            string textToRead = Read1.Text + Read2.Text;
            _logger.Information("开始朗读文本，长度: {Length}", textToRead.Length);

            try { _speechSynthesizer?.SpeakAsyncCancelAll(); } catch { /* 忽略 Win7 的 COM 异常 */ }
            try
            {
                _speechSynthesizer?.SpeakAsync(textToRead);
            }
            catch { /* 忽略 */ }

            _logger.Debug("朗读任务已启动");
        }

        private void Read1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Read2.Focus();
                _logger.Debug("焦点从Read1移动到Read2");
            }
        }

        private void Read2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                _logger.Debug("在Read2中按Enter键，触发朗读");
                ReadButton_Click(sender, e);
            }
        }
    }
}