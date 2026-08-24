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
    public class PrepareModeSetting
    {
        /// <summary>
        /// 是否启用朗读
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Speak { get; set; } = true;

        /// <summary>
        /// 是否允许修改
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Locked { set; get; } = false;

        /// <summary>
        /// 跳动速度
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Speed { get; set; } = 20;

        /// <summary>
        /// 上次的准备列表
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> Name { get; set; } = new List<string>();

        /// <summary>
        /// 上一次抽取的姓名
        /// </summary>
        public string LastName { get; set; }
    }
}