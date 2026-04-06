using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameCube.GlobalVariables.DataClass
{
    public class ImformationData
    {
        /// <summary>
        /// 推荐过更好的颜色
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UsedBetterColor { get; set; } = false;
    }
}
