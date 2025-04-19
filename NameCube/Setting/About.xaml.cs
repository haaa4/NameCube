using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.Setting
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class About : Page
    {
        public About()
        {
            InitializeComponent();
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/haaa4/NameCube");
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Process.Start("https://space.bilibili.com/1440486452");
        }
    }
}
