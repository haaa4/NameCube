using Masuit.Tools.Files;
using Masuit.Tools.Logging;
using Microsoft.Win32;
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
            VersionText.Text=GlobalVariables.json.Version;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Ring.Visibility = Visibility.Visible;
            ComputerButton.IsEnabled = false;
            NowText.Text = "解压缩包中......";
            if(await UpdataFromThisComputer()==1)
            {
                NowText.Text = "安装中断...";
                ComputerButton.IsEnabled = true;
                Ring.Visibility = Visibility.Hidden;
            }
            NowText.Text = "解压缩完成";
            ComputerButton.IsEnabled = true;
            Ring.Visibility = Visibility.Hidden;
            Application.Current.Shutdown();
            Process.Start("NameUpdate.exe");
        }
        static async Task<int> UpdataFromThisComputer()
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
                    return 0;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    LogManager.Error(ex);
                    return 1;
                }
            }
            return 1;
        }
    }
}
