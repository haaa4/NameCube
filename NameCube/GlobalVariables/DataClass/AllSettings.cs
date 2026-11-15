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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace NameCube.GlobalVariables.DataClass
{
    public class AllSettings
    {
        /// <summary>
        /// 姓名表
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> Name { get; set; } = new List<string> { "张三", "李四", "王五" };

        /// <summary>
        /// 黑暗模式
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Dark { get; set; } = false;

        /// <summary>
        /// 朗读音量
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Volume { get; set; } = 100;

        /// <summary>
        /// 朗读速度
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Speed { get; set; } = 0;

        /// <summary>
        /// 使用系统设置的朗读
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool SystemSpeech { get; set; } = false;

        /// <summary>
        /// 朗读引擎：System（系统自带 TTS）或 Edge（微软 Edge 在线朗读）
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string TtsEngine { get; set; } = SpeechEngines.System;

        /// <summary>
        /// Edge 朗读使用的音色短名（如 zh-CN-XiaoxiaoNeural），仅当 TtsEngine 为 Edge 时有效
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string EdgeVoice { get; set; } = "zh-CN-XiaoxiaoNeural";

        /// <summary>
        /// 是否把 Edge 朗读合成过的音频存到本地（user\tts-cache），下次同一句话直接播放。
        /// 用可空类型是为了区分“旧配置里没有这个字段”（null，按开启处理）和“用户手动关掉”（false）。
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool? TtsCacheEnabled { get; set; } = true;

        /// <summary>
        /// 主窗口始终置顶
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Top { get; set; } = true;

        /// <summary>
        /// 点鸣魔方模式（0:长期运行模式 1：单次运行模式）
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int NameCubeMode { get; set; } = 0;

        /// <summary>
        /// 字体颜色
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public Brush color { get; set; } =
            (Brush)new BrushConverter().ConvertFromInvariantString("#FF005493");

        /// <summary>
        /// 字体类型
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public FontFamily Font { get; set; } = new FontFamily("Arial");

        /// <summary>
        /// 上次更新的检查时间
        /// </summary>
        public string UpdataTime;

        /// <summary>
        /// 更新获取通道（0：仅正式版本，1：所有版本）
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int UpdataGet { get; set; } = 0;

        /// <summary>
        /// 是否显示推荐，如果为当前版本名或者None，则不显示
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Recommend { get; set; }

        /// <summary>
        /// 低内存模式
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool LowMemoryMode { get; set; }


        /// <summary>
        /// 新版本号
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string newVersion { get; set; }

        /// <summary>
        /// Debug模式
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool debug { get; set; }

        /// <summary>
        /// 禁用主界面显示动画
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DisableTheDisplayAnimationOfTheMainWindow { get; set; }

        /// <summary>
        /// 默认最大化窗口
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DefaultToMaximumSize { get; set; }

        /// <summary>
        /// 日志等级
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int LogLevel { get; set; } = 1;

        /// <summary>
        /// 日志保留天数
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int LogRetentionDays { get; set; } = 7;

        /// <summary>
        /// 数据获取方式
        /// （0=Github，1=Gitee）
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int DownloadWay { get; set; } = 0;
        /// <summary>
        /// 默认打开的页面
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int DefaultPage { get; set; } = 0;
        /// <summary>
        /// 是否启用主界面背景
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool HaveBackgroundImage { get; set; } = false;
        /// <summary>
        /// 背景图片拉伸方式
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public Stretch? BackgroundImageStretch { get; set; } =Stretch.Fill;
        /// <summary>
        /// 背景图片不透明度
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int? BackgroundOpacity { get; set; } = 100;
    }

    /// <summary>
    /// 朗读引擎的取值（与 config.json 里 TtsEngine 字段对应）。
    /// </summary>
    public static class SpeechEngines
    {
        /// <summary>系统自带 TTS（System.Speech）。</summary>
        public const string System = "System";

        /// <summary>微软 Edge 在线朗读（需要联网）。</summary>
        public const string Edge = "Edge";

        /// <summary>判断配置值是不是 Edge。</summary>
        public static bool IsEdge(string engine)
        {
            // 注意：本类里有个常量叫 System，会把命名空间 System 遮住，所以这里用 using System 引入的类型名
            return string.Equals(engine, Edge, StringComparison.OrdinalIgnoreCase);
        }
    }
}