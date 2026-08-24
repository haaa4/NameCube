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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace NameCube.Function
{
    internal class AppFunction
    {
        public static void Restart()
        {
            if(GlobalVariablesData.config.PermissionManager.needAdmin[0]&&(!CredentialHelper.PermissionVerification()))
            {
                return;
            }
            string[] args = Environment.GetCommandLineArgs();
            File.WriteAllText(
                Path.Combine(GlobalVariablesData.userDataDir, "START"),
                "The cake is a lie"//彩蛋而已
            );
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath,
                UseShellExecute = true  
            };
            Process.Start(startInfo);
            Application.Current.Shutdown();

        }
    }
}