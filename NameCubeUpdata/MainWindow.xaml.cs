using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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

namespace NameCubeUpdata
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Task.Run(() => Updata());
        }
        public void Updata()
        {
            try
            {
                Thread.Sleep(5000);
                string ProgramFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string UpdataFile = File.ReadAllText(Path.Combine(ProgramFile, "Path.txt"));
                File.Delete(Path.Combine(ProgramFile, "Path.txt"));

                try
                {
                    Directory.Delete(UpdataFile, true);
                }
                catch
                {
                    //这里不需要管
                }

                CopyDirectory(Path.Combine(ProgramFile, "Data"), UpdataFile);
                File.WriteAllText(Path.Combine(ProgramFile, "Success"), "Yhea!");
                MessageBox.Show("更新已完成，请自行开启程序", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            }

        }
        public static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite = true)
        {
            // 获取目录信息
            var dir = new DirectoryInfo(sourceDir);

            // 检查源目录是否存在
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"源目录不存在: {sourceDir}");
            }

            // 创建目标目录（如果不存在）
            Directory.CreateDirectory(destinationDir);

            // 复制文件
            foreach (FileInfo file in dir.GetFiles())
            {
                string destFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(destFilePath, overwrite);
            }

            // 递归复制子目录
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestDir, overwrite);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosed(e);
        }
    }
}
