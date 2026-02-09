using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameCube.GlobalVariables.DataClass
{
    public class StartToDo
    {
        /// <summary>
        /// 启用朗读球
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Ball { get; set; } = false;

        /// <summary>
        /// 自动内存清理
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AlwaysCleanMemory { get; set; } = false;

        /// <summary>
        /// 自动请求升级
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AutoUpdata { get; set; } = true;
    }
}
