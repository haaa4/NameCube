using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;  // 添加Serilog命名空间

namespace NameCube
{
    internal class FolderMover
    {
        /// <summary>
        /// 移动文件夹到新位置（支持跨磁盘驱动器）
        /// </summary>
        /// <param name="sourcePath">源文件夹路径</param>
        /// <param name="destinationPath">目标文件夹路径</param>
        /// <param name="overwrite">是否覆盖已存在的文件夹</param>
        /// <returns>操作结果和错误信息</returns>
        public static (bool Success, string Message) MoveFolder(
            string sourcePath,
            string destinationPath,
            bool overwrite = false)
        {
            try
            {
                Log.Information("开始移动文件夹，源路径: {SourcePath}，目标路径: {DestinationPath}，覆盖: {Overwrite}",
                    sourcePath, destinationPath, overwrite);

                // 验证源文件夹是否存在
                if (!Directory.Exists(sourcePath))
                {
                    Log.Error("源文件夹不存在: {SourcePath}", sourcePath);
                    return (false, $"源文件夹不存在: {sourcePath}");
                }

                // 处理目标文件夹已存在的情况
                if (Directory.Exists(destinationPath))
                {
                    if (overwrite)
                    {
                        Log.Information("目标文件夹已存在，执行覆盖操作");
                        // 递归删除目标文件夹
                        Directory.Delete(destinationPath, true);
                        Log.Information("目标文件夹已删除");
                    }
                    else
                    {
                        Log.Error("目标文件夹已存在且不允许覆盖: {DestinationPath}", destinationPath);
                        return (false, $"目标文件夹已存在: {destinationPath}");
                    }
                }

                // 检查是否在同一驱动器（根路径相同）
                string sourceRoot = Path.GetPathRoot(sourcePath);
                string destRoot = Path.GetPathRoot(destinationPath);
                bool sameDrive = sourceRoot?.Equals(destRoot, StringComparison.OrdinalIgnoreCase) ?? false;

                Log.Debug("驱动器检查 - 源驱动器: {SourceRoot}，目标驱动器: {DestRoot}，同一驱动器: {SameDrive}",
                    sourceRoot, destRoot, sameDrive);

                if (sameDrive)
                {
                    // 同一驱动器：直接移动
                    Log.Information("同一驱动器，直接移动文件夹");
                    Directory.Move(sourcePath, destinationPath);
                }
                else
                {
                    // 跨驱动器：复制后删除源文件夹
                    Log.Information("跨驱动器，复制后删除源文件夹");
                    CopyDirectory(sourcePath, destinationPath);
                    Directory.Delete(sourcePath, true);
                    Log.Information("源文件夹已删除");
                }

                Log.Information("文件夹移动成功: {SourcePath} -> {DestinationPath}", sourcePath, destinationPath);
                return (true, $"文件夹移动成功: {sourcePath} -> {destinationPath}");
            }
            catch (Exception ex)
            {
                string errorDetails = $"移动文件夹失败: {ex.GetType().Name} - {ex.Message}";
                Log.Error(ex, "移动文件夹失败: {SourcePath} -> {DestinationPath}", sourcePath, destinationPath);

                // 兼容 C# 7.3 的错误处理
                if (ex is UnauthorizedAccessException)
                {
                    Log.Error("权限不足错误: {ErrorDetails}", errorDetails);
                    return (false, $"权限不足: {errorDetails}");
                }
                else if (ex is PathTooLongException)
                {
                    Log.Error("路径过长错误: {ErrorDetails}", errorDetails);
                    return (false, $"路径过长: {errorDetails}");
                }
                else if (ex is IOException)
                {
                    Log.Error("I/O错误: {ErrorDetails}", errorDetails);
                    return (false, $"I/O错误: {errorDetails}");
                }
                else if (ex is DirectoryNotFoundException)
                {
                    Log.Error("目录未找到错误: {ErrorDetails}", errorDetails);
                    return (false, $"目录未找到: {errorDetails}");
                }
                else
                {
                    return (false, errorDetails);
                }
            }
        }

        /// <summary>
        /// 递归复制目录及其内容
        /// </summary>
        private static void CopyDirectory(string sourceDir, string destDir)
        {
            try
            {
                Log.Debug("开始复制目录: {SourceDir} -> {DestDir}", sourceDir, destDir);

                // 创建目标目录
                Directory.CreateDirectory(destDir);
                Log.Debug("目标目录已创建: {DestDir}", destDir);

                // 复制所有文件
                foreach (string file in Directory.GetFiles(sourceDir))
                {
                    string destFile = Path.Combine(destDir, Path.GetFileName(file));
                    Log.Debug("复制文件: {SourceFile} -> {DestFile}", file, destFile);
                    File.Copy(file, destFile, true); // 覆盖已存在文件
                }

                // 递归复制子目录
                foreach (string subDir in Directory.GetDirectories(sourceDir))
                {
                    string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                    CopyDirectory(subDir, destSubDir);
                }

                Log.Debug("目录复制完成: {SourceDir} -> {DestDir}", sourceDir, destDir);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "复制目录时发生错误: {SourceDir} -> {DestDir}", sourceDir, destDir);
                throw;
            }
        }
    }
}