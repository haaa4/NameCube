using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameCube.GlobalVariables.DataClass
{
    public class BirdSettingsData
    {
        /// <summary>
        /// 主界面启动方式（0：左键，1：右键，2：长按，3：左键+右键，4：右键+长按）
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int StartWay { get; set; } = 4;

        /// <summary>
        /// 是否为用户自定义图标图片
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool UseDefinedImage { get; set; } = false;

        /// <summary>
        /// 吸附阈值
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int AdsorbValue { get; set; } = 60;
        /// <summary>
        /// 长按误判阈值
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int LongPressMisjudgment { get; set; } = 30;
        /// <summary>
        /// 无反应后自动吸附
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AutoAbsord { get; set; } = false;

        /// <summary>
        /// 透明度
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int diaphaneity { get; set; } = 0;

        /// <summary>
        /// 启动位置（0:屏幕左侧，1:屏幕右侧，2:上一次位置）
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int StartLocationWay { get; set; } = 0;

        /// <summary>
        /// 启动位置x坐标
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public double StartLocationX { get; set; } = 0;

        /// <summary>
        /// 启动位置y坐标
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public double StartLocationY { get; set; } = 0;

        /// <summary>
        /// 悬浮球长度
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Width { get; set; } = 50;

        /// <summary>
        /// 悬浮球高度
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Height { get; set; } = 50;
    }
}
