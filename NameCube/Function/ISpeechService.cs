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

using System;

namespace NameCube.Function
{
    /// <summary>
    /// 语音播报能力的抽象。
    /// 调用方（各个模式页面、转盘、设置页）只依赖这个接口，
    /// 不直接 new SpeechSynthesizer，也就不需要关心它是怎么创建、什么时候失败。
    /// </summary>
    public interface ISpeechService : IDisposable
    {
        /// <summary>
        /// 底层语音引擎是否可用。首次访问时会尝试创建引擎（懒初始化）。
        /// 阉割版 Win7 上这里会是 false，调用方据此决定是否提示用户。
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// 最近一次初始化失败的原因；初始化成功时为 null。
        /// </summary>
        string LastError { get; }

        /// <summary>
        /// 确保引擎已创建（幂等）。初始化失败不会抛异常，只会把 IsAvailable 置为 false。
        /// </summary>
        void EnsureInitialized();

        /// <summary>
        /// 异步朗读一段文本。引擎不可用或文本为空时静默返回，绝不抛异常。
        /// </summary>
        void Speak(string text);

        /// <summary>
        /// 立即停止所有正在排队/正在朗读的内容。
        /// </summary>
        void CancelAll();

        /// <summary>
        /// 按当前配置重新应用发音人、语速、音量（设置页修改滑块后调用）。
        /// </summary>
        void ApplySettings();
    }
}
