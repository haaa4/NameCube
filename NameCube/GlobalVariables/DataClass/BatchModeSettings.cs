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
using System.Collections.Generic;

namespace NameCube.GlobalVariables.DataClass
{
    public class BatchModeSettings
    {
        /// <summary>
        /// 是否为数字模式
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool NumberMode { get; set; } = false;

        /// <summary>
        /// 数字数量
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Number { get; set; } = 53;

        /// <summary>
        /// 抽取数量
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Index { get; set; } = 10;

        /// <summary>
        /// 允许重复
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Repetition { get; set; } = false;

        /// <summary>
        /// 是否允许修改
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Locked { get; set; } = false;

        /// <summary>
        /// 上一次抽取的姓名表
        /// </summary>
        public List<string> LastName { get; set; } = new List<string>();
    }
}