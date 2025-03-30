using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.Setting
{
    /// <summary>
    /// BirdSettings.xaml 的交互逻辑
    /// </summary>
    public partial class BirdSettings : Page
    {
        bool CanChange;
        public BirdSettings()
        {
            InitializeComponent();
            CanChange = false;
            BallCheck.IsChecked = GlobalVariables.json.StartToDo.Ball;
            CanChange = true;
        }
        private void BallCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.StartToDo.Ball = BallCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            Application.Current.Shutdown();
            Process.Start(Application.ResourceAssembly.Location, string.Join(" ", args.Skip(1)));
        }
    }
}
