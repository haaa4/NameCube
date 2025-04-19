using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

namespace NameUpdate
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
             Updata();

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
        }
        static async Task Updata()
        {
            try
            {
                Thread.Sleep(2000);
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string updatePath = System.IO.      Path.Combine(appPath, "Updata");

                // 步骤1：清理旧文件
                CleanDirectory(appPath);

                // 步骤2：复制更新文件
                if (Directory.Exists(updatePath))
                {
                    CopyUpdateFiles(updatePath, appPath);

                    // 步骤3：删除更新文件夹
                    Directory.Delete(updatePath, true);
                }

                MessageBox.Show("更新成功完成！");
                Process.Start("NameCube.exe");
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新过程中发生错误: {ex.Message}");
                Application.Current.Shutdown();
            }

        }
        // 安全复制文件（含子目录）
        static void CleanDirectory(string targetPath)
        {
            // 保留文件白名单
            string[] protectedFiles = { "NameUpdate.exe", "NameUpdate.exe.config" };

            // 删除非保护文件
            foreach (string file in Directory.GetFiles(targetPath))
            {
                string fileName = System.IO.Path.GetFileName(file);
                if (!protectedFiles.Contains(fileName))
                {
                    File.Delete(file);
                }
            }

            // 删除非更新目录
            foreach (string dir in Directory.GetDirectories(targetPath))
            {
                string dirName = System.IO.Path.GetFileName(dir);
                if (!dirName.Equals("Updata", StringComparison.OrdinalIgnoreCase))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        static void CopyUpdateFiles(string sourcePath, string targetPath)
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                string relativePath = dirPath.Substring(sourcePath.Length).TrimStart(System.IO.Path.DirectorySeparatorChar);
                string destDir = System.IO.Path.Combine(targetPath, relativePath);
                Directory.CreateDirectory(destDir);
            }

            foreach (string filePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                string relativePath = filePath.Substring(sourcePath.Length).TrimStart(System.IO.Path.DirectorySeparatorChar);
                string destFile = System.IO.Path.Combine(targetPath, relativePath);
                File.Copy(filePath, destFile, true);
            }
        }
    }
    }
