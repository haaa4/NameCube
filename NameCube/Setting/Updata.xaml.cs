using Masuit.Tools.Files;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using File = System.IO.File;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Threading;
using Masuit.Tools.Net;
using System.Net.Http;
using System.Net;
using System.Windows.Forms;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace NameCube.Setting
{
    /// <summary>
    /// Updata.xaml 的交互逻辑
    /// </summary>
    public partial class Updata : Page
    {
        public Updata()
        {
            InitializeComponent();
            Canchange = false;
            VersionText.Text = GlobalVariables.Version;
            UpdataWayComboBox.SelectedIndex = GlobalVariables.json.AllSettings.UpdataGet;
            AutoStart.IsChecked = GlobalVariables.json.StartToDo.AutoUpdata;
            Canchange = true;

            if (GlobalVariables.json.AllSettings.UpdataTime != null)
            {
                CheckText.Text = "上次检查时间:" + GlobalVariables.json.AllSettings.UpdataTime;
            }
            else
            {
                CheckText.Text = "上次检查时间:从未检查过";
            }
        }
        bool Canchange;
        private void UpdataFromComputerButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "从本地安装";
            openFileDialog.Filter = "安装包 (*.zip)|*.zip|所有文件 (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Task.Run(() => UpdataFromThisComputer(openFileDialog.FileName, GlobalVariables.configDir));
                CaseText.Text = "解压缩包中...";
                NowProgressBar.IsIndeterminate = true;
                CheckButton.IsEnabled = false;
                UpdataFromComputerButton.IsEnabled = false;
            }
        }

        public void UpdataFromThisComputer(string file, string UpdataFile)
        {
            try
            {
                string path = Path.Combine(UpdataFile, "Updata", "Data");
                if (Directory.Exists(Path.Combine(UpdataFile, "Updata")))
                {
                    Directory.Delete(Path.Combine(UpdataFile, "Updata"), true);
                }
                Directory.CreateDirectory(path);
                var SevenZipCompressor = new SevenZipCompressor(null);
                SevenZipCompressor.Decompress(file, path);
            }
            catch (Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError(ex.Message);
                this.Dispatcher.Invoke(() =>
                {
                    NowProgressBar.IsIndeterminate = false;
                    CheckButton.IsEnabled = true;
                    UpdataFromComputerButton.IsEnabled = true;
                    CaseText.Text = "解压缩失败";
                });
            }
            this.Dispatcher.Invoke(() =>
            {
                CaseText.Text = "准备安装程序...";
            });
            try
            {
                string path = Path.Combine(UpdataFile, "Updata");
                File.WriteAllText(Path.Combine(path, "Path.txt"), Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                File.Copy(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NameCubeUpdata.exe"), Path.Combine(path, "NameCubeUpdata.exe"));
                MessageBoxFunction.ShowMessageBoxInfo("更新程序即将启动，请勿关闭程序,否则可能会导致文件损坏！");
                new ToastContentBuilder()
                            .AddArgument("action", "viewConversation")
                            .AddArgument("conversationId", 9813)
                            .AddText("点鸣魔方")
                            .AddText("点鸣魔方正在更新，请稍后")
                            .Show();
                ProcessStartInfo psi = new ProcessStartInfo(Path.Combine(path, "NameCubeUpdata.exe"))
                {
                    Verb = "runas",
                    UseShellExecute = true
                };
                try
                {
                    Process.Start(psi);
                    this.Dispatcher.Invoke(() =>
                    {
                        Application.Current.Shutdown();
                    });
                }
                catch
                {
                    MessageBoxFunction.ShowMessageBoxError("需要管理员权限才能继续安装！");
                    this.Dispatcher.Invoke(() =>
                    {
                        NowProgressBar.IsIndeterminate = false;
                        CheckButton.IsEnabled = true;
                        UpdataFromComputerButton.IsEnabled = true;
                        CaseText.Text = "准备安装程序失败";
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError(ex.Message);
                this.Dispatcher.Invoke(() =>
                {
                    NowProgressBar.IsIndeterminate = false;
                    CheckButton.IsEnabled = true;
                    UpdataFromComputerButton.IsEnabled = true;
                    CaseText.Text = "准备安装程序失败";
                });
            }
        }

        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            CaseText.Text = "获取最新版本中...";
            NowProgressBar.IsIndeterminate = true;
            CheckButton.IsEnabled = false;
            UpdataFromComputerButton.IsEnabled = false;
            string GetVersion = "";
            try
            {
                GetVersion = await GithubData.GetLatestReleaseVersionAsync("haaa4", "NameCube");
                GlobalVariables.json.AllSettings.UpdataTime = DateTime.Now.ToString("s");
                if(GetVersion!=GlobalVariables.Version)
                {
                    NowProgressBar.IsIndeterminate = false;
                    NowProgressBar.Value = 0;
                    CaseText.Text = "检测到新的版本：" + GetVersion + "。5秒后将开始更新";
                    await Task.Run(() => UpdataFromGithubAsync(GetVersion));
                }
                else
                {
                    NowProgressBar.IsIndeterminate = false;
                    CheckButton.IsEnabled = true;
                    UpdataFromComputerButton.IsEnabled = true;
                    CaseText.Text = "已是最新版本";
                    CheckText.Text = "上次检查时间:" + GlobalVariables.json.AllSettings.UpdataTime;
                }
            }
            catch (Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError(ex.Message);
                NowProgressBar.IsIndeterminate = false;
                CheckButton.IsEnabled = true;
                UpdataFromComputerButton.IsEnabled = true;
                CaseText.Text = "检查更新失败";
            }
        }
        public async Task UpdataFromGithubAsync(string Getversion)
        {
            for (int i = 1; i <= 5; i++)
            {
                Thread.Sleep(1000);
                this.Dispatcher.Invoke(() =>
                {
                    NowProgressBar.Value += 20;
                });
            }
            this.Dispatcher.Invoke(() =>
            {
                NowProgressBar.IsIndeterminate = true;
                CaseText.Text = "清理过往资源";
                NowProgressBar.Value = 0;
            });
            try
            {
                if (File.Exists(Path.Combine(GlobalVariables.configDir, "UpdataZip.zip")))
                {
                    File.Delete(Path.Combine(GlobalVariables.configDir, "UpdataZip.zip"));
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(() =>
                {
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    NowProgressBar.IsIndeterminate = false;
                    CheckButton.IsEnabled = true;
                    UpdataFromComputerButton.IsEnabled = true;
                    CaseText.Text = "清理过往资源失败";
                });

            }
            this.Dispatcher.Invoke(() =>
            {
                NowProgressBar.IsIndeterminate = false;
                CaseText.Text = "下载安装资源";
                NowProgressBar.Value = 0;
            });
            try
            {
                string url = $"https://github.com/haaa4/NameCube/releases/download/{Getversion}/NameCube.{Getversion}.Setup.x64.zip";
                string filename = Path.Combine(GlobalVariables.configDir, "UpdataZip.zip");
                await DownloadFileAsync(url, filename);
            }
            catch(Exception ex)
            {

                this.Dispatcher.Invoke(() =>
                {
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    NowProgressBar.IsIndeterminate = false;
                    CheckButton.IsEnabled = true;
                    UpdataFromComputerButton.IsEnabled = true;
                    CaseText.Text = "下载失败";
                });
            }
            await Task.Run(() => UpdataFromThisComputer(Path.Combine(GlobalVariables.configDir, "UpdataZip.zip"), GlobalVariables.configDir));
        }
        private async Task DownloadFileAsync(string url, string fileName)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = File.Create(fileName))
                    {
                        var buffer = new byte[8192];
                        int bytesRead;
                        long totalRead = 0;
                        var totalBytes = response.Content.Headers.ContentLength ?? -1;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalRead += bytesRead;

                            if (totalBytes > 0)
                            {
                                var progress = (int)((double)totalRead / totalBytes * 100);
                                this.Dispatcher.Invoke(() =>
                                {
                                    NowProgressBar.Value = progress;
                                });

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdataWayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Canchange)
            {
                GlobalVariables.json.AllSettings.UpdataGet = UpdataWayComboBox.SelectedIndex;
                GlobalVariables.SaveJson();
            }
        }

        private void AutoStart_Click(object sender, RoutedEventArgs e)
        {
            if(Canchange)
            {
                GlobalVariables.json.StartToDo.AutoUpdata = AutoStart.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }
    }
}
