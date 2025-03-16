using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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

        public void SaveJson()
        {
            string jsonString = JsonSerializer.Serialize(json);
            File.WriteAllText("config.json", jsonString);
        }
        Json json = new Json();
        public void OnShowAfterLongPress()
        {
            NavigationMenu.Navigate(typeof(OnePeopleMode));
        }

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            InitializeTrayIcon();
            DataContext = this;
            if (!File.Exists("config.json"))
            {
                json = new Json
                {
                    Name = new List<string>(),
                    Speech = true,
                    Dark = false,
                    Volume = 100,
                    Speed = 0,
                    Ball = true
                };
                json.Name.Add("张三");

                SaveJson();
            }
            else
            {
                string jsonstring = File.ReadAllText("config.json");
                json = JsonSerializer.Deserialize<Json>(jsonstring);
                if (json.Dark)
                {
                    Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                    Wpf.Ui.Appearance.ApplicationTheme.Dark, // Theme type
                     Wpf.Ui.Controls.WindowBackdropType.Auto,  // Background type
                     true                                      // Whether to change accents automatically
                   );
                }
            }

            Loaded += (sender, args) =>
            {
                // 导航到第一个菜单项
                NavigationMenu.Navigate(typeof(OnePeopleMode));
            };

        }
        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath),
                Visible = true,
                Text = "学号魔方"
            };

            // 添加右键菜单
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("显示窗口", null, (s, e) => ShowWindow());
            contextMenu.Items.Add("退出", null, (s, e) => ExitApp());
            _notifyIcon.ContextMenuStrip = contextMenu;

            // 双击托盘图标显示窗口
            _notifyIcon.DoubleClick += (s, e) => ShowWindow();
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
