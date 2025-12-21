using System;
using System.Windows.Media.Animation;
using Wpf.Ui;
using Serilog; // 添加Serilog引用

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

            if (GlobalVariables.json.AllSettings.NameCubeMode == 1)
            {
                BallSetting.Visibility = System.Windows.Visibility.Collapsed;
                _logger.Debug("当前为模式1，隐藏悬浮球设置");
            }

            if (GlobalVariables.json.AllSettings.Top)
            {
                Topmost = true;
                _logger.Debug("启用窗口置顶");
            }

            _logger.Information("设置窗口创建完成");
        }

        private void NavigationMenu_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationMenu.Navigate(typeof(Setting.Appearance));
            DebugItem.Visibility = GlobalVariables.json.AllSettings.debug ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

            _logger.Information("导航菜单加载完成，调试项可见性: {DebugVisible}", GlobalVariables.json.AllSettings.debug);
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