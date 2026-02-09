using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public List<int> probabilityOfHappening = new List<int> { 4, 2, 3, 4, 2, 2, 3, 1, 1 };
    }
}
