using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NameCube
{
    /// <summary>
    /// RepeatWarning.xaml 的交互逻辑
    /// </summary>
    public partial class RepeatWarning
    {
        public RepeatWarning()
        {
            InitializeComponent();
            if (GlobalVariables.json.AllSettings.NameCubeMode == 1)
            {
                Text.Text = "点鸣魔方已经启动，请勿再次启动";
                Image.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Environment.Exit(0);
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            Environment.Exit(0);
        }
    }
}
