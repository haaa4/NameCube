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
using Serilog; // 添加Serilog引用
using System;
using System.Diagnostics;
using System.IO;
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
                _speechService?.ApplySettings(); // 立即生效，不用等下次打开模式页
                _logger.Debug("音量设置修改为: {Volume}", (int)VolumeSlider.Value);
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.Speed = (int)SpeedSlider.Value - 10;
                GlobalVariablesData.SaveConfig();
                _speechService?.ApplySettings(); // 立即生效，不用等下次打开模式页
                _logger.Debug("语速设置修改为: {Speed}", (int)SpeedSlider.Value - 10);
            }
        }

        private void SystemSpeechCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.SystemSpeech = SystemSpeechCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                _speechService?.ApplySettings(); // 立即生效，不用等下次打开模式页
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

        /// <summary>
        /// 打开旧版「语音属性」面板：Win7 等系统没有 ms-settings: 协议，
        /// 语音引擎（SAPI）的设置项在控制面板里，也就是 sapi.cpl 这个控制面板项。
        /// </summary>
        private void LegacySpeechSettings_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("尝试打开旧版语音属性面板");

            // 32 位进程在 64 位系统上访问 System32 会被 WOW64 重定向到 SysWOW64，
            // 想拿到真正的 64 位 System32 只能走 Sysnative；x64 进程则直接用 System32。
            string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string nativeSystem32 = Environment.Is64BitProcess
                ? Path.Combine(windows, "System32")
                : Path.Combine(windows, "Sysnative");

            // 依次尝试：本位数系统目录 → System32 / SysWOW64 里的副本（精简版系统可能只剩其一）
            var candidates = new[]
            {
                // (sapi.cpl 路径, 用来打开它的 control.exe)
                (Path.Combine(nativeSystem32, "Speech", "SpeechUX", "sapi.cpl"),
                 Path.Combine(nativeSystem32, "control.exe")),
                (Path.Combine(windows, "System32", "Speech", "SpeechUX", "sapi.cpl"), "control.exe"),
                (Path.Combine(windows, "SysWOW64", "Speech", "SpeechUX", "sapi.cpl"), "control.exe")
            };

            foreach (var (cpl, control) in candidates)
            {
                if (!File.Exists(cpl)) continue;

                // control.exe 缺失时退化为从 PATH 里找
                string exe = File.Exists(control) ? control : "control.exe";
                if (TryStart(exe, $"\"{cpl}\""))
                {
                    _logger.Debug("已通过 {Cpl} 打开语音属性", cpl);
                    return;
                }
            }

            // 回退：控制面板并没有公开的 canonical name，但部分系统上能识别，
            // 认不出来时 control.exe 会自己弹一个“找不到”的提示，不会崩
            if (TryStart("control.exe", "/name Microsoft.Speech"))
            {
                _logger.Debug("已通过 control.exe 打开语音属性");
                return;
            }

            _logger.Warning("未找到系统语音属性面板，已尝试 System32 / Sysnative / SysWOW64 下的 Speech\\SpeechUX\\sapi.cpl");
            SnackBarFunction.ShowSnackBarInMainWindow(
                "没有找到语音属性面板，这台电脑可能精简了语音组件",
                Wpf.Ui.Controls.ControlAppearance.Caution);
        }

        /// <summary>
        /// 启动一个外部程序，失败（找不到文件/被策略拦截）时返回 false 而不是抛异常。
        /// </summary>
        private bool TryStart(string fileName, string arguments)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "启动 {FileName} 失败", fileName);
                return false;
            }
        }

        private ISpeechService _speechService;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _speechService = SpeechService.Instance;
            _speechService.EnsureInitialized();

            if (!_speechService.IsAvailable)
            {
                _logger.Warning("语音引擎不可用: {Error}", _speechService.LastError);
                WarningBar.Visibility = Visibility.Visible;
                this.IsEnabled = false;
            }
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // 服务内部已经吞掉了所有异常，这里不需要 try
                _speechService?.Speak(TestTextBox.Text);
            }
        }
    }
}
