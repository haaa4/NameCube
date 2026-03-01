using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Application = System.Windows.Application;
using Path = System.IO.Path;
using Timer = System.Timers.Timer;

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
        }
        int updateMode = 1;
        private void button_Click(object sender, RoutedEventArgs e)
        {
            var StartStoryBoard = FindResource("QuickInstallationStart") as Storyboard;
            updateMode = 1;
            button.Visibility = Visibility.Collapsed;
            button1.Visibility = Visibility.Collapsed;
            button2.Visibility = Visibility.Visible;
            button3.Visibility = Visibility.Visible;
            button4.Visibility = Visibility.Visible;
            textBlock2.Visibility = Visibility.Visible;
            textBox.Visibility = Visibility.Visible;
            textBlock2.Text = "选择安装路径(路径不存在将被创建）";
            button3.IsEnabled = false;
            textBox.Text = "";
            StartStoryBoard.Begin();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            //根据更新模式选择不同的返回动画
            var BackStoryBoard = new Storyboard();
            if (updateMode == 1)
            {
                BackStoryBoard = FindResource("QuickInstallationBack") as Storyboard;
            }
            else
            {
                BackStoryBoard = FindResource("ManualUpdateBack") as Storyboard;
            }


            button4.IsEnabled = false;
            BackStoryBoard.Completed += (s, en) =>
            {
                button.Visibility = Visibility.Visible;
                button1.Visibility = Visibility.Visible;
                button2.Visibility = Visibility.Collapsed;
                button3.Visibility = Visibility.Collapsed;
                button4.Visibility = Visibility.Collapsed;
                textBlock2.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Collapsed;
                button4.IsEnabled = true;
            };
            BackStoryBoard.Begin();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //快速安装模式
            var StartStoryBoard = FindResource("ManualUpdateStart") as Storyboard;
            updateMode = 2;
            button.Visibility = Visibility.Collapsed;
            button1.Visibility = Visibility.Collapsed;
            button2.Visibility = Visibility.Visible;
            button3.Visibility = Visibility.Visible;
            button4.Visibility = Visibility.Visible;
            textBlock2.Visibility = Visibility.Visible;
            textBox.Visibility = Visibility.Visible;
            textBlock2.Text = "选择需更新的NameCube程序";
            textBox.Text = "";
            button3.IsEnabled = false;
            StartStoryBoard.Begin();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (updateMode == 1)
            {
                //选择安装路径（快速安装模式）
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
                {
                    Description = "请选择安装路径",
                };
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    button3.IsEnabled = true;
                    textBox.Text = folderBrowserDialog.SelectedPath;
                    //安装路径补全
                    if (textBox.Text.EndsWith("\\"))
                    {
                        textBox.Text += "NameCube";
                    }
                    else
                    {
                        textBox.Text += "\\NameCube";
                    }
                }

            }
            else
            {
                //选择需更新的NameCube程序（手动更新模式）
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "NameCube程序 (点鸣魔方.exe)|点鸣魔方.exe",
                    Title = "请选择需更新的NameCube程序"
                };
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    button3.IsEnabled = true;
                    textBox.Text = openFileDialog.FileName;
                }
            }
        }
        /// <summary>
        /// 提取软件包
        /// </summary>
        /// <param name="file">保存路径</param>
        /// <exception cref="Exception">未找到相应资源</exception>
        private void ExtractPackage(string file)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "NameCubeSetup.Packet.Packet.zip";
            //此段用于调试资源名称，正式发布时可删除
            //string[] get =System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new Exception("资源未找到");
                using (FileStream fileStream = new FileStream(file + "\\NameCube.zip", FileMode.Create))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }
        /// <summary>
        /// 获取指定路径的父目录路径。
        /// </summary>
        /// <param name="path">文件或目录路径。</param>
        /// <returns>父目录路径；如果路径为 null、空字符串，或者没有父目录（如根目录），则返回 null。</returns>
        /// <exception cref="ArgumentException">当路径包含一个或多个无效字符时抛出。</exception>
        public static string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            string parent = Path.GetDirectoryName(path);

            // Path.GetDirectoryName 对于根目录返回 null，对于顶级相对路径（如 "folder"）返回空字符串
            return string.IsNullOrEmpty(parent) ? null : parent;
        }
        /// <summary>
        /// 异步解压ZIP文件，并更新进度条。
        /// </summary>
        /// <param name="zipPath">ZIP文件路径</param>
        /// <param name="extractPath">解压目标文件夹路径</param>
        public async Task UnzipWithProgressAsync(string zipPath, string extractPath)
        {
            // 确保目标文件夹存在
            Directory.CreateDirectory(extractPath);

            // 创建进度报告器，自动将回调封送到UI线程
            var progress = new Progress<double>(percent =>
            {
                // 假设progressBar.Value为int类型（0-100），WPF中可能是double（0-100）
                progressBar.Value = (int)(percent * 100);
            });

            // 在后台线程执行解压操作
            await Task.Run(() => ExtractZip(zipPath, extractPath, progress));
        }

        /// <summary>
        /// 实际解压逻辑（在后台线程执行）
        /// </summary>
        private void ExtractZip(string zipPath, string extractPath, IProgress<double> progress)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                long totalBytes = 0;
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    totalBytes += entry.Length;
                }

                if (totalBytes == 0)
                {
                    progress.Report(1.0);
                    return;
                }

                long extractedBytes = 0;
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string destinationPath = Path.Combine(extractPath, entry.FullName);

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }

                    string directory = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(directory))
                        Directory.CreateDirectory(directory);

                    // 修复：手动实现 ExtractToFile 逻辑，兼容 .NET Framework 4.5 及更早版本
                    using (var entryStream = entry.Open())
                    using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        entryStream.CopyTo(fileStream);
                    }

                    extractedBytes += entry.Length;
                    double percent = (double)extractedBytes / totalBytes;
                    progress.Report(percent);
                }
            }
        }
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            var startStoryBoard = FindResource("ShowProcessRing") as Storyboard;
            progressBar.Visibility = Visibility.Visible;
            textBlock3.Visibility = Visibility.Visible;
            button4.IsEnabled = false;
            startStoryBoard.Completed += (s, en) =>
            {
                textBox.Visibility = Visibility.Collapsed;
                textBlock2.Visibility = Visibility.Collapsed;
                button2.Visibility = Visibility.Collapsed;
                button3.Visibility = Visibility.Collapsed;

                try
                {
                    string file = textBox.Text;
                    if(updateMode==2)
                    {
                        //手动更新模式需要删除原程序
                        File.Delete(file);
                        file = GetParentPath(file);
                        if(File.Exists(file+ "\\NameCubeUpdata.exe"))
                        {
                            File.Delete(file + "\\NameCubeUpdata.exe");
                        }
                    }
                    Directory.CreateDirectory(file);
                    ExtractPackage(file);
                    ExtractZip(file + "\\NameCube.zip", file, new Progress<double>(percent =>
                    {
                        progressBar.Value = (int)(percent * 100);
                    }));
                    File.Delete(file + "\\NameCube.zip");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("安装过程中发生错误: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
                textBlock4.Visibility = Visibility.Visible;
                checkBox.Visibility = Visibility.Visible;
                button5.Visibility = Visibility.Visible;
                var ShowFinishStoryBoard = FindResource("ShowFinish") as Storyboard;
                ShowFinishStoryBoard.Completed += (ss, een) =>
                {
                    progressBar.Visibility = Visibility.Collapsed;
                    textBlock3.Visibility = Visibility.Collapsed;
                };
                ShowFinishStoryBoard.Begin();

            };
            if(updateMode==2)
            {
                //手动更新模式与快速安装模式不同的提示语
                textBlock3.Text = "正在更新中";
                textBlock4.Text = "更新完成";
            }
            startStoryBoard.Begin();

        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                string file= textBox.Text;
                if (updateMode==2)
                {
                    file=GetParentPath(file);
                }
                //运行点鸣魔方
                Process.Start(file + "\\点鸣魔方.exe");
            }
            Application.Current.Shutdown();
        }
    }
}
