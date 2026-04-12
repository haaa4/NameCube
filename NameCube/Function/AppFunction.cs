using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace NameCube.Function
{
    internal class AppFunction
    {
        public static void Restart()
        {
            string[] args = Environment.GetCommandLineArgs();
            File.WriteAllText(
                Path.Combine(GlobalVariablesData.configDir, "START"),
                "The cake is a lie"
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