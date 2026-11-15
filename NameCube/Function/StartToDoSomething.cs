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

using NameCube.Function.Internet;
using NameCube.ToolBox.AutomaticProcessPages;
using Serilog;  // 添加Serilog命名空间
using System;
using System.Threading.Tasks;
using System.Windows;

namespace NameCube
{
    //傻逼名字
    internal class StartToDoSomething
    {
        
        /// <summary>
        /// 检查更新。方法内部已经捕获所有异常，调用方可以安全地“发射后不管”。
        /// </summary>
        public static async Task GetUpdata()
        {
            try
            {
                Log.Information("开始检查更新");

                if (!GlobalVariablesData.config.StartToDo.AutoUpdata)
                {
                    Log.Debug("自动更新已禁用，跳过检查");
                    return;
                }

                Log.Debug("自动更新已启用");

                // 必须 await：否则下面读到的还是旧的（默认）数据，永远判断成“已是最新版本”。
                var info = await GetInternetData.RefreshNewVersionInformationClassAsync();
                if (info == null)
                {
                    Log.Warning("检查更新失败：没有获取到服务器版本信息，本次跳过");
                    return;
                }

                string GetVersion;
                int GetVersionCode;
                if (GlobalVariablesData.config.AllSettings.UpdataGet == 0)
                {
                    GetVersion = info.LatestVersionName;
                    GetVersionCode = info.LatestVersionCode;
                }
                else
                {
                    GetVersion = info.PreReleaseVersionName;
                    GetVersionCode = info.PreReleaseVersionCode;
                }

                Log.Information("获取到最新版本: {LatestVersion}，检查时间: {CheckTime}",
                    GetVersion, DateTime.Now.ToString("s"));

                if (GetVersionCode > GlobalVariablesData.VERSIONCODE)
                {
                    Log.Information("发现新版本: {LatestVersion}，当前版本: {CurrentVersion}，显示通知",
                        GetVersion, GlobalVariablesData.VERSION);
                    GlobalVariablesData.config.AllSettings.newVersion = GetVersion;
                }
                else
                {
                    Log.Debug("当前已是最新版本: {CurrentVersion}", GlobalVariablesData.VERSION);
                    GlobalVariablesData.config.AllSettings.newVersion = null;
                }
                GlobalVariablesData.config.AllSettings.UpdataTime = DateTime.Now.ToString("f");

                Log.Information("更新检查完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "执行GetUpdata时发生错误");
            }
        }

        public static void RunAutomaticProcesses()
        {
            try
            {
                Log.Information("开始运行自动进程");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AutoToDo.StartRunAutomaticProcesses();
                });
                Log.Information("自动进程启动完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "运行自动进程时发生错误");
            }
        }

        //public static int ExtractVersionCode(string input)
        //{
        //    if (string.IsNullOrEmpty(input))
        //        throw new ArgumentException("输入字符串不能为空。");

        //    // 正则表达式解释：
        //    // \( 匹配左括号
        //    // # 匹配井号
        //    // [^#]* 匹配任意非#字符（如p或r）
        //    // (\d+) 捕获一组数字
        //    // # 匹配井号
        //    // \) 匹配右括号
        //    string pattern = @"\(#[^#]*(\d+)#\)";
        //    Match match = Regex.Match(input, pattern);

        //    if (match.Success)
        //    {
        //        return int.Parse(match.Groups[1].Value);
        //    }

        //    throw new ArgumentException("输入字符串中未找到有效的版本代码格式。");
        //}
    }
}