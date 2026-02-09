using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NameCube.GlobalVariables.DataClass
{


    public class ShortCut
    {
        /// <summary>
        /// 快捷键
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<Key> keys { get; set; } = new List<Key>();

        /// <summary>
        /// 启动方式(0=无,1=单人模式,2=因子模式,3=批量模式,4=数字模式,5=预备模式,6=记忆模式,7=主页)
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int openWay { get; set; } = -1;

        /// <summary>
        /// 触发的自动流程(为null时则以openway为准)
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ProcessGroup ProcessGroup { get; set; } = null;

        /// <summary>
        /// 上一次修改时间
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string LastChangeTime { get; set; } = "-1";
    }
    public class ShortCutKey
    {
        /// <summary>
        /// 快捷键组合
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<ShortCut> keysGrounp = new List<ShortCut>();
    }
}
