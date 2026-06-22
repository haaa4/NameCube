using NameCube.Function;
using Serilog; // 添加Serilog引用
using System.Windows.Media.Animation;
using Wpf.Ui.Controls;

namespace NameCube.Setting
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow
    {
        private static readonly ILogger _logger = Log.ForContext<SettingsWindow>(); // 添加Serilog日志实例

        public SettingsWindow()
        {
            InitializeComponent();
            _logger.Debug("SettingsWindow 初始化开始");

            if (GlobalVariablesData.config.AllSettings.NameCubeMode == 1)
            {
                Item5.Visibility = System.Windows.Visibility.Collapsed;
                _logger.Debug("当前为模式1，隐藏悬浮球设置");
            }

            _logger.Information("设置窗口创建完成");
        }

        private void NavigationMenu_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationMenu.Navigate(typeof(Setting.Welcome));
            DebugItem.Visibility = GlobalVariablesData.config.AllSettings.debug ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            _logger.Information("导航菜单加载完成，调试项可见性: {DebugVisible}", GlobalVariablesData.config.AllSettings.debug);
            Item1.IsEnabled = true;
            Item2.IsEnabled = true;
            Item3.IsEnabled = true;
            Item4.IsEnabled = true;
            Item5.IsEnabled = true;
            Item6.IsEnabled = true;
            Item7.IsEnabled = true;
            Item8.IsEnabled = true;
            Item9.IsEnabled = true;
            Item10.IsEnabled = true;
            Item11.IsEnabled = true;
            Item12.IsEnabled = true;
            if (!CredentialHelper.PermissionVerification())
            {
                Item1.IsEnabled = !GlobalVariablesData.config.PermissionManager.needAdmin[2];
                Item2.IsEnabled = !GlobalVariablesData.config.PermissionManager.needAdmin[3];
                Item3.IsEnabled = !GlobalVariablesData.config.PermissionManager.needAdmin[4];
                Item4.IsEnabled = !GlobalVariablesData.config.PermissionManager.needAdmin[5];
                Item5.IsEnabled = !GlobalVariablesData.config.PermissionManager.needAdmin[6];
                Item6.IsEnabled = !GlobalVariablesData.config.PermissionManager.needAdmin[7];
                Item7.IsEnabled = false;
                Item8.IsEnabled = false;
                Item9.IsEnabled =! GlobalVariablesData.config.PermissionManager.needAdmin[8];
                Item10.IsEnabled = !GlobalVariablesData.config.PermissionManager.needAdmin[9];
                Item11.IsEnabled = !GlobalVariablesData.config.PermissionManager.needAdmin[10];
                Item12.IsEnabled = !GlobalVariablesData.config.PermissionManager.needAdmin[11];
                
                this.Title = "应用设置（权限受限）";
                TitleBar.Title = "应用设置（权限受限）";
            }
            else
            {
                Item1.IsEnabled = true;
                Item2.IsEnabled = true;
                Item3.IsEnabled = true;
                Item4.IsEnabled = true;
                Item5.IsEnabled = true;
                Item6.IsEnabled = true;
                Item7.IsEnabled = true;
                Item8.IsEnabled = true;
                Item9.IsEnabled = true;
                Item10.IsEnabled = true;
                Item11.IsEnabled = true;
                Item12.IsEnabled = true;
                this.Title = "应用设置";
                TitleBar.Title = "应用设置";
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _logger.Information("用户点击重启按钮");
            AppFunction.Restart();
        }

        private void FluentWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var showStoryBoard = FindResource("ShowStoryBoard") as Storyboard;
            showStoryBoard.Stop();
            showStoryBoard.Remove();
            border.Visibility = System.Windows.Visibility.Visible;

            showStoryBoard.Completed += (s, en) =>
            {
                border.Visibility = System.Windows.Visibility.Collapsed;
                _logger.Debug("设置窗口显示动画完成");
            };

            showStoryBoard.Begin();
            _logger.Debug("开始设置窗口显示动画");
        }
    }
}