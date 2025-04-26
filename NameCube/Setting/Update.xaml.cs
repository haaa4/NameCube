using Masuit.Tools.Files;
using Masuit.Tools.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NameCube.Setting
{
    /// <summary>
    /// Update.xaml 的交互逻辑
    /// </summary>
    public partial class Update : Page
    {
        public Update()
        {
            InitializeComponent();
            VersionText.Text=GlobalVariables.Version;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!IsRunAsAdmin())
            {
                MessageBoxFunction.ShowMessageBoxInfo("我们需要获得您的管理员权限");
                File.WriteAllText(System.IO.Path.Combine(GlobalVariables.configDir, "START"), "The cake is a lie");
                // 重新启动程序并请求管理员权限
                var exeName = Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo startInfo = new ProcessStartInfo(exeName)
                {
                    Verb = "runas", // 触发UAC提示
                    UseShellExecute = true
                };

                try
                {
                    Process.Start(startInfo);
                    Application.Current.Shutdown();
                }
                catch
                {
                    MessageBoxFunction.ShowMessageBoxInfo("我们需要获得您的管理员权限");
                    Application.Current.Shutdown();
                }
                return;
            }

            Ring.Visibility = Visibility.Visible;
            ComputerButton.IsEnabled = false;
            NowText.Text = "解压缩包中......";
            UpdataFromThisComputer();
        }
        private static bool IsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private async Task UpdataFromThisComputer()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "从本地安装";
            openFileDialog.Filter = "安装包 (*.zip)|*.zip|所有文件 (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                try
                { 
                    Directory.CreateDirectory("Updata");
                    SevenZipCompressor.Decompress(openFileDialog.FileName, "Updata");
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        NowText.Text = "解压缩完成";
                        ComputerButton.IsEnabled = true;
                        Ring.Visibility = Visibility.Hidden;
                        Application.Current.Shutdown();
                        Process.Start("NameUpdate.exe");
                    }));
                }
                catch(Exception ex)
                {
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        NowText.Text = "安装中断...";
                        ComputerButton.IsEnabled = true;
                        Ring.Visibility = Visibility.Hidden;
                    }));
                }
            }
            else
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    NowText.Text = "安装中断...";
                    ComputerButton.IsEnabled = true;
                    Ring.Visibility = Visibility.Hidden;
                }));
                return;
            }
        }
    }
}
