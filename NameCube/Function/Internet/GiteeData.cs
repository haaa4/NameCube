//// /*
////  * NameCube-<点名器>
////  * Copyright (C) 2025-2026 haaa4
////  *
////  * This program is free software: you can redistribute it and/or modify
////  * it under the terms of the GNU General Public License as published by
////  * the Free Software Foundation, either version 3 of the License, or
////  * (at your option) any later version.
////  *
////  * This program is distributed in the hope that it will be useful,
////  * but WITHOUT ANY WARRANTY

//using NameCube.Function.Internet;
//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Net.Http;
//using System.Threading.Tasks;

//namespace NameCube.Function
//{
//    internal class GiteeData
//    {

        

//        public static async Task<string> GerVersion()
//        {
//            try
//            {
//                if (GlobalVariablesData.config.AllSettings.UpdataGet == 0)
//                {
//                    await DownloadHelper.DownloadFileAsync("https://gitee.com/haaa4/NameCube/releases/download/V0.1/Version.txt", AppDomain.CurrentDomain.BaseDirectory + "\\TEMP.version");
//                }
//                else
//                {
//                    await DownloadHelper.DownloadFileAsync("https://gitee.com/haaa4/NameCube/releases/download/V0.1/Version.txt", AppDomain.CurrentDomain.BaseDirectory + "\\TEMP.version");
//                }
//                string get = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\TEMP.version");
//                if (get.EndsWith("\r\n"))
//                {
//                    get = get.Substring(0, get.Length - 2);
//                }
//                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\TEMP.version");
//                return get;
//            }
//            catch (Exception) { throw; }
//        }
//    }
//}