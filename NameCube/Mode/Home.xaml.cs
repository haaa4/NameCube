using Serilog;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            ChildrenPage.Navigate(new HomeChildrenPage.Photos());
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
            Log.Information("导航到记忆势能模式");
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

        private void CloseRecommendButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("关闭推荐显示");
            GlobalVariablesData.config.AllSettings.Recommend = GlobalVariablesData.VERSION;
            GlobalVariablesData.SaveConfig();
            SnackBarFunction.ShowSnackBarInMainWindow("本版本内，推荐将不再显示。如果想要永久关闭，请到应用设置->其他", Wpf.Ui.Controls.ControlAppearance.Info);
            //Recommend.Visibility = Visibility.Collapsed;
            CloseRecommendButton.Visibility = Visibility.Collapsed;
            ChildrenPage.Visibility = Visibility.Collapsed;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("Home页面加载");
            if (GlobalVariablesData.config.AllSettings.Recommend == GlobalVariablesData.VERSION || GlobalVariablesData.config.AllSettings.Recommend == "None")
            {
                Log.Debug("推荐已关闭");
                ChildrenPage.Visibility = Visibility.Collapsed;
                CloseRecommendButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                Log.Debug("显示推荐内容");
            }
            if (!GlobalVariablesData.config.ImformationData.UsedBetterColor)
            {
                if (GlobalVariablesData.config.AllSettings.color.ToString() == "#FF30D7D7")
                {
                    GlobalVariablesData.config.AllSettings.color = (Brush)
    new BrushConverter().ConvertFromInvariantString("#FF005493");
                    SnackBarFunction.ShowSnackBarInMainWindow("检测到您使用的是过去的推荐色，已自动切换到新的推荐色", Wpf.Ui.Controls.ControlAppearance.Info);
                    Log.Information("检测到用户使用的是过去的推荐色，已自动切换到新的推荐色");
                }
                GlobalVariablesData.config.ImformationData.UsedBetterColor = true;
                GlobalVariablesData.SaveConfig();
            }
            Log.Information("Home页面加载完成");
        }
    }
}