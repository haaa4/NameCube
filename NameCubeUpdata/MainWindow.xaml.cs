// /*
//  * NameCube-<点名器>
//  * Copyright (C) 2025-2026 haaa4
//  *
//  * This program is free software: you can redistribute it and/or modify
//  * it under the terms of the GNU General Public License as published by
//  * the Free Software Foundation, either version 3 of the License, or
//  * (at your option) any later version.
//  *
//  * This program is distributed in the hope that it will be useful,
//  * but WITHOUT ANY WARRANTY

using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using Application = System.Windows.Application;
using File = System.IO.File;
using Path = System.IO.Path;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using MessageBox = System.Windows.MessageBox;

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

        private int updateMode = 1;

        // ============ 架构信息 ============
        // 工程会用 /p:TargetArch=x86 再编译一份，产物名 NameCubeSetupX86.exe；
        // 装进去的 .NET 6 运行时位数必须和主程序一致，否则 32 位程序在 64 位系统上无法加载 64 位运行时。
#if X86
        private const string TargetArch = "x86";
#else
        private const string TargetArch = "x64";
#endif

        // ============ 环境检测相关常量 ============
        /// <summary>.NET 6 桌面运行时下载地址（微软官方 aka.ms 短链，始终指向最新的 6.0 补丁版本）</summary>
        private static readonly string Net6DownloadUrl =
            "https://aka.ms/dotnet/6.0/windowsdesktop-runtime-win-" + TargetArch + ".exe";

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

        // ================== 环境检测辅助方法 ==================

        /// <summary>
        /// 判断操作系统是否为 Windows 7 (6.1) 及以上
        /// </summary>
        private static bool IsWindows7OrLater()
        {
            Version v = Environment.OSVersion.Version;
            // Windows 7 = 6.1，Vista = 6.0，XP = 5.1
            return v.Major > 6 || (v.Major == 6 && v.Minor >= 1);
        }

        /// <summary>
        /// 检测 .NET 6 桌面运行时是否已安装（执行同位数 dotnet.exe --list-runtimes 判断）
        /// </summary>
        private static bool IsNet6RuntimeInstalled()
        {
            try
            {
                // 1. 定位与本安装包同一位数的 dotnet.exe
                //    64 位系统上 x64 运行时在 Program Files\dotnet，x86 运行时在 Program Files (x86)\dotnet，
                //    而 dotnet --list-runtimes 只列本 host 看得到的那些；拿错了就会把「已装」误判成「没装」。
                string dotnetExe = Path.Combine(
                    Environment.GetFolderPath(TargetArch == "x86"
                        ? Environment.SpecialFolder.ProgramFilesX86
                        : Environment.SpecialFolder.ProgramFiles),
                    "dotnet", "dotnet.exe");

                // 如果连 dotnet.exe 都没有，那肯定没装
                if (!File.Exists(dotnetExe)) return false;

                // 2. 执行 dotnet --list-runtimes 命令
                var psi = new ProcessStartInfo
                {
                    FileName = dotnetExe,
                    Arguments = "--list-runtimes",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    // 3. 在输出结果中查找 Microsoft.WindowsDesktop.App 6.
                    // 这个命令的输出最准确，不受注册表和文件目录结构变化的影响
                    return output.Contains("Microsoft.WindowsDesktop.App 6.");
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 下载并静默安装 .NET 6 桌面运行时，下载进度绑定到 progressBar
        /// </summary>
        /// <returns>是否安装成功</returns>
        private async Task<bool> DownloadAndInstallNet6Async()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"windowsdesktop-runtime-6.0-win-{TargetArch}.exe");

            try
            {
                // === 1. 隐藏背景中其他的控件（解决问题1） ===
                button2.Visibility = Visibility.Collapsed;
                textBox.Visibility = Visibility.Collapsed;
                textBlock2.Visibility = Visibility.Collapsed;
                button3.Visibility = Visibility.Collapsed;
                button4.Visibility = Visibility.Collapsed;

                // 显示进度条和提示文字
                progressBar.Visibility = Visibility.Visible;
                progressBar.Opacity = 1;
                progressBar.Value = 0;
                progressBar.IsIndeterminate = false; // 下载阶段使用确定进度
                textBlock3.Visibility = Visibility.Visible;
                textBlock3.Opacity = 1;
                textBlock3.Text = "正在下载 .NET 6 运行时...";
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                // ===== 下载（WebClient 会上报进度）=====
                using (var client = new WebClient())
                {
                    var tcs = new TaskCompletionSource<bool>();
                    client.DownloadProgressChanged += (s, ev) =>
                    {
                        // 进度绑定到进度条
                        progressBar.Value = ev.ProgressPercentage;
                    };
                    client.DownloadFileCompleted += (s, ev) =>
                    {
                        if (ev.Error != null) tcs.TrySetException(ev.Error);
                        else tcs.TrySetResult(true);
                    };
                    client.DownloadFileAsync(new Uri(Net6DownloadUrl), tempFile);
                    await tcs.Task;
                }

                // ===== 2. 安装阶段（解决问题2）=====
                // 切换提示文字
                textBlock3.Text = "正在安装 .NET 6 运行时...\n这可能需要几分钟，请耐心等待，切勿关闭程序";
                // 切换为跑马灯（不确定进度）状态
                progressBar.IsIndeterminate = true;

                var psi = new ProcessStartInfo
                {
                    FileName = tempFile,
                    Arguments = "/install /quiet /norestart",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    // 将同步等待放入后台线程，避免阻塞 UI 线程，从而让跑马灯动画正常播放
                    await Task.Run(() => process.WaitForExit());

                    // 0 = 成功，3010 = 成功但需要重启
                    return process.ExitCode == 0 || process.ExitCode == 3010;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"下载或安装 .NET 6 运行时失败：{ex.Message}\n请手动安装后重试。\n\n下载地址：https://dotnet.microsoft.com/download/dotnet/6.0", "安装失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                // 恢复 UI 状态，保证后续 ShowProcessRing 动画从初始状态开始
                progressBar.IsIndeterminate = false;
                progressBar.Value = 0;
                progressBar.Opacity = 0;
                progressBar.Visibility = Visibility.Collapsed;

                textBlock3.Text = "正在安装中...";
                textBlock3.Opacity = 0;
                textBlock3.Visibility = Visibility.Collapsed;

                // 删除临时安装包
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { }
                }
            }
        }

        // ================== 主安装按钮 ==================

        private async void button3_Click(object sender, RoutedEventArgs e)
        {
            // ========== 快速安装模式：先做环境检查 ==========
            if (updateMode == 1)
            {
                // 1) 检查操作系统版本（Win7 及以上）
                if (!IsWindows7OrLater())
                {
                    System.Windows.MessageBox.Show(
                        "您的操作系统版本过低，本程序需要 Windows 7 或更高版本才能运行。\n\n安装程序即将退出。",
                        "不兼容的操作系统",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }

#if !X86
                // 1.5) 64 位安装包不能装在 32 位系统上（32 位安装包可装在 64 位系统，走 WOW64）
                if (!Environment.Is64BitOperatingSystem)
                {
                    System.Windows.MessageBox.Show(
                        "当前系统是 32 位 Windows，无法安装 64 位版本。\n\n请下载并运行 32 位安装包（NameCubeSetupX86.exe）。\n\n安装程序即将退出。",
                        "架构不匹配",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                    return;
                }
#endif

                // 2) 检查 .NET 6 桌面运行时
                if (!IsNet6RuntimeInstalled())
                {
                    var choice = System.Windows.MessageBox.Show(
                        "检测到您的电脑未安装 .NET 6 运行时，应用程序需要它才能运行。\n\n是否现在自动从官网下载并安装？\n（如选择“否”，安装程序将退出）",
                        "缺少 .NET 6 运行时",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (choice != MessageBoxResult.Yes)
                    {
                        // 用户拒绝安装 → 退出安装程序
                        Application.Current.Shutdown();
                        return;
                    }

                    // 临时禁用按钮，防止重复触发
                    button3.IsEnabled = false;
                    bool installed = await DownloadAndInstallNet6Async();
                    button3.IsEnabled = true;

                    if (!installed)
                    {
                        System.Windows.MessageBox.Show(
                            "安装 .NET 6 运行时失败，请手动安装后重试。\n\n下载地址：https://dotnet.microsoft.com/download/dotnet/6.0",
                            "安装失败",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        Application.Current.Shutdown();
                        return;
                    }
                }
            }

            // ========== 以下为原有的安装流程，保持不变 ==========
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
                    if (updateMode == 2)
                    {
                        //手动更新模式需要删除原程序
                        file = GetParentPath(file);
                        string targetDir = file; // 假设 file 是目录路径

                        foreach (string entry in Directory.GetFileSystemEntries(targetDir))
                        {
                            string name = Path.GetFileName(entry);

                            // 跳过需要保留的 logs 和 user 文件夹
                            if (name == "logs" || name == "user")
                                continue;

                            // 删除文件
                            if (File.Exists(entry))
                            {
                                File.Delete(entry);
                            }
                            // 删除文件夹（递归删除内部所有内容）
                            else if (Directory.Exists(entry))
                            {
                                Directory.Delete(entry, true);
                            }
                        }
                    }
                    Directory.CreateDirectory(file);
                    ExtractPackage(file);
                    ExtractZip(file + "\\NameCube.zip", file, new Progress<double>(percent =>
                    {
                        progressBar.Value = (int)(percent * 100);
                    }));
                    File.Delete(file + "\\NameCube.zip");
                    if (updateMode == 1)
                    {
                        //快速安装模式创建桌面快捷方式
                        CreateDesktopShortcut(file + "\\点鸣魔方.exe");
                    }
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
            if (updateMode == 2)
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
                string file = textBox.Text;
                if (updateMode == 2)
                {
                    file = GetParentPath(file);
                }
                //运行点鸣魔方
                Process.Start(file + "\\点鸣魔方.exe");
            }
            Application.Current.Shutdown();
        }

        public static bool CreateDesktopShortcut(string app)
        {
            WshShell shell = null;
            try
            {
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = Path.Combine(desktop, "点鸣魔方.lnk");
                string appPath = app;
                string startMenuFolder = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                string shortcutStartPath = System.IO.Path.Combine(startMenuFolder, "Programs", "点鸣魔方" + ".lnk");

                if (!System.IO.File.Exists(appPath))
                {
                    throw new FileNotFoundException("应用程序路径无效");
                }

                shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                IWshShortcut shortcutStart = (IWshShortcut)shell.CreateShortcut(shortcutStartPath);
                shortcut.TargetPath = appPath;
                shortcutStart.TargetPath = appPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(appPath);
                shortcutStart.WorkingDirectory = Path.GetDirectoryName(appPath);
                shortcut.IconLocation = $"{appPath},0";
                shortcut.Description = "抽名字，点名器";
                shortcut.Save();
                shortcutStart.IconLocation = $"{appPath},0";
                shortcutStart.Description = "抽名字，点名器";
                shortcutStart.Save();

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                // 可选：手动释放 COM 对象
                if (shell != null)
                    Marshal.ReleaseComObject(shell);
            }
        }
    }
}