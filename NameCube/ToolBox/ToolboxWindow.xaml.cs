using Masuit.Tools.Win32;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Media.Animation;
using NameCube.Function;
using Serilog;

namespace NameCube.ToolBox
{
    /// <summary>
    /// ToolboxWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ToolboxWindow
    {
        private static readonly ILogger _logger = Log.ForContext<ToolboxWindow>();
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();

        public ToolboxWindow()
        {
            InitializeComponent();
            _logger.Debug("工具箱窗口初始化开始");

            if (GlobalVariablesData.config.AllSettings.Top)
            {
                Topmost = true;
                _logger.Debug("工具箱窗口启用置顶");
            }

            _logger.Information("工具箱窗口创建完成");
        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _logger.Debug("工具箱窗口加载完成，开始显示动画");

            NavigationMenu.Navigate(typeof(ToolBox.SpeechToolbox));
            _logger.Debug("导航到语音工具箱页面");

            var showStoryBoard = FindResource("ShowStoryBoard") as Storyboard;
            showStoryBoard.Stop();
            showStoryBoard.Remove();
            border.Visibility = System.Windows.Visibility.Visible;

            showStoryBoard.Completed += (s, en) =>
            {
                border.Visibility = System.Windows.Visibility.Collapsed;
                _logger.Debug("工具箱窗口显示动画完成");
            };

            showStoryBoard.Begin();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("用户点击工具箱窗口的重启按钮");
            AppFunction.Restart();
        }
    }
}