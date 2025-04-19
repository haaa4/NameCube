using NameCube.Setting;
using System.Linq;
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
        Timer Timer=new Timer();

        public void OnShowAfterLongPress()
        {
            NavigationMenu.Navigate(typeof(Mode.OnePeopleMode));
        }

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            DataContext = this;
            Timer.Interval = 2000;
            Timer.Tick += Timer_Tick;
            Timer.Start();
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
                NavigationMenu.Navigate(typeof(Mode.OnePeopleMode));
            };

        }

        private void Timer_Tick(object sender, System.EventArgs e)
        {
            this.Topmost = GlobalVariables.json.AllSettings.Top;
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

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();

            if (settingsWindow == null)
            {
                // 创建新实例
                settingsWindow = new SettingsWindow();
            }

            // 确保窗口可见并激活
            settingsWindow.Show();
            settingsWindow.Activate();
            settingsWindow.WindowState = WindowState.Normal;
        }

        private void CardAction_Click_1(object sender, RoutedEventArgs e)
        {
            var toolboxWindow = Application.Current.Windows.OfType<ToolBox.ToolboxWindow>().FirstOrDefault();

            if (toolboxWindow == null)
            {
                // 创建新实例
                toolboxWindow = new ToolBox.ToolboxWindow();
            }

            // 确保窗口可见并激活
            toolboxWindow.Show();
            toolboxWindow.Activate();
            toolboxWindow.WindowState = WindowState.Normal;
        }
    }
}
