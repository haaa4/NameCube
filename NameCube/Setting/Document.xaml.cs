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

namespace NameCube.Setting
{
    /// <summary>
    /// Document.xaml 的交互逻辑
    /// </summary>
    public partial class Document : Page
    {
        public Document()
        {
            InitializeComponent();
            canChange = false;
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Namecube");
            if (GlobalVariables.configDir == path)
            {
                ModeCombox.SelectedIndex = 0;
            }
            else
            {
                ModeCombox.SelectedIndex = 1;
            }
            canChange = true;
        }
        bool canChange = true;
        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(GlobalVariables.configDir);
        }

        private void CardAction_Click_1(object sender, RoutedEventArgs e)
        {
            SnackBarFunction.ShowSnackBarInSettingWindow("正在删除，请稍等", ControlAppearance.Primary,null,1);
            Task.Run(() =>
            {
                try
                {
                    if (Directory.Exists(System.IO.Path.Combine(GlobalVariables.configDir, "logs")))
                        Directory.Delete(System.IO.Path.Combine(GlobalVariables.configDir, "logs"), true);
                    if (Directory.Exists(System.IO.Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups")))
                        Directory.Delete(System.IO.Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups"), true);
                    if (Directory.Exists(System.IO.Path.Combine(GlobalVariables.configDir, "Updata")))
                        Directory.Delete(System.IO.Path.Combine(GlobalVariables.configDir, "Updata"), true);
                    MessageBoxFunction.ShowMessageBoxInfo("删除成功");
                }
                catch (Exception ex)
                {
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                }
            });



        }

        private void CardAction_Click_2(object sender, RoutedEventArgs e)
        {
            SnackBarFunction.ShowSnackBarInSettingWindow("正在删除，请稍等", ControlAppearance.Primary, null, 1);
            Task.Run(() =>
            {
                try
                {
                    Directory.Delete(GlobalVariables.configDir, true);
                    MessageBoxFunction.ShowMessageBoxInfo("删除成功，请自行启动软件");
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        System.Windows.Application.Current.Shutdown();
                    }));;
                }
                catch (Exception ex)
                {
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
                    saveFileDialog.Title = "保存配置文件"; // 对话框标题
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SnackBarFunction.ShowSnackBarInSettingWindow("正在保存，请稍等", ControlAppearance.Primary, null, 1);
                    Task.Run(() =>
                    {
                        try
                        {
                            var SevenZipCompressor = new SevenZipCompressor(null);
                            SevenZipCompressor.Zip(new List<string>() { GlobalVariables.configDir }, saveFileDialog.FileName);
                            MessageBoxFunction.ShowMessageBoxInfo("保存成功");
                        }
                        catch (Exception ex)
                        {
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
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            System.Windows.Application.Current.Shutdown();
                        })); ;
                    }
                    catch (Exception ex)
                    {
                        MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    }
                });
            }


        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canChange)
            {
                SnackBarFunction.ShowSnackBarInSettingWindow("正在保存，请稍等", ControlAppearance.Primary, null, 1);
                if (ModeCombox.SelectedIndex == 1) 
                {
                    string movePath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NameCube");
                    Task.Run(() =>
                    {
                        FolderMover.MoveFolder(GlobalVariables.configDir, movePath, true);
                        MessageBoxFunction.ShowMessageBoxInfo("移动成功");
                        GlobalVariables.configDir = movePath;
                    });
                }
                else
                {
                    string movePath= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Namecube");
                    Task.Run(() =>
                    {
                        FolderMover.MoveFolder(GlobalVariables.configDir, movePath, true);
                        MessageBoxFunction.ShowMessageBoxInfo("移动成功");
                        GlobalVariables.configDir = movePath;
                    });
                }
            }
        }
    }
}
