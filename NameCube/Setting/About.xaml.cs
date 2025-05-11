using System.Diagnostics;
using System.IO;
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
        }

        int click = 0;
        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            click++;
            if(click==15)
            {
                //大哥，来这里找彩蛋就不要脸咯
                File.WriteAllText(Path.Combine(GlobalVariables.configDir, "FridUnknowCrickKind.dll"), "把此文件改为Fuck.txt，再重新打开软件试试");
                MessageBoxFunction.ShowMessageBoxInfo("彩蛋提示：\n配置文件啊，你多么高尚啊！可好像混入了一只蚯蚓，他在说：“草”！？？");
            }
        }

        private void Image_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

    }
}
