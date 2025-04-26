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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NameCube.FirstUse
{
    /// <summary>
    /// NameCubeMode.xaml 的交互逻辑
    /// </summary>
    public partial class NameCubeMode : Page
    {
        public NameCubeMode()
        {
            InitializeComponent();
        }

        private void Media1_MediaEnded(object sender, RoutedEventArgs e)
        {
            Media1.Position = TimeSpan.Zero; // 将播放位置重置到视频开始处
            Media1.Play();
        }

        private void Media2_MediaEnded(object sender, RoutedEventArgs e)
        {
            Media2.Position = TimeSpan.Zero; // 将播放位置重置到视频开始处
            Media2.Play();
        }

        private void Media2_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBoxFunction.ShowMessageBoxError(e.ErrorException.ToString());
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            if(RadioButton1.IsChecked.Value)
            {
                GlobalVariables.json.AllSettings.NameCubeMode = 0;
            }
            else
            {
                GlobalVariables.json.AllSettings.NameCubeMode = 1;
            }
        }
    }
}
