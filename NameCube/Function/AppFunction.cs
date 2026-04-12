using System;
using System.Diagnostics;
using System.IO;
<<<<<<< HEAD
using System.Linq;
=======
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
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
<<<<<<< HEAD
                UseShellExecute = true  
            };
            Process.Start(startInfo);
            Application.Current.Shutdown();

=======
                UseShellExecute = true
            };
            Process.Start(startInfo);
            Application.Current.Shutdown();
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
        }
    }
}