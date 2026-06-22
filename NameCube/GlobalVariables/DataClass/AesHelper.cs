using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NameCube.GlobalVariables.DataClass
{
    public static class AesHelper
    {
        public static class AesBestPracticeHelper
        {
            private const int KeySize = 32;

            /// <summary>
            /// 加密：自动生成随机IV，并将IV拼接到密文头部返回
            /// </summary>
            public static string Encrypt(string plainText, string keyBase64)
            {
                keyBase64= Convert.ToBase64String(GetValidKey(keyBase64)); // 确保密钥是32字节
                if (string.IsNullOrEmpty(plainText)) throw new ArgumentNullException(nameof(plainText));
                byte[] key = Convert.FromBase64String(keyBase64);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    // 1. 自动生成一个随机的16字节IV
                    aesAlg.GenerateIV();
                    byte[] iv = aesAlg.IV;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor();
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        // 2. 首先将 IV 写入流的最前面（先写IV）
                        msEncrypt.Write(iv, 0, iv.Length);

                        // 3. 再写入密文数据
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                            csEncrypt.FlushFinalBlock();
                        }

                        // 4. 最终返回 Base64（包含 IV + 密文）
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }

            /// <summary>
            /// 解密：自动从密文中截取前16字节作为IV，再解密后面的内容
            /// </summary>
            public static string Decrypt(string cipherTextBase64, string keyBase64)
            {
                keyBase64 = Convert.ToBase64String(GetValidKey(keyBase64)); // 确保密钥是32字节
                if (string.IsNullOrEmpty(cipherTextBase64)) throw new ArgumentNullException(nameof(cipherTextBase64));
                byte[] key = Convert.FromBase64String(keyBase64);
                byte[] fullCipherBytes = Convert.FromBase64String(cipherTextBase64);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;

                    // 1. 从密文中取出前 16 个字节作为 IV
                    byte[] iv = new byte[aesAlg.BlockSize / 8]; // AES固定为16字节
                    byte[] cipherBytes = new byte[fullCipherBytes.Length - iv.Length];

                    Array.Copy(fullCipherBytes, 0, iv, 0, iv.Length);
                    Array.Copy(fullCipherBytes, iv.Length, cipherBytes, 0, cipherBytes.Length);

                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor();
                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 转化为32字节有效秘钥
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static byte[] GetValidKey(string key)
        {
            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
                // 对于AES，我们可以使用前32字节 (256位)
                return hash; // 32字节
            }
        }
    }
}
