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
using Serilog;  // 添加Serilog命名空间

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
            Log.Information("NameCubeMode页面初始化");
        }

        private void Media1_MediaEnded(object sender, RoutedEventArgs e)
        {
            Log.Debug("演示视频1播放结束，重新开始播放");
            Media1.Position = TimeSpan.Zero;
            Media1.Play();
        }

        private void Media2_MediaEnded(object sender, RoutedEventArgs e)
        {
            Log.Debug("演示视频2播放结束，重新开始播放");
            Media2.Position = TimeSpan.Zero;
            Media2.Play();
        }

        private void Media2_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Log.Error(e.ErrorException, "播放演示视频2时发生错误");
            MessageBoxFunction.ShowMessageBoxError(e.ErrorException.ToString());
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RadioButton1.IsChecked.Value)
                {
                    GlobalVariables.json.AllSettings.NameCubeMode = 0;
                    Log.Information("用户选择了悬浮球模式");
                }
                else
                {
                    GlobalVariables.json.AllSettings.NameCubeMode = 1;
                    Log.Information("用户选择了传统窗口模式");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "设置NameCube模式时发生错误");
            }
        }
    }
}