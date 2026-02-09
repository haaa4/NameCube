using Masuit.Tools.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NameCube.Function;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;
using Path = System.IO.Path;
using Serilog; // 添加Serilog引用
using Application = System.Windows.Application;

namespace NameCube.Setting
{
    /// <summary>
    /// Document.xaml 的交互逻辑
    /// </summary>
    public partial class Document : Page
    {
        private static readonly ILogger _logger = Log.ForContext<Document>(); // 添加Serilog日志实例

        public Document()
        {
            InitializeComponent();
            _logger.Debug("Document 页面初始化开始");
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Namecube");

        }


        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("打开配置目录: {ConfigDir}", GlobalVariablesData.configDir);
            Process.Start(GlobalVariablesData.configDir);
        }

        private void CardAction_Click_1(object sender, RoutedEventArgs e)
        {
            _logger.Information("开始清理临时文件");
            SnackBarFunction.ShowSnackBarInSettingWindow("正在删除，请稍等", ControlAppearance.Primary, null, 1);
            Task.Run(() =>
            {
                try
                {
                    if (Directory.Exists(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "logs")))
                    {
                        _logger.Debug("删除日志目录");
                        Directory.Delete(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "logs"), true);
                    }

                    if (Directory.Exists(System.IO.Path.Combine(GlobalVariablesData.userDataDir, "Mode_data", "MemoryFactoryMode", "Backups")))
                    {
                        _logger.Debug("删除备份目录");
                        Directory.Delete(System.IO.Path.Combine(GlobalVariablesData.userDataDir, "Mode_data", "MemoryFactoryMode", "Backups"), true);
                    }

                    if (Directory.Exists(System.IO.Path.Combine(GlobalVariablesData.userDataDir, "Updata")))
                    {
                        _logger.Debug("删除更新目录");
                        Directory.Delete(System.IO.Path.Combine(GlobalVariablesData.userDataDir, "Updata"), true);
                    }

                    MessageBoxFunction.ShowMessageBoxInfo("删除成功");
                    _logger.Information("临时文件清理成功");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "清理临时文件时发生异常");
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                }
            });
        }

        private void CardAction_Click_2(object sender, RoutedEventArgs e)
        {
            _logger.Warning("用户请求删除整个配置目录: {ConfigDir}", GlobalVariablesData.configDir);
            SnackBarFunction.ShowSnackBarInSettingWindow("正在删除，请稍等", ControlAppearance.Primary, null, 1);
            Task.Run(() =>
            {
                try
                {
                    Directory.Delete(GlobalVariablesData.configDir, true);
                    Directory.Delete(GlobalVariablesData.userDataDir, true);
                    MessageBoxFunction.ShowMessageBoxInfo("删除成功，请自行启动软件");
                    _logger.Information("配置目录删除成功，程序即将退出");

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        _logger.Information("程序退出");
                        Application.Current.Shutdown();
                    }));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "删除配置目录时发生异常");
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                }
            });
        }

        private void CardAction_Click_3(object sender, RoutedEventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = "c:\\";
                saveFileDialog.Filter = "数据备份包(*.dataBackup)|*.dataBackup|所有文件 (*.*)|*.*";
                saveFileDialog.Title = "保存备份文件";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _logger.Information("开始备份配置文件到: {FilePath}", saveFileDialog.FileName);
                    SnackBarFunction.ShowSnackBarInSettingWindow("正在保存，请稍等", ControlAppearance.Primary, null, 1);

                    Task.Run(() =>
                    {
                        try
                        {
                            var SevenZipCompressor = new SevenZipCompressor(null);
                            File.Copy(Path.Combine(GlobalVariablesData.configDir, "config.json"), Path.Combine(GlobalVariablesData.userDataDir, "config_backup.json"),true);
                            SevenZipCompressor.Zip(new List<string>() { GlobalVariablesData.userDataDir }, saveFileDialog.FileName);
                            File.Delete(Path.Combine(GlobalVariablesData.userDataDir, "config_backup.json"));
                            MessageBoxFunction.ShowMessageBoxInfo("保存成功");
                            _logger.Information("配置文件备份成功: {FilePath}", saveFileDialog.FileName);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "备份配置文件时发生异常");
                            MessageBoxFunction.ShowMessageBoxError("保存失败\n" + ex.Message);
                        }
                    });
                }
            }
        }

        private void CardAction_Click_4(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "从数据备份包导入";
            openFileDialog.Filter = "数据备份包(*.dataBackup)|*.dataBackup|压缩包（<v1.3)(*.zip)|*.zip|所有文件 (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _logger.Information("开始从压缩包恢复配置: {FilePath}", openFileDialog.FileName);
                SnackBarFunction.ShowSnackBarInSettingWindow("正在保存，请稍等", ControlAppearance.Primary, null, 1);

                Task.Run(() =>
                {
                    try
                    {
                        Directory.Delete(GlobalVariablesData.userDataDir, true);
                        Directory.CreateDirectory(GlobalVariablesData.userDataDir);
                        var SevenZipCompressor = new SevenZipCompressor(null);
                        SevenZipCompressor.Decompress(openFileDialog.FileName, GlobalVariablesData.userDataDir);
                        File.Copy(Path.Combine(GlobalVariablesData.userDataDir, "config_backup.json"), Path.Combine(GlobalVariablesData.configDir, "config.json"), true);
                        File.Delete(Path.Combine(GlobalVariablesData.userDataDir, "config_backup.json"));
                        MessageBoxFunction.ShowMessageBoxInfo("覆盖成功，请自行启动软件");
                        _logger.Information("配置恢复成功，程序即将重启");

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            _logger.Information("程序退出");
                            Application.Current.Shutdown();
                        }));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "从压缩包恢复配置时发生异常");
                        MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    }
                });
            }
        }

        private void CardAction_Click_5(object sender, RoutedEventArgs e)
        {
            _logger.Information("打开数据目录: {ConfigDir}", GlobalVariablesData.userDataDir);
            Process.Start(GlobalVariablesData.userDataDir);
        }
    }
}