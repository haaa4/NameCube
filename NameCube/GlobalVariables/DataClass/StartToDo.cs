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

namespace NameCube.GlobalVariables.DataClass
{
    public class StartToDo
    {
        /// <summary>
        /// 启用朗读球
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Ball { get; set; } = false;

        /// <summary>
        /// 自动内存清理
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AlwaysCleanMemory { get; set; } = false;

        /// <summary>
        /// 自动请求升级
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AutoUpdata { get; set; } = true;
    }
}