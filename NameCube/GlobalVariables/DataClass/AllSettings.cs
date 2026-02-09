using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NameCube.GlobalVariables.DataClass
{
    public class AllSettings
    {
        /// <summary>
        /// 姓名表
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> Name { get; set; } = new List<string> { "张三", "李四", "王五" };

        /// <summary>
        /// 黑暗模式
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Dark { get; set; } = false;

        /// <summary>
        /// 朗读音量
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Volume { get; set; } = 100;

        /// <summary>
        /// 朗读速度
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Speed { get; set; } = 0;

        /// <summary>
        /// 使用系统设置的朗读
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool SystemSpeech { get; set; } = false;

        /// <summary>
        /// 主窗口始终置顶
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Top { get; set; } = true;

        /// <summary>
        /// 学号魔方模式（0:长期运行模式 1：单次运行模式）
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int NameCubeMode { get; set; } = 0;

        /// <summary>
        /// 字体颜色
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public Brush color { get; set; } =
            (Brush)new BrushConverter().ConvertFromInvariantString("#FF005493");

        ///// <summary>
        ///// 字体类型
        ///// </summary>
        //[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        //public FontFamily Font { get; set; } = new FontFamily("Arial");

        /// <summary>
        /// 上次更新的检查时间
        /// </summary>
        public string UpdataTime;

        /// <summary>
        /// 更新获取通道（0：仅正式版本，1：所有版本）
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int UpdataGet { get; set; } = 0;

        /// <summary>
        /// 是否显示推荐，如果为当前版本名或者None，则不显示
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Recommend { get; set; }

        /// <summary>
        /// 低内存模式
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool LowMemoryMode { get; set; }

        /// <summary>
        /// 用户更新token
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string token { get; set; }
        /// <summary>
        /// 新版本号
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string newVersion { get; set; }

        /// <summary>
        /// Debug模式
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool debug { get; set; }

        /// <summary>
        /// 禁用主界面显示动画
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DisableTheDisplayAnimationOfTheMainWindow { get; set; }
        /// <summary>
        /// 默认最大化窗口
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool DefaultToMaximumSize { get; set; }
        /// <summary>
        /// log等级
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int LogLevel { get; set; } = 1;
    }
}
