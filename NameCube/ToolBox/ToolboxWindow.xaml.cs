using NameCube.Function;
<<<<<<< HEAD
using NameCube.Setting;
=======
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
using Serilog;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Media.Animation;
using Wpf.Ui.Controls;

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

<<<<<<< HEAD

=======
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
            _logger.Information("工具箱窗口创建完成");
        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _logger.Debug("工具箱窗口加载完成，开始显示动画");

            NavigationMenu.Navigate(typeof(ToolBox.Welcome));
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

        private async void NavigationViewItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Wpf.Ui.Controls.ContentDialog();
            dialog = new ContentDialog()
            {
                CloseButtonText = "取消",
                PrimaryButtonText = "继续",
                Title = "即将打开网页，继续吗？",
            };
            var ToolBoxWindow = Application.Current.Windows.OfType<ToolboxWindow>().FirstOrDefault();
            dialog.DialogHostEx = ToolBoxWindow.RootContentDialogPresenter;
            Wpf.Ui.Controls.ContentDialogResult contentDialogResult = await dialog.ShowAsync();
            if (contentDialogResult == Wpf.Ui.Controls.ContentDialogResult.None)
            {
                Log.Debug("用户取消打开网页操作");
                dialog.Hide();
            }
            else
            {
                ProcessStartInfo processStartInfo = new();
                processStartInfo.FileName = "https://github.com/haaa4/DeskSweeper";
<<<<<<< HEAD
                processStartInfo.UseShellExecute= true;
=======
                processStartInfo.UseShellExecute = true;
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
                Process.Start(processStartInfo);
            }
        }
    }
}