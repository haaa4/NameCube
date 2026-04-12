using Newtonsoft.Json;

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