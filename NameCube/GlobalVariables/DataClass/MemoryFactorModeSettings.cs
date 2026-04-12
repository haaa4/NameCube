using Newtonsoft.Json;
using System.Collections.Generic;

namespace NameCube.GlobalVariables.DataClass
{
    public class MemoryFactorModeSettings
    {
        /// <summary>
        /// 启用朗读
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Speech { set; get; } = true;

        /// <summary>
        /// 是否允许修改
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Locked { get; set; } = false;

        /// <summary>
        /// 主界面字体跳动速度
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Speed { get; set; } = 20;

        /// <summary>
        /// 上一次抽取的姓名
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// debug模式
        /// </summary>
        public bool debug = false;

        /// <summary>
        /// 每一种事件的发生概率
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<int> probabilityOfHappening = new List<int> { 4, 2, 3, 4, 2, 2, 3, 1, 1, 1 };
    }
}