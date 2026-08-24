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
using System.Windows;
using System.Windows.Controls;

namespace NameCube.ToolBox
{
    /// <summary>
    /// AutomaticProcess.xaml 的交互逻辑
    /// </summary>
    public partial class AutomaticProcess : Page
    {
        private static readonly ILogger _logger = Log.ForContext<AutomaticProcess>();

        public AutomaticProcess()
        {
            InitializeComponent();
            _logger.Debug("自动处理页面初始化");

            if (GlobalVariablesData.config.AutomaticProcess.debug)
            {
                DebugItem.Visibility = Visibility.Visible;
                _logger.Warning("自动处理调试模式已启用");
            }
            else
            {
                _logger.Debug("自动处理调试模式未启用");
            }
        }
    }
}