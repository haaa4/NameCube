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
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;

namespace NameCube.Function
{
    /// <summary>
    /// <see cref="ISpeechService"/> 的实现：整个程序共用一个朗读引擎。
    /// </summary>
    /// <remarks>
    /// 三个要点：
    /// 1. 单例：SpeechSynthesizer 会加载系统语音引擎、占用音频设备，属于“昂贵且稀缺”的资源，
    ///    每个页面 new 一个既慢又容易互相抢麦。这里用 Lazy&lt;T&gt; 保证全局只有一份，且线程安全。
    /// 2. 懒初始化 + 失败缓存：真的要用语音时才创建；创建失败（阉割版 Win7）就记住失败，
    ///    之后所有调用直接静默返回，不再反复尝试、不再抛异常。
    /// 3. 双引擎：配置里 TtsEngine 决定走系统 TTS 还是 Edge 在线朗读，调用方完全不用关心。
    ///    系统引擎是同步排队的；Edge 引擎是“后一条顶掉前一条”（在线合成有延迟，排长队没意义）。
    /// 4. 可释放：持有 SpeechSynthesizer 就必须在程序退出时 Dispose，见 App.OnExit。
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

        // Edge 引擎的状态：播放器、当前朗读的取消源、以及后台任务（释放时等它结束）。
        private readonly EdgeTtsPlayer _edgePlayer = new EdgeTtsPlayer();
        private CancellationTokenSource _edgeCts;
        private Task _edgeTask = Task.CompletedTask;

        // 私有构造函数：外部只能通过 Instance 拿到实例，防止有人 new 出第二个引擎。
        private SpeechService()
        {
        }

        private bool UseEdge => SpeechEngines.IsEdge(GlobalVariablesData.config.AllSettings.TtsEngine);

        public bool IsAvailable
        {
            get
            {
                if (UseEdge)
                {
                    // Edge 引擎不需要本地语音组件，能不能出声要等真正合成时才知道，
                    // 这里不能因为系统没装中文语音就把整个语音功能判死。
                    return true;
                }

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
            // Edge 引擎不需要初始化本地语音合成器
            if (UseEdge)
            {
                return;
            }

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

            if (UseEdge)
            {
                SpeakWithEdge(text);
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
            if (UseEdge)
            {
                CancelEdge();
            }

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
            if (UseEdge)
            {
                // Edge 引擎每次朗读时才读配置，这里只要把正在念的旧内容停下，
                // 让新设置从下一句开始生效即可。
                CancelEdge();
                return;
            }

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
        /// 用 Edge 在线引擎朗读：先取消上一条（在线合成有延迟，排队只会越来越滞后），
        /// 再在后台线程合成 + 播放。所有异常都吞掉并记日志，不打断点名流程。
        /// </summary>
        private void SpeakWithEdge(string text)
        {
            var settings = GlobalVariablesData.config.AllSettings;
            string voice = string.IsNullOrWhiteSpace(settings.EdgeVoice)
                ? "zh-CN-XiaoxiaoNeural"
                : settings.EdgeVoice;

            // 系统语音的 Speed 是 -10~10，Edge 用百分比表达，乘 10 后正好是 -100%~100%
            int ratePercent = Math.Clamp(settings.Speed, -10, 10) * 10;
            int volume = Math.Clamp(settings.Volume, 0, 100);

            CancellationTokenSource cts;
            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                _edgeCts?.Cancel();
                _edgeCts?.Dispose();
                cts = new CancellationTokenSource();
                _edgeCts = cts;

                // 上一条还在念的话立刻掐掉，避免两句话叠在一起
                _edgePlayer.Stop();
            }

            _edgeTask = Task.Run(async () =>
            {
                try
                {
                    byte[] audio = await EdgeTtsClient
                        .SynthesizeAsync(text, voice, ratePercent, cts.Token)
                        .ConfigureAwait(false);
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }

                    _edgePlayer.Play(audio, volume, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // 正常的“被下一条打断”，不算错误
                }
                catch (Exception ex)
                {
                    lock (_gate)
                    {
                        _lastError = ex.Message;
                    }

                    _logger.Warning(ex, "Edge 朗读失败（音色 {Voice}，文本长度 {Length}）", voice, text.Length);
                }
            });
        }

        /// <summary>停掉 Edge 引擎当前正在合成/播放的内容。</summary>
        private void CancelEdge()
        {
            lock (_gate)
            {
                try
                {
                    _edgeCts?.Cancel();
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "取消 Edge 朗读时发生异常，已忽略");
                }

                _edgePlayer.Stop();
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
            CancellationTokenSource edgeCts;
            Task edgeTask;

            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                edgeCts = _edgeCts;
                _edgeCts = null;
                edgeTask = _edgeTask;
            }

            try
            {
                edgeCts?.Cancel();
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "退出时取消 Edge 朗读发生异常，已忽略");
            }

            // 给后台合成一小段时间收尾，避免进程退出时还在往已释放的播放器里写数据
            try
            {
                edgeTask?.Wait(TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "等待 Edge 朗读任务结束时发生异常，已忽略");
            }

            _edgePlayer.Dispose();
            edgeCts?.Dispose();

            lock (_gate)
            {
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
