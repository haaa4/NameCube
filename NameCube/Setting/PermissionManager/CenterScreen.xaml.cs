using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media.Animation;

namespace NameCube.Setting.PermissionManager
{
    /// <summary>
    /// 绑定U盘窗口
    /// </summary>
    public partial class CenterScreen 
    {
        // 存储密钥文件的名称（固定）
        private const string KeyFileName = "NameCube_app_key.dat";
        // 用于AES加密的固定盐值
        private static readonly byte[] Salt = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };



        public CenterScreen()
        {
            InitializeComponent();
            Loaded += BindUsbWindow_Loaded;
        }

        private void BindUsbWindow_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUsbList();
        }

        /// <summary>
        /// 刷新U盘列表
        /// </summary>
        private void RefreshUsbList()
        {
            var drives = GetUsbDrivesWithDetails();
            cmbUsbDrives.ItemsSource = drives;
            if (drives.Any())
                cmbUsbDrives.SelectedIndex = 0;
            else
                lblStatus.Text = "未检测到U盘，请插入后点击刷新。";
        }

        /// <summary>
        /// 获取所有可用的U盘信息（包含盘符和卷标）
        /// </summary>
        private List<UsbDriveInfo> GetUsbDrivesWithDetails()
        {
            var list = new List<UsbDriveInfo>();
            var drives = DriveInfo.GetDrives()
                                  .Where(d => d.DriveType == DriveType.Removable && d.IsReady);
            foreach (var drive in drives)
            {
                string serial = GetUsbSerialNumber(drive.RootDirectory.FullName);
                list.Add(new UsbDriveInfo
                {
                    RootPath = drive.RootDirectory.FullName,
                    VolumeLabel = string.IsNullOrEmpty(drive.VolumeLabel) ? "无卷标" : drive.VolumeLabel,
                    SerialNumber = serial ?? "未知"
                });
            }
            return list;
        }

        /// <summary>
        /// 通过WMI获取U盘的硬件序列号
        /// </summary>
        private static string GetUsbSerialNumber(string driveLetter)
        {
            try
            {
                string drive = driveLetter.TrimEnd('\\');
                // 使用精确查询，避免遍历全部
                using (var searcher = new ManagementObjectSearcher(
                    $"SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID='{drive}'"))
                {
                    foreach (ManagementObject disk in searcher.Get())
                    {
                        return disk["VolumeSerialNumber"]?.ToString().Trim();
                    }
                }
            }
            catch
            {
                // WMI 异常返回 null
            }
            return null;
        }

        // 刷新按钮点击事件
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshUsbList();
        }

        // 绑定按钮点击事件
        private void BtnBind_Click(object sender, RoutedEventArgs e)
        {
            // 1. 验证输入
            if (cmbUsbDrives.SelectedItem == null)
            {
                lblStatus.Text = "请先选择一个U盘！";
                lblStatus.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            string password = GlobalVariablesData.creds;


            // 2. 获取U盘信息
            var selectedDrive = (UsbDriveInfo)cmbUsbDrives.SelectedItem;
            string rootPath = selectedDrive.RootPath;
            string serial = selectedDrive.SerialNumber;
            if (string.IsNullOrEmpty(serial) || serial == "未知")
            {
                lblStatus.Text = "无法获取该U盘的硬件序列号，绑定可能不安全。是否继续？";
            }

            // 3. 执行绑定
            try
            {
                BindUsbKey(rootPath, password, serial);
                lblStatus.Text = $"U盘 {rootPath} 绑定成功！密钥文件已保存。";
                lblStatus.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"绑定失败：{ex.Message}";
                lblStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        /// <summary>
        /// 生成随机密钥，加密保存到U盘，并将哈希保存到本地
        /// </summary>
        private void BindUsbKey(string usbDrivePath, string userPassword, string serialNumber)
        {
            // 1. 生成32字节随机密钥
            byte[] secretKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secretKey);
            }
            string secretBase64 = Convert.ToBase64String(secretKey);

            // 2. 将密钥与序列号组合（防复制）
            string combined = $"{secretBase64}:{serialNumber}";

            // 3. 使用用户密码加密组合字符串
            string encrypted = EncryptString(combined, userPassword);

            // 4. 写入U盘（先删除旧文件，避免属性冲突）
            string fullPath = Path.Combine(usbDrivePath, KeyFileName);
            if (File.Exists(fullPath))
            {
                File.SetAttributes(fullPath, FileAttributes.Normal); // 移除隐藏属性
                File.Delete(fullPath); // 删除旧文件
            }

            File.WriteAllText(fullPath, encrypted);
            File.SetAttributes(fullPath, FileAttributes.Hidden); // 重新设为隐藏

            // 5. 将原始密钥的哈希保存到本地（用于验证）
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(secretKey);
                string hashBase64 = Convert.ToBase64String(hash);
                GlobalVariablesData.config.PermissionManager.usbHash = hashBase64;
                GlobalVariablesData.SaveConfig();
            }
        }


        /// <summary>
        /// AES加密方法（使用用户密码派生密钥）
        /// </summary>
        private string EncryptString(string plainText, string password)
        {
            using (var aes = Aes.Create())
            {
#pragma warning disable SYSLIB0041 // 类型或成员已过时
                using (var deriveBytes = new Rfc2898DeriveBytes(password, Salt, 10000))
                {
                    aes.Key = deriveBytes.GetBytes(32);
                    aes.IV = deriveBytes.GetBytes(16);
                }
#pragma warning restore SYSLIB0041 // 类型或成员已过时

                var encryptor = aes.CreateEncryptor();
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // 将 IV 和密文拼接
                byte[] result = new byte[aes.IV.Length + cipherBytes.Length];
                Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);
                return Convert.ToBase64String(result);
            }
        }

        // 取消按钮
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var story = FindResource("Start") as Storyboard;
            story?.Begin();
        }
    }

    /// <summary>
    /// U盘信息实体类（用于ComboBox绑定）
    /// </summary>
    public class UsbDriveInfo
    {
        public string RootPath { get; set; }
        public string VolumeLabel { get; set; }
        public string SerialNumber { get; set; }

        // 用于ComboBox显示的文字
        public string DisplayText => $"{RootPath} ({VolumeLabel}) - SN:{SerialNumber ?? "未知"}";
    }
}