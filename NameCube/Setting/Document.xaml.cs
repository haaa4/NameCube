using Masuit.Tools.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

            canChange = false;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Namecube");
            if (GlobalVariables.configDir == path)
            {
                ModeCombox.SelectedIndex = 0;
                _logger.Information("配置目录为应用程序目录: {ConfigDir}", GlobalVariables.configDir);
            }
            else
            {
                ModeCombox.SelectedIndex = 1;
                _logger.Information("配置目录为AppData目录: {ConfigDir}", GlobalVariables.configDir);
            }
            canChange = true;
        }

        bool canChange = true;

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("打开配置目录: {ConfigDir}", GlobalVariables.configDir);
            Process.Start(GlobalVariables.configDir);
        }

        private void CardAction_Click_1(object sender, RoutedEventArgs e)
        {
            _logger.Information("开始清理临时文件");
            SnackBarFunction.ShowSnackBarInSettingWindow("正在删除，请稍等", ControlAppearance.Primary, null, 1);
            Task.Run(() =>
            {
                try
                {
                    if (Directory.Exists(System.IO.Path.Combine(GlobalVariables.configDir, "logs")))
                    {
                        _logger.Debug("删除日志目录");
                        Directory.Delete(System.IO.Path.Combine(GlobalVariables.configDir, "logs"), true);
                    }

                    if (Directory.Exists(System.IO.Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups")))
                    {
                        _logger.Debug("删除备份目录");
                        Directory.Delete(System.IO.Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups"), true);
                    }

                    if (Directory.Exists(System.IO.Path.Combine(GlobalVariables.configDir, "Updata")))
                    {
                        _logger.Debug("删除更新目录");
                        Directory.Delete(System.IO.Path.Combine(GlobalVariables.configDir, "Updata"), true);
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
            _logger.Warning("用户请求删除整个配置目录: {ConfigDir}", GlobalVariables.configDir);
            SnackBarFunction.ShowSnackBarInSettingWindow("正在删除，请稍等", ControlAppearance.Primary, null, 1);
            Task.Run(() =>
            {
                try
                {
                    Directory.Delete(GlobalVariables.configDir, true);
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
                saveFileDialog.Filter = "压缩包(*.zip)|*.zip|所有文件 (*.*)|*.*";
                saveFileDialog.Title = "保存配置文件";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _logger.Information("开始备份配置文件到: {FilePath}", saveFileDialog.FileName);
                    SnackBarFunction.ShowSnackBarInSettingWindow("正在保存，请稍等", ControlAppearance.Primary, null, 1);

                    Task.Run(() =>
                    {
                        try
                        {
                            var SevenZipCompressor = new SevenZipCompressor(null);
                            SevenZipCompressor.Zip(new List<string>() { GlobalVariables.configDir }, saveFileDialog.FileName);
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
            openFileDialog.Title = "从压缩包导入";
            openFileDialog.Filter = "压缩包 (*.zip)|*.zip|所有文件 (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _logger.Information("开始从压缩包恢复配置: {FilePath}", openFileDialog.FileName);
                SnackBarFunction.ShowSnackBarInSettingWindow("正在保存，请稍等", ControlAppearance.Primary, null, 1);

                Task.Run(() =>
                {
                    try
                    {
                        Directory.Delete(GlobalVariables.configDir, true);
                        Directory.CreateDirectory(GlobalVariables.configDir);

                        var SevenZipCompressor = new SevenZipCompressor(null);
                        SevenZipCompressor.Decompress(openFileDialog.FileName, GlobalVariables.configDir);

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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canChange)
            {
                _logger.Information("更改配置目录存储模式为: {Mode}", ModeCombox.SelectedIndex);
                SnackBarFunction.ShowSnackBarInSettingWindow("正在保存，请稍等", ControlAppearance.Primary, null, 1);

                if (ModeCombox.SelectedIndex == 1)
                {
                    string movePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NameCube");
                    _logger.Information("移动配置目录从 {OldPath} 到 {NewPath}", GlobalVariables.configDir, movePath);

                    Task.Run(() =>
                    {
                        try
                        {
                            FolderMover.MoveFolder(GlobalVariables.configDir, movePath, true);
                            MessageBoxFunction.ShowMessageBoxInfo("移动成功");
                            GlobalVariables.configDir = movePath;
                            _logger.Information("配置目录移动完成");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "移动配置目录时发生异常");
                        }
                    });
                }
                else
                {
                    string movePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Namecube");
                    _logger.Information("移动配置目录从 {OldPath} 到 {NewPath}", GlobalVariables.configDir, movePath);

                    Task.Run(() =>
                    {
                        try
                        {
                            FolderMover.MoveFolder(GlobalVariables.configDir, movePath, true);
                            MessageBoxFunction.ShowMessageBoxInfo("移动成功");
                            GlobalVariables.configDir = movePath;
                            _logger.Information("配置目录移动完成");
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "移动配置目录时发生异常");
                        }
                    });
                }
            }
        }
    }
}