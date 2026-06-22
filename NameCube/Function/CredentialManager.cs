using CredentialManagement;
using NameCube.Setting.PermissionManager;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NameCube.Function
{

    public static class CredentialHelper
    {
        // 唯一的目标名称
        private const string TargetName = "NameCube_UserCredential";
        /// <summary>
        /// 保存或更新凭据
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        public static void SaveCredential(string username, string password)
        {
            using (var cred = new Credential())
            {
                cred.Target = TargetName;
                cred.Username = username;
                cred.Password = password;
                cred.PersistanceType = PersistanceType.LocalComputer;
                cred.Save();
            }
        }

        /// <summary>
        /// 读取凭据，如果不存在则返回 null
        /// </summary>
        /// <returns>凭据，如果不存在则返回 null</returns>
        public static string? LoadCredential()
        {
            using (var cred = new Credential())
            {
                cred.Target = TargetName;
                // Load() 方法会尝试从凭据管理器中加载匹配 Target 的凭据
                if (!cred.Load())
                {
                    return null; // 未找到凭据
                }
                return cred.Password    ;
            }
        }

        /// <summary>
        /// 删除凭据
        /// </summary>
        public static void DeleteCredential()
        {
            using (var cred = new Credential())
            {
                cred.Target = TargetName;
                cred.Delete();
            }
        }
        
        public static bool PermissionVerification()
        {
            if(GlobalVariablesData.creds==null)
            {
                return true;
            }
            AuthenticationWindow authManager = new();
            bool? get=authManager.ShowDialog();
            if(get.HasValue==false)
            {
                get = false;
            }
            Log.Information("Permission verification result: {Result}", get.Value);
            return get.Value;
        }
    }
}
