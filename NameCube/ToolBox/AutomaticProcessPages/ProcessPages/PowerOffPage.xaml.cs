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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessPages
{
    /// <summary>
    /// PowerOffPage.xaml 的交互逻辑
    /// 触发时会把宿主窗口切成全屏，显示设置里指定的提醒文字并开始倒计时。
    /// </summary>
    public partial class PowerOffPage : Page
    {
        /// <summary>
        /// 未设置提醒内容时使用的默认文案
        /// </summary>
        public const string DefaultRemindText = "电脑即将关机，请尽快保存正在编辑的内容。";

        /// <summary>
        /// 默认倒计时秒数
        /// </summary>
        public const int DefaultCountdownSeconds = 30;

        private const int MinCountdownSeconds = 5;
        private const int MaxCountdownSeconds = 3600;

        private readonly int shutDown;
        private readonly bool debug;
        private readonly bool canCancle;
        private readonly int countdownSeconds;
        private readonly string remindText;

        private DispatcherTimer timer;
        private DateTime deadline;
        private bool finished;
        private bool restoredWindow;
        private Window hostWindow;
        private bool fullScreenApplied;

        public event Action<string> RequestParentAction;

        public event Action<int> EndThePageAction;

        /// <summary>
        /// 关机页面
        /// </summary>
        /// <param name="shutDownWay">(0:立刻关机,1:一般关机，2:强制关机)</param>
        /// <param name="isDebug">是否为调试模式（不会真的关机）</param>
        /// <param name="show">是否显示窗口</param>
        /// <param name="remindText">全屏提醒文字，留空使用默认文案</param>
        /// <param name="canCancle">是否允许取消关机</param>
        /// <param name="countdownSeconds">倒计时秒数，小于 5 时使用默认值 30</param>
        public PowerOffPage(
            int shutDownWay,
            bool isDebug = false,
            bool show = true,
            string remindText = null,
            bool canCancle = true,
            int countdownSeconds = 0)
        {
            Log.Information(
                "初始化关机页面 - 关机方式: {ShutDownWay}, 调试模式: {Debug}, 显示: {Show}, 可取消: {CanCancle}, 倒计时: {Countdown}秒",
                GetShutDownWayName(shutDownWay), isDebug, show, canCancle, countdownSeconds);

            InitializeComponent();

            shutDown = shutDownWay;
            debug = isDebug;
            this.canCancle = canCancle;
            this.remindText = string.IsNullOrWhiteSpace(remindText) ? DefaultRemindText : remindText.Trim();
            this.countdownSeconds = NormalizeCountdown(countdownSeconds);

            Focusable = true;
            RemindTextBlock.Text = this.remindText;
            CountdownText.Text = this.countdownSeconds.ToString();
            FootText.Text = BuildFootText();

            if (!canCancle)
            {
                CancelButton.Visibility = Visibility.Collapsed;
                Log.Debug("流程设置为不可取消，已隐藏「继续使用电脑」按钮");
            }

            Loaded += Page_Loaded;
            Unloaded += Page_Unloaded;
            PreviewKeyDown += Page_PreviewKeyDown;
        }

        private static int NormalizeCountdown(int seconds)
        {
            if (seconds < MinCountdownSeconds)
            {
                return DefaultCountdownSeconds;
            }

            return Math.Min(seconds, MaxCountdownSeconds);
        }

        private string BuildFootText()
        {
            string way = GetShutDownWayName(shutDown);
            if (shutDown == 2)
            {
                way += "（不等待其他程序关闭）";
            }

            if (canCancle)
            {
                return $"关机方式：{way}　·　倒计时结束后自动关机，点击「继续使用电脑」或按 Esc 可以取消";
            }

            return $"关机方式：{way}　·　倒计时结束后自动关机，流程组设置为「不可取消」";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateCountdownUi(countdownSeconds);
            EnterFullScreen();

            deadline = DateTime.UtcNow.AddSeconds(countdownSeconds);
            timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(200) };
            timer.Tick += Timer_Tick;
            timer.Start();

            Focus();
            Log.Information("关机提醒已开始，倒计时 {Countdown} 秒", countdownSeconds);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            StopTimer();
            RestoreWindow();
        }

        private void StopTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
                timer = null;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeSpan left = deadline - DateTime.UtcNow;
            if (left.TotalMilliseconds <= 0)
            {
                StopTimer();
                ExecuteShutDown("倒计时结束");
                return;
            }

            int remain = (int)Math.Ceiling(left.TotalSeconds);
            UpdateCountdownUi(remain, left.TotalMilliseconds / (countdownSeconds * 1000.0));
        }

        private void UpdateCountdownUi(int remainSeconds, double progressRatio = 1)
        {
            CountdownText.Text = Math.Max(0, remainSeconds).ToString();
            CountdownProgress.Value = Math.Max(0, Math.Min(100, progressRatio * 100));
        }

        private void Page_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && canCancle)
            {
                e.Handled = true;
                Button_Click(null, null);
            }
        }

        private void EnterFullScreen()
        {
            hostWindow = Window.GetWindow(this);
            if (hostWindow == null || debug)
            {
                return;
            }

            try
            {
                if (!hostWindow.IsVisible)
                {
                    hostWindow.Show();
                }

                hostWindow.Activate();

                if (hostWindow is ProcessesRunningWindow processesRunningWindow)
                {
                    processesRunningWindow.EnterFullScreenMode();
                }
                else
                {
                    hostWindow.WindowStyle = WindowStyle.None;
                    hostWindow.ResizeMode = ResizeMode.NoResize;
                    hostWindow.WindowState = WindowState.Maximized;
                    hostWindow.Topmost = true;
                }

                fullScreenApplied = true;
                if (!canCancle)
                {
                    hostWindow.Closing += HostWindow_Closing;
                }

                Log.Debug("关机提醒已切换到全屏模式");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "切换全屏提醒时发生错误");
            }
        }

        private void HostWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!finished && !canCancle)
            {
                e.Cancel = true;
                Log.Warning("流程设置为不可取消，已拦截关闭窗口操作");
            }
        }

        private void RestoreWindow()
        {
            if (restoredWindow || !fullScreenApplied || hostWindow == null)
            {
                return;
            }

            restoredWindow = true;
            try
            {
                hostWindow.Closing -= HostWindow_Closing;

                if (hostWindow is ProcessesRunningWindow processesRunningWindow)
                {
                    processesRunningWindow.ExitFullScreenMode();
                }
                else
                {
                    hostWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                    hostWindow.ResizeMode = ResizeMode.CanResize;
                    hostWindow.WindowState = WindowState.Normal;
                    hostWindow.Topmost = false;
                }

                Log.Debug("已退出全屏提醒模式");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "恢复窗口样式时发生错误");
            }
            finally
            {
                fullScreenApplied = false;
            }
        }

        private void PowerOffButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击关机按钮 - 关机方式: {ShutDownWay}", GetShutDownWayName(shutDown));
            StopTimer();
            ExecuteShutDown("用户点击关机");
        }

        private void ExecuteShutDown(string reason)
        {
            if (finished)
            {
                return;
            }

            finished = true;
            PowerOffButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            StatusText.Text = "正在关机…";

            Log.Information("执行关机 - 原因: {Reason}, 关机方式: {ShutDownWay}", reason, GetShutDownWayName(shutDown));

            if (debug)
            {
                RestoreWindow();
                if (shutDown == 0)
                {
                    CallParentMethodDebug("立刻关机");
                }
                else if (shutDown == 1)
                {
                    CallParentMethodDebug("一般关机");
                }
                else
                {
                    CallParentMethodDebug("强制关机");
                }
                return;
            }

            string arguments;
            if (shutDown == 0)
            {
                arguments = "/c shutdown /s /t 0";
            }
            else if (shutDown == 1)
            {
                arguments = "/c shutdown /s";
            }
            else
            {
                arguments = "/c shutdown /s /f /t 0";
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                };

                Log.Information("执行关机命令 - 参数: {Arguments}", arguments);

                using (Process process = Process.Start(startInfo))
                {
                    process?.WaitForExit();
                }

                Log.Information("关机命令执行完成");
                StatusText.Text = "关机命令已发送，如果系统没有响应可以手动关机";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "执行关机命令时发生错误");
                StatusText.Text = "关机命令执行失败：" + ex.Message;
                PowerOffButton.IsEnabled = true;
            }
        }

        private void CallEnd(string debugText, bool debug, int ret = 0)
        {
            Log.Debug("调用结束方法 - 调试文本: {DebugText}, 调试模式: {Debug}, 返回码: {ReturnCode}",
                debugText, debug, ret);
            if (debug)
            {
                CallParentMethodDebug(debugText);
            }
            else
            {
                CallEndThePage(ret);
            }
        }

        private void CallEndThePage(int ret = 0)
        {
            Log.Debug("调用结束页面，返回码: {ReturnCode}", ret);
            EndThePageAction?.Invoke(ret);
        }

        private void CallParentMethodDebug(string data)
        {
            Log.Debug("调用父页面调试方法，数据: {Data}", data);
            RequestParentAction?.Invoke(data);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!canCancle)
            {
                Log.Warning("流程设置为不可取消，忽略取消关机的请求");
                return;
            }

            Log.Information("用户取消了关机");
            StopTimer();
            RestoreWindow();
            CallEnd("用户没有关机", debug, 1);
        }

        private string GetShutDownWayName(int way)
        {
            switch (way)
            {
                case 0: return "立刻关机";
                case 1: return "一般关机";
                case 2: return "强制关机";
                case 3: return "重启";
                default: return $"未知({way})";
            }
        }
    }
}
