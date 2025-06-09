using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                // 验证源文件夹是否存在
                if (!Directory.Exists(sourcePath))
                {
                    return (false, $"源文件夹不存在: {sourcePath}");
                }

                // 处理目标文件夹已存在的情况
                if (Directory.Exists(destinationPath))
                {
                    if (overwrite)
                    {
                        // 递归删除目标文件夹
                        Directory.Delete(destinationPath, true);
                    }
                    else
                    {
                        return (false, $"目标文件夹已存在: {destinationPath}");
                    }
                }

                // 检查是否在同一驱动器（根路径相同）
                bool sameDrive = Path.GetPathRoot(sourcePath)?.Equals(
                    Path.GetPathRoot(destinationPath), StringComparison.OrdinalIgnoreCase) ?? false;

                if (sameDrive)
                {
                    // 同一驱动器：直接移动
                    Directory.Move(sourcePath, destinationPath);
                }
                else
                {
                    // 跨驱动器：复制后删除源文件夹
                    CopyDirectory(sourcePath, destinationPath);
                    Directory.Delete(sourcePath, true);
                }

                return (true, $"文件夹移动成功: {sourcePath} -> {destinationPath}");
            }
            catch (Exception ex)
            {
                string errorDetails = $"移动文件夹失败: {ex.GetType().Name} - {ex.Message}";

                // 兼容 C# 7.3 的错误处理
                if (ex is UnauthorizedAccessException)
                {
                    return (false, $"权限不足: {errorDetails}");
                }
                else if (ex is PathTooLongException)
                {
                    return (false, $"路径过长: {errorDetails}");
                }
                else if (ex is IOException)
                {
                    return (false, $"I/O错误: {errorDetails}");
                }
                else if (ex is DirectoryNotFoundException)
                {
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
            // 创建目标目录
            Directory.CreateDirectory(destDir);

            // 复制所有文件
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true); // 覆盖已存在文件
            }

            // 递归复制子目录
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }
    }
}
