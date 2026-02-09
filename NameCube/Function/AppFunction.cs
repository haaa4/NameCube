using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Application.Current.Shutdown();
            Process.Start(Application.ResourceAssembly.Location, string.Join(" ", args.Skip(1)));
        }
    }
}
