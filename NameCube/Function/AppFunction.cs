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