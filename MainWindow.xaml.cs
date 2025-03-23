using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace NameCube
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private NotifyIcon _notifyIcon;

        public void OnShowAfterLongPress()
        {
            NavigationMenu.Navigate(typeof(OnePeopleMode));
        }

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            DataContext = this;

            if (GlobalVariables.json.AllSettings.Dark)
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                Wpf.Ui.Appearance.ApplicationTheme.Dark, // Theme type
                 Wpf.Ui.Controls.WindowBackdropType.Auto,  // Background type
                 true                                      // Whether to change accents automatically
               );
            }

            Loaded += (sender, args) =>
            {
                // 导航到第一个菜单项
                NavigationMenu.Navigate(typeof(OnePeopleMode));
            };

        }

        private void ShowWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal; // 恢复窗口状态
            this.Activate(); // 激活窗口到前台
        }

        private void ExitApp()
        {
            _notifyIcon.Dispose(); // 清理托盘图标
            Application.Current.Shutdown(); // 手动关闭应用
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
