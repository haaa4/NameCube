using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameCube.GlobalVariables.DataClass
{
    public class BatchModeSettings
    {
        /// <summary>
        /// 是否为数字模式
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool NumberMode { get; set; } = false;

        /// <summary>
        /// 数字数量
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Number { get; set; } = 53;

        /// <summary>
        /// 抽取数量
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Index { get; set; } = 10;

        /// <summary>
        /// 允许重复
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Repetition { get; set; } = false;

        /// <summary>
        /// 是否允许修改
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Locked { get; set; } = false;

        /// <summary>
        /// 上一次抽取的姓名表
        /// </summary>
        public List<string> LastName { get; set; } = new List<string>();
    }
}
