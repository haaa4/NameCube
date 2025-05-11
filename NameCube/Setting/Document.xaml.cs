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
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(GlobalVariables.configDir);
        }

        private void CardAction_Click_1(object sender, RoutedEventArgs e)
        {

            try
            {
                if (File.Exists(System.IO.Path.Combine(GlobalVariables.configDir, "logs")))
                    Directory.Delete(System.IO.Path.Combine(GlobalVariables.configDir, "logs"), true);
                if (File.Exists(System.IO.Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups")))
                    Directory.Delete(System.IO.Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups"), true);
                MessageBoxFunction.ShowMessageBoxInfo("删除成功");
            }
            catch (Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError(ex.Message);
            }


        }

        private void CardAction_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.Delete(GlobalVariables.configDir, true);
                MessageBoxFunction.ShowMessageBoxInfo("删除成功，请自行启动软件");
                System.Windows.Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError(ex.Message);
            }


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
                    try
                    {
                        var SevenZipCompressor=new SevenZipCompressor(null);
                        SevenZipCompressor.Zip(new List<string>() { GlobalVariables.configDir }, saveFileDialog.FileName);
                        MessageBoxFunction.ShowMessageBoxInfo("保存成功");
                    }
                    catch (Exception ex)
                    {
                        MessageBoxFunction.ShowMessageBoxError("保存失败\n" + ex.Message);
                    }

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
                try
                {
                    Directory.Delete(GlobalVariables.configDir, true);
                    Directory.CreateDirectory(GlobalVariables.configDir);
                    var SevenZipCompressor = new SevenZipCompressor(null);
                    SevenZipCompressor.Decompress(openFileDialog.FileName, GlobalVariables.configDir);
                    MessageBoxFunction.ShowMessageBoxInfo("覆盖成功，请自行启动软件");
                    System.Windows.Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                }

            }


        }

    }
}
