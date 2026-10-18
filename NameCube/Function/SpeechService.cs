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
using System.Threading;

namespace NameCube.Function
{
    /// <summary>
    /// <see cref="ISpeechService"/> 的实现：整个程序共用一个 SpeechSynthesizer。
    /// </summary>
    /// <remarks>
    /// 三个要点：
    /// 1. 单例：SpeechSynthesizer 会加载系统语音引擎、占用音频设备，属于“昂贵且稀缺”的资源，
    ///    每个页面 new 一个既慢又容易互相抢麦。这里用 Lazy&lt;T&gt; 保证全局只有一份，且线程安全。
    /// 2. 懒初始化 + 失败缓存：真的要用语音时才创建；创建失败（阉割版 Win7）就记住失败，
    ///    之后所有调用直接静默返回，不再反复尝试、不再抛异常。
    /// 3. 可释放：持有 SpeechSynthesizer 就必须在程序退出时 Dispose，见 App.OnExit。
    /// </remarks>
    public sealed class SpeechService : ISpeechService
    {
        private static readonly ILogger _logger = Log.ForContext<SpeechService>();

        // Lazy<T> 的默认模式 ExecutionAndPublication 保证：多线程同时访问 Instance 时，
        // 工厂方法只会执行一次，其他线程等待它完成。
        private static readonly Lazy<ISpeechService> _lazyInstance =
            new Lazy<ISpeechService>(() => new SpeechService(), LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>全局唯一实例（对外暴露接口类型，而不是实现类型）。</summary>
        public static ISpeechService Instance => _lazyInstance.Value;

        // 一把锁保护下面所有共享状态：synthesizer 的创建/使用/释放可能发生在 UI 线程、
        // 定时器线程（System.Timers.Timer 的 Elapsed 在线程池上）甚至退出流程里。
        private readonly object _gate = new object();

        private SpeechSynthesizer _synthesizer;
        private bool _initializationFailed;
        private bool _runtimeErrorLogged;
        private string _lastError;
        private bool _disposed;

        // 私有构造函数：外部只能通过 Instance 拿到实例，防止有人 new 出第二个引擎。
        private SpeechService()
        {
        }

        public bool IsAvailable
        {
            get
            {
                EnsureInitialized();
                lock (_gate)
                {
                    return _synthesizer != null;
                }
            }
        }

        public string LastError
        {
            get
            {
                lock (_gate)
                {
                    return _lastError;
                }
            }
        }

        public void EnsureInitialized()
        {
            lock (_gate)
            {
                // 已释放、已确认失败、或已经创建过，都直接返回。
                if (_disposed || _initializationFailed || _synthesizer != null)
                {
                    return;
                }

                try
                {
                    // 这一步在阉割版 Win7 上会抛 COM/PlatformNotSupported 之类的异常
                    _synthesizer = new SpeechSynthesizer();
                    ApplySettingsCore();
                    _logger.Information("语音引擎初始化成功（系统语音: {SystemSpeech}）",
                        GlobalVariablesData.config.AllSettings.SystemSpeech);
                }
                catch (Exception ex)
                {
                    // 初始化失败是“永久性”的：记住它，别每次都重试
                    _initializationFailed = true;
                    _lastError = ex.Message;
                    _synthesizer = null;
                    _logger.Warning(ex, "语音引擎初始化失败，已禁用语音播报（常见于阉割版 Win7）");
                }
            }
        }

        public void Speak(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            EnsureInitialized();

            lock (_gate)
            {
                if (_disposed || _synthesizer == null)
                {
                    return;
                }

                try
                {
                    _synthesizer.SpeakAsync(text);
                }
                catch (Exception ex)
                {
                    // 朗读时出错属于“临时性”故障：不永久禁用，只记一次日志，下次还能再试
                    LogRuntimeErrorOnce(ex);
                }
            }
        }

        public void CancelAll()
        {
            lock (_gate)
            {
                if (_disposed || _synthesizer == null)
                {
                    return;
                }

                try
                {
                    _synthesizer.SpeakAsyncCancelAll();
                }
                catch (Exception ex)
                {
                    LogRuntimeErrorOnce(ex);
                }
            }
        }

        public void ApplySettings()
        {
            lock (_gate)
            {
                // 还没创建就不用管：创建时会按最新配置初始化
                if (_disposed || _synthesizer == null)
                {
                    return;
                }

                ApplySettingsCore();
            }
        }

        /// <summary>
        /// 按当前配置设置发音人/语速/音量。调用前必须已持有 _gate。
        /// </summary>
        private void ApplySettingsCore()
        {
            var settings = GlobalVariablesData.config.AllSettings;

            if (settings.SystemSpeech)
            {
                // 用户选择“系统语音”时保持系统默认音色，只把之前可能改过的参数还原
                _synthesizer.Rate = 0;
                _synthesizer.Volume = 100;
                return;
            }

            try
            {
                _synthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
            }
            catch (Exception ex)
            {
                // 找不到中文女声不是致命问题：继续用系统默认发音人朗读
                _logger.Debug(ex, "未找到可用女声，改用系统默认发音人");
            }

            _synthesizer.Rate = Math.Clamp(settings.Speed, -10, 10);
            _synthesizer.Volume = Math.Clamp(settings.Volume, 0, 100);
        }

        private void LogRuntimeErrorOnce(Exception ex)
        {
            if (_runtimeErrorLogged)
            {
                return;
            }

            _runtimeErrorLogged = true;
            _lastError = ex.Message;
            _logger.Warning(ex, "语音播报时发生异常，本次运行将不再重复记录");
        }

        /// <summary>
        /// 释放语音引擎。程序退出时由 App.OnExit 调用。
        /// 注意：这里没有终结器（~SpeechService），因为本类没有直接持有非托管句柄，
        /// SpeechSynthesizer 自己会负责它内部的 COM 资源，所以也不需要 GC.SuppressFinalize。
        /// </summary>
        public void Dispose()
        {
            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;

                try
                {
                    _synthesizer?.Dispose();
                }
                catch (Exception ex)
                {
                    // 释放阶段抛异常没有意义，吞掉即可（但不再吞得悄无声息）
                    _logger.Debug(ex, "释放语音引擎时发生异常，已忽略");
                }
                finally
                {
                    _synthesizer = null;
                }
            }
        }
    }
}
