using Masuit.Tools.Files;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Contexts;
using System.Text;
using NameCube.Function;
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
using Serilog; // 添加Serilog引用

namespace NameCube.Setting
{
    /// <summary>
    /// Updata.xaml 的交互逻辑
    /// </summary>
    public partial class Updata : Page
    {
        private static readonly ILogger _logger = Log.ForContext<Updata>(); // 添加Serilog日志实例

        public Updata()
        {
            InitializeComponent();
            _logger.Debug("Updata 页面初始化开始");
            GlobalVariablesData.config.StartToDo.AutoUpdata = false;
            GlobalVariablesData.SaveConfig();
            Canchange = false;
            VersionText.Text = GlobalVariablesData.VERSION;
            UpdataWayComboBox.SelectedIndex = GlobalVariablesData.config.AllSettings.UpdataGet;
            AutoStart.IsChecked = GlobalVariablesData.config.StartToDo.AutoUpdata;
            tokenText.Text = GlobalVariablesData.config.AllSettings.token;
            Canchange = true;

            if (GlobalVariablesData.config.AllSettings.UpdataTime != null)
            {
                CheckText.Text = "上次检查时间:" + GlobalVariablesData.config.AllSettings.UpdataTime;
            }
            else
            {
                CheckText.Text = "上次检查时间:从未检查过";
            }

            if (GlobalVariablesData.ISBETA)
            {
                WarningInfo.IsOpen = true;
                _logger.Warning("当前运行的是Beta版本");
            }

            if (GlobalVariablesData.config.AllSettings.newVersion != null)
            {
                CaseText.Text = "检测到新的版本：" + GlobalVariablesData.config.AllSettings.newVersion;
                UpkButton.IsEnabled = true;
                _logger.Information("检测到新版本: {NewVersion}", GlobalVariablesData.config.AllSettings.newVersion);
            }

            _logger.Information("更新设置加载完成，当前版本: {Version}, 自动更新: {AutoUpdate}, 更新方式: {UpdateWay}",
                GlobalVariablesData.VERSION,
                GlobalVariablesData.config.StartToDo.AutoUpdata,
                GlobalVariablesData.config.AllSettings.UpdataGet);
        }

        bool Canchange;

