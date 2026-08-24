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

using Serilog; // 添加Serilog引用
using System.Windows.Controls;

namespace NameCube.Setting
{
    /// <summary>
    /// ModeSettings.xaml 的交互逻辑
    /// </summary>
    public partial class ModeSettings : Page
    {
        private static readonly ILogger _logger = Log.ForContext<ModeSettings>(); // 添加Serilog日志实例

        public ModeSettings()
        {
            InitializeComponent();
            _logger.Debug("ModeSettings 初始化完成");
        }
    }
}