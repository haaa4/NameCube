using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

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
            if(GlobalVariables.json.AllSettings.Dark)
            {
                EggButton.Visibility=Visibility.Visible;
            }
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/haaa4/NameCube");
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Process.Start("https://space.bilibili.com/1440486452");
        }
        int click = 0;
        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            click++;
            if(click==15)
            {
                //大哥，来这里找彩蛋就不要脸咯
                MessageBoxFunction.ShowMessageBoxInfo("彩蛋提示：\n光明照亮一切，却让黑暗之下的东西不再显现");
            }
        }

        private void Image_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.bilibili.com/video/BV1MpWee8EtE?spm_id_from=333.788.recommend_more_video.-1&vd_source=13097fd622724d879280b908f9c3796e");
        }
    }
}
