using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Path = System.IO.Path;

namespace NameCubeSetup
{
    /// <summary>
    /// UpdateGuideWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateGuideWindow : Window
    {
        public UpdateGuideWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!isError)
            {
                MessageBox.Show("请勿关闭此窗口，安装程序正在运行中！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                e.Cancel = true;
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

        /// <summary>
        /// 判断指定名称的进程是否正在运行。
        ///  注意：processName 通常不需要包含 ".exe" 后缀
        /// </summary>
        /// <param name="processName">进程名</param>
        /// <returns></returns>
        public static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        private bool isError = false;

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Text3rd.Foreground = Brushes.Blue;
            //第三步，判断进程是否关闭
            try
            {
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "START"));
                bool isRunning = true;
                for (int i = 1; i <= 60; i++)
                {
                    if (!IsProcessRunning("点鸣魔方"))
                    {
                        isRunning = false;
                        break;
                    }
                    await Task.Delay(1000);
                }
                if (isRunning)
                {
                    throw new Exception("点鸣魔方进程未关闭，请关闭后重试！");
                }
            }
            catch (Exception ex)
            {
                ErrorAttention.Text = "发生错误：" + ex.Message;
                Text3rd.Foreground = Brushes.Red;
                MessageBox.Show("发生错误：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Text3rd.Foreground = Brushes.Green;
            Image3rd.Source = FinishExampleImage.Source;
            //第四步，解压文件
            //先解压文件到统一目录，再将解压后的文件移动到目标目录，防止原应用损坏
            try
            {
                string tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TempExtract");
                Directory.CreateDirectory(tempDir);
                ExtractPackage(tempDir);
                await UnzipWithProgressAsync(Path.Combine(tempDir, "NameCube.zip"), Path.Combine(tempDir, "Temp"));
                tempDir = Path.Combine(tempDir, "Temp");
                Directory.CreateDirectory(tempDir);
                string targetDir = GetParentPath(GetParentPath(AppDomain.CurrentDomain.BaseDirectory));
                if (targetDir == null)
                    throw new Exception("无法获取目标目录");
                foreach (string dir in Directory.GetDirectories(tempDir))
                {
                    string destDir = Path.Combine(targetDir, Path.GetFileName(dir));
                    if (Directory.Exists(destDir))
                        Directory.Delete(destDir, true);
                    Directory.Move(dir, destDir);
                }
                foreach (string file in Directory.GetFiles(tempDir))
                {
                    string destFile = Path.Combine(targetDir, Path.GetFileName(file));
                    if (File.Exists(destFile))
                        File.Delete(destFile);
                    File.Move(file, destFile);
                }
                Directory.Delete(tempDir, true);
            }
            catch (Exception ex)
            {
                isError = true;
                ErrorAttention.Text = "发生错误：" + ex.Message;
                Text4th.Foreground = Brushes.Red;
                MessageBox.Show("发生错误：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Text4th.Foreground = Brushes.Green;
            Image4th.Source = FinishExampleImage.Source;
            //通知更新程序完成，删除标记文件，更新A端会监视此文件并执行后续操作
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FINISH"), "");
            Process.Start(Path.Combine(GetParentPath(GetParentPath(AppDomain.CurrentDomain.BaseDirectory)), "点鸣魔方.exe"));
            //防止被阻止关闭窗口
            isError = true;
            //程序退出
            Application.Current.Shutdown();
        }
    }
}