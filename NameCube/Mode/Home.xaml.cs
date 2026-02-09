using Serilog;
using SixLabors.ImageSharp;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NameCube.Mode
{
    /// <summary>
    /// Home.xaml 的交互逻辑
    /// </summary>
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
            Log.Debug("Home页面初始化完成");
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("导航到单抽模式");
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.LoadPage(new OnePeopleMode());
        }

        private void CardAction_Click_1(object sender, RoutedEventArgs e)
        {
            Log.Information("导航到记忆因子模式");
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.LoadPage(new MemoryFactorMode());
        }

        private void CardAction_Click_2(object sender, RoutedEventArgs e)
        {
            Log.Information("导航到批量模式");
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.LoadPage(new BatchMode());
        }

        private void CardAction_Click_3(object sender, RoutedEventArgs e)
        {
            Log.Information("导航到数字模式");
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.LoadPage(new NumberMode());
        }

        private void CardAction_Click_4(object sender, RoutedEventArgs e)
        {
            Log.Information("导航到预备模式");
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.LoadPage(new PrepareMode());
        }

        private void CardAction_Click_5(object sender, RoutedEventArgs e)
        {
            Log.Information("导航到记忆模式");
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.LoadPage(new MemoryMode());
        }

        private void CloseRecommendButtom_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("关闭推荐显示");
            GlobalVariablesData.config.AllSettings.Recommend = GlobalVariablesData.VERSION;
            GlobalVariablesData.SaveConfig();
            SnackBarFunction.ShowSnackBarInMainWindow("本版本内，推荐将不再显示。如果想要永久关闭，请到应用设置->其他", Wpf.Ui.Controls.ControlAppearance.Info);
            Recommend.Visibility = Visibility.Collapsed;
            CloseRecommendButtom.Visibility = Visibility.Collapsed;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("Home页面加载");
            if (GlobalVariablesData.config.AllSettings.Recommend == GlobalVariablesData.VERSION || GlobalVariablesData.config.AllSettings.Recommend == "None")
            {
                Log.Debug("推荐已关闭");
                Recommend.Visibility = Visibility.Collapsed;
                CloseRecommendButtom.Visibility = Visibility.Collapsed;
            }
            else
            {
                Log.Debug("显示推荐内容");
            }
            Log.Information("Home页面加载完成");
        }
    }
}