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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameCube.GlobalVariables.DataClass
{
    public class PermissionManager
    {
        /// <summary>
        /// 用户的USB设备哈希值，用于权限验证
        /// </summary>
        public string usbHash { get; set; } = string.Empty;
        /// <summary>
        /// 各部分功能是否需要管理员权限，按照顺序对应请查看PermissionManagerSettings.xaml
        /// </summary>
        public Dictionary<string,bool> needAdminUser { get; set; }= new Dictionary<string, bool>()
        {
            {"Restart",false},
            {"MainWindow",false},
            {"Archives",false},
            {"ModeSetting",false},
            {"Appearance",false},
            {"Speech",false},
            {"BirdSetting",false},
            {"ShortCutKeySetting",false},
            {"LogSetting",false},
            {"UpData",false},
            {"WindowsAndRendering",false},
            {"Other",false},
            {"About",false},
            {"MemoryToolbox",false},
            {"AutomaticProcess",false},
        };
        /*
         * 
            "Restart"
            "MainWindow"
            "Archives"
            "ModeSetting
            "Appearance"
            "Speech"
            "BirdSetting"
            "ShortCutKeySetting"
            "LogSetting"
            "UpData"
            "WindowsAndRendering"
            "Other"
            "About"
            "MemoryToolbox"
            "AutomaticProcess"
         */
    }
}
