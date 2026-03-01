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


        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("开始检查更新");
            CaseText.Text = "获取最新版本中...";
            NowProgressBar.IsIndeterminate = true;
            CheckButton.IsEnabled = false;
            UpkButton.IsEnabled = false;

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

                GlobalVariablesData.config.AllSettings.UpdataTime = DateTime.Now.ToString("f");

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

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "检查更新时发生异常");
                SnackBarFunction.ShowSnackBarInSettingWindow(ex.Message,Wpf.Ui.Controls.ControlAppearance.Caution);
                NowProgressBar.IsIndeterminate = false;
                CheckButton.IsEnabled = true;
                CaseText.Text = "检查更新失败";
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

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/haaa4/NameCube/releases");
        }

        private async Task CardAction_ClickAsync(object sender, RoutedEventArgs e)
        {
            _logger.Information("开始尝试强制更新");
            CaseText.Text = "获取最新版本中...";
            NowProgressBar.IsIndeterminate = true;
            CheckButton.IsEnabled = false;
            UpkButton.IsEnabled = false;

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

                GlobalVariablesData.config.AllSettings.UpdataTime = DateTime.Now.ToString("f");

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

            }
            catch (Exception ex)
            {
                _logger.Error(ex, "检查更新时发生异常");
                SnackBarFunction.ShowSnackBarInSettingWindow(ex.Message, Wpf.Ui.Controls.ControlAppearance.Caution);
                NowProgressBar.IsIndeterminate = false;
                CheckButton.IsEnabled = true;
                CaseText.Text = "检查更新失败";
            }
        }
    }
}