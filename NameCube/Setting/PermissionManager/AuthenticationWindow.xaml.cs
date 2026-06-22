using NameCube.Function;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace NameCube.Setting.PermissionManager
{
    /// <summary>
    /// AuthenticationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AuthenticationWindow
    {
        public AuthenticationWindow()
        {
            InitializeComponent();
        }
        bool USBAuthenticationSuccessful = false;

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            USBAuthenticationSuccessful = UsbVerificationService.VerifyUsbCredentials(GlobalVariablesData.creds, GlobalVariablesData.config.PermissionManager.usbHash);
            if(USBAuthenticationSuccessful)
            {
                btnUsbVerification.Content="验证通过，可直接按确定继续";
                btnUsbVerification.IsEnabled = false;
            }
            else
            {
                btnUsbVerification.Content = "未找到匹配的U盘，请插入绑定的U盘后按此按钮重试";
                btnUsbVerification.IsEnabled = true;
            }
        }

        private void btnUsbVerification_Click(object sender, RoutedEventArgs e)
        {
            btnUsbVerification.IsEnabled = false;
            btnUsbVerification.Content = "正在认证U盘凭证";
            USBAuthenticationSuccessful = UsbVerificationService.VerifyUsbCredentials(GlobalVariablesData.creds, GlobalVariablesData.config.PermissionManager.usbHash);
            if (USBAuthenticationSuccessful)
            {
                btnUsbVerification.Content = "验证通过，可直接按确定继续";
                btnUsbVerification.IsEnabled = false;
            }
            else
            {
                btnUsbVerification.Content = "未找到匹配的U盘，请插入绑定的U盘后重试";
                btnUsbVerification.IsEnabled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(USBAuthenticationSuccessful||Password.Password==GlobalVariablesData.creds)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBoxFunction.ShowMessageBoxWarning("密码错误且未找到匹配的U盘，请重新输入密码或插入绑定的U盘后重试！");
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
    public static class UsbVerificationService
    {
        // 密钥文件名（与绑定时一致）
        private const string KeyFileName = "NameCube_app_key.dat";
        // AES解密用的盐值（与绑定时一致）
        private static readonly byte[] Salt = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

        /// <summary>
        /// 验证是否有任何一个已插入的U盘包含正确的凭证。
        /// </summary>
        /// <param name="password">用户输入的密码</param>
        /// <param name="localHash">本地存储的密钥哈希（Base64字符串）</param>
        /// <returns>如果找到一个匹配的U盘则返回 true，否则 false</returns>
        public static bool VerifyUsbCredentials(string password, string localHash)
        {
            // 1. 获取所有当前可用的U盘根目录
            var usbDrives = GetAvailableUsbDrives();
            if (usbDrives.Count == 0)
                return false; // 没有U盘

            // 2. 遍历每个U盘进行验证
            foreach (string rootPath in usbDrives)
            {
                if (TryVerifySingleUsb(rootPath, password, localHash))
                    return true;
            }

            return false; // 所有U盘都不匹配
        }

        /// <summary>
        /// 获取所有可用的U盘根目录（如 "E:\", "F:\"）
        /// </summary>
        private static List<string> GetAvailableUsbDrives()
        {
            return DriveInfo.GetDrives()
                            .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                            .Select(d => d.RootDirectory.FullName)
                            .ToList();
        }

        /// <summary>
        /// 尝试验证单个U盘
        /// </summary>
        private static bool TryVerifySingleUsb(string rootPath, string password, string localHash)
        {
            try
            {
                // 1. 检查密钥文件是否存在
                string filePath = Path.Combine(rootPath, KeyFileName);
                if (!File.Exists(filePath))
                    return false;

                // 2. 读取加密内容并解密
                string encrypted = File.ReadAllText(filePath);
                string combined = DecryptString(encrypted, password);
                if (string.IsNullOrEmpty(combined))
                    return false; // 密码错误或数据损坏

                // 3. 拆分出密钥和序列号
                string[] parts = combined.Split(':');
                if (parts.Length != 2)
                    return false;

                string secretBase64 = parts[0];
                string savedSerial = parts[1];

                // 4. 验证序列号是否与当前U盘一致
                string currentSerial = GetUsbSerialNumber(rootPath);
                if (string.IsNullOrEmpty(currentSerial) || currentSerial != savedSerial)
                    return false;

                // 5. 计算密钥的哈希并与本地哈希比对
                byte[] secretKey = Convert.FromBase64String(secretBase64);
                using (var sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(secretKey);
                    string hashBase64 = Convert.ToBase64String(hash);
                    return string.Equals(hashBase64, localHash);
                }
            }
            catch
            {
                // 任何异常（如文件损坏、解密失败等）都视为该U盘无效
                return false;
            }
        }

        /// <summary>
        /// 获取U盘的硬件序列号（通过WMI）
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

        /// <summary>
        /// AES解密方法（与绑定时对应）
        /// </summary>
        private static string DecryptString(string cipherText, string password)
        {
            try
            {
                byte[] fullBytes = Convert.FromBase64String(cipherText);
                using (var aes = Aes.Create())
                {
                    // 提取IV（前16字节）
                    byte[] iv = new byte[16];
                    Buffer.BlockCopy(fullBytes, 0, iv, 0, 16);

                    // 提取密文（剩余部分）
                    byte[] cipherBytes = new byte[fullBytes.Length - 16];
                    Buffer.BlockCopy(fullBytes, 16, cipherBytes, 0, cipherBytes.Length);

                    // 用相同的密码和盐生成密钥
                    using (var deriveBytes = new Rfc2898DeriveBytes(password, Salt, 10000))
                    {
                        aes.Key = deriveBytes.GetBytes(32);
                        aes.IV = iv;
                    }

                    var decryptor = aes.CreateDecryptor();
                    byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
