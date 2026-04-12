using Newtonsoft.Json;
using System.Collections.Generic;

namespace NameCube.GlobalVariables.DataClass
{
    public class PrepareModeSetting
    {
        /// <summary>
        /// 是否启用朗读
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Speak { get; set; } = true;

        /// <summary>
        /// 是否允许修改
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Locked { set; get; } = false;

        /// <summary>
        /// 跳动速度
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Speed { get; set; } = 20;

        /// <summary>
        /// 上次的准备列表
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> Name { get; set; } = new List<string>();

        /// <summary>
        /// 上一次抽取的姓名
        /// </summary>
        public string LastName { get; set; }
    }
}