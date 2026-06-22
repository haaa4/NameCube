using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameCube.GlobalVariables.DataClass
{
    public class PermissionManager
    {
        /// <summary>
        /// 用户的USB设备哈希值，用于权限验证
        /// </summary>
        public string usbHash { get; set; } = string.Empty;
        /// <summary>
        /// 各部分功能是否需要管理员权限，按照顺序对应请查看PermissionManagerSettings.xaml
        /// </summary>
        public List<bool> needAdmin { get; set; } = new List<bool> { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
    }
}