        private void UpdataFromComputerButton_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("用户选择从本地安装更新");
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "从本地安装";
            openFileDialog.Filter = "安装包 (*.zip)|*.zip|所有文件 (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                _logger.Information("选择本地安装包: {FilePath}", openFileDialog.FileName);
                Task.Run(() => UpdataFromThisComputer(openFileDialog.FileName, GlobalVariablesData.configDir));
                CaseText.Text = "解压缩包中...";
                NowProgressBar.IsIndeterminate = true;
                UpkButton.IsEnabled = false;
                CheckButton.IsEnabled = false;
                UpdataFromComputerButton.IsEnabled = false;
            }
        }

        public void UpdataFromThisComputer(string file, string UpdataFile)
        {
            _logger.Information("开始从本地文件更新: {File}", file);

            try
            {
                string path = Path.Combine(UpdataFile, "Updata", "Data");
                if (Directory.Exists(Path.Combine(UpdataFile, "Updata")))
                {
                    _logger.Debug("删除旧的更新目录");
                    Directory.Delete(Path.Combine(UpdataFile, "Updata"), true);
                }

                Directory.CreateDirectory(path);
                var SevenZipCompressor = new SevenZipCompressor(null);
                SevenZipCompressor.Decompress(file, path);
                _logger.Debug("解压缩完成");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "解压缩更新包时发生异常");
                MessageBoxFunction.ShowMessageBoxError(ex.Message);

                this.Dispatcher.Invoke(() =>
                {
                    NowProgressBar.IsIndeterminate = false;
                    CheckButton.IsEnabled = true;
                    UpkButton.IsEnabled = true;
                    UpdataFromComputerButton.IsEnabled = true;
                    CaseText.Text = "解压缩失败";
                });
                return;
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

                SnackBarFunction.ShowSnackBarInSettingWindow("更新程序即将启动，请勿关闭程序,否则可能会导致文件损坏！", Wpf.Ui.Controls.ControlAppearance.Caution);
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
                    _logger.Information("启动更新程序");
                    Process.Start(psi);

                    this.Dispatcher.Invoke(() =>
                    {
                        _logger.Information("关闭主程序以进行更新");
                        Application.Current.Shutdown();
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "启动更新程序失败");
                    MessageBoxFunction.ShowMessageBoxError("需要管理员权限才能继续安装！");

                    this.Dispatcher.Invoke(() =>
                    {
                        NowProgressBar.IsIndeterminate = false;
                        CheckButton.IsEnabled = true;
                        UpkButton.IsEnabled = true;
                        UpdataFromComputerButton.IsEnabled = true;
                        CaseText.Text = "准备安装程序失败";
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "准备安装程序时发生异常");
                MessageBoxFunction.ShowMessageBoxError(ex.Message);

                this.Dispatcher.Invoke(() =>
                {
                    NowProgressBar.IsIndeterminate = false;
                    CheckButton.IsEnabled = true;
                    UpkButton.IsEnabled = true;
                    UpdataFromComputerButton.IsEnabled = true;
                    CaseText.Text = "准备安装程序失败";
                });
            }
        }

        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("开始检查更新");
            CaseText.Text = "获取最新版本中...";
            NowProgressBar.IsIndeterminate = true;
            CheckButton.IsEnabled = false;
            UpkButton.IsEnabled = false;
            UpdataFromComputerButton.IsEnabled = false;

            string GetVersion = "";
            try
            {
                if (GlobalVariablesData.config.AllSettings.token == "" || GlobalVariablesData.config.AllSettings.token == null)
                {
                    _logger.Debug("使用匿名方式检查更新");
                    GetVersion = await GithubData.GetLatestReleaseVersionAsync("haaa4", "NameCube");
                }
                else
                {
                    _logger.Debug("使用Token方式检查更新");
                    GetVersion = await GithubData.GetLatestReleaseVersionAsync("haaa4", "NameCube", GlobalVariablesData.config.AllSettings.token);
                }

                GlobalVariablesData.config.AllSettings.UpdataTime = DateTime.Now.ToString("g");

                if (GetVersion != GlobalVariablesData.VERSION)
                {
                    NowProgressBar.IsIndeterminate = false;
                    NowProgressBar.Value = NowProgressBar.Maximum;
                    CaseText.Text = "检测到新的版本：" + GetVersion;
                    GlobalVariablesData.config.AllSettings.newVersion = GetVersion;
                    GlobalVariablesData.SaveConfig();
                    UpkButton.IsEnabled = true;
                    CheckText.Text = "上次检查时间:" + GlobalVariablesData.config.AllSettings.UpdataTime;

                    _logger.Information("发现新版本: {NewVersion}, 当前版本: {CurrentVersion}", GetVersion, GlobalVariablesData.VERSION);
                }
                else
                {
                    GlobalVariablesData.config.AllSettings.newVersion = null;
                    GlobalVariablesData.SaveConfig();
                    CaseText.Text = "已是最新版本";
                    CheckText.Text = "上次检查时间:" + GlobalVariablesData.config.AllSettings.UpdataTime;
                    _logger.Information("已是最新版本: {CurrentVersion}", GlobalVariablesData.VERSION);
                }

                NowProgressBar.IsIndeterminate = false;
                CheckButton.IsEnabled = true;
                UpdataFromComputerButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "检查更新时发生异常");
                MessageBoxFunction.ShowMessageBoxError(ex.Message);
                NowProgressBar.IsIndeterminate = false;
                CheckButton.IsEnabled = true;
                UpdataFromComputerButton.IsEnabled = true;
                CaseText.Text = "检查更新失败";
            }
        }

        public async Task UpdataFromGithubAsync(string Getversion)
        {
            _logger.Information("开始从GitHub更新到版本: {Version}", Getversion);

            this.Dispatcher.Invoke(() =>
            {
                NowProgressBar.IsIndeterminate = true;
                CheckButton.IsEnabled = false;
                UpkButton.IsEnabled = false;
                UpdataFromComputerButton.IsEnabled = false;
            });

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
                if (File.Exists(Path.Combine(GlobalVariablesData.userDataDir, "UpdataZip.zip")))
                {
                    _logger.Debug("删除旧的更新文件");
                    File.Delete(Path.Combine(GlobalVariablesData.userDataDir, "UpdataZip.zip"));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "清理过往资源时发生异常");
                this.Dispatcher.Invoke(() =>
                {
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    NowProgressBar.IsIndeterminate = false;
                    CheckButton.IsEnabled = true;
                    UpkButton.IsEnabled = true;
                    UpdataFromComputerButton.IsEnabled = true;
                    CaseText.Text = "清理过往资源失败";
                });
                return;
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
                string filename = Path.Combine(GlobalVariablesData.userDataDir, "UpdataZip.zip");
                _logger.Information("开始下载更新文件: {Url}", url);
                await DownloadFileAsync(url, filename);
                _logger.Information("下载完成");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "下载更新文件时发生异常");
                this.Dispatcher.Invoke(() =>
                {
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    NowProgressBar.IsIndeterminate = false;
                    CheckButton.IsEnabled = true;
                    UpkButton.IsEnabled = true;
                    UpdataFromComputerButton.IsEnabled = true;
                    CaseText.Text = "下载失败";
                });
                return;
            }

            await Task.Run(() => UpdataFromThisComputer(Path.Combine(GlobalVariablesData.userDataDir, "UpdataZip.zip"), GlobalVariablesData.userDataDir));
        }

        private async Task DownloadFileAsync(string url, string fileName)
        {
            _logger.Debug("开始下载文件: {Url} 到 {FileName}", url, fileName);

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

                        _logger.Debug("文件下载完成，总大小: {TotalBytes} bytes", totalRead);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "下载文件时发生异常");
                throw ex;
            }
        }

        private void UpdataWayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Canchange)
            {
                GlobalVariablesData.config.AllSettings.UpdataGet = UpdataWayComboBox.SelectedIndex;
                GlobalVariablesData.SaveConfig();
                _logger.Information("更新方式修改为: {UpdateWay}", UpdataWayComboBox.SelectedIndex);
            }
        }

        private void AutoStart_Click(object sender, RoutedEventArgs e)
        {
            if (Canchange)
            {
                GlobalVariablesData.config.StartToDo.AutoUpdata = AutoStart.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                _logger.Information("自动更新修改为: {AutoUpdate}", AutoStart.IsChecked.Value);
            }
        }

        private void tokenText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Canchange)
            {
                GlobalVariablesData.config.AllSettings.token = tokenText.Text;
                GlobalVariablesData.SaveConfig();
                _logger.Debug("GitHub Token已更新");
            }
        }

        private async void UpkButton_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("用户点击更新按钮，开始更新到版本: {Version}", GlobalVariablesData.config.AllSettings.newVersion);
            await Task.Run(() => UpdataFromGithubAsync(GlobalVariablesData.config.AllSettings.newVersion));
        }


        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/haaa4/NameCube/releases");
        }
    }
}