using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameCube.GlobalVariables.DataClass
{
    public class AutomaticProcess
    {
        /// <summary>
        /// debug模式
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool debug { get; set; } = false;

        /// <summary>
        /// 所有流程组
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<ProcessGroup> processGroups { get; set; } =
            new List<ProcessGroup> { new ProcessGroup() };

        /// <summary>
        /// 自动运行流程组的时间表
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public Dictionary<int, List<ProcessGroup>> processesSchedule { get; set; } =
            new Dictionary<int, List<ProcessGroup>>();
    }

    public class ProcessGroup
    {
        /// <summary>
        /// 流程数据列表
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<ProcessData> processDatas { get; set; } = new List<ProcessData>();

        /// <summary>
        /// 流程组名称
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string name { get; set; } = "默认流程";

        /// <summary>
        /// 准备时提醒信息
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string remindText;

        /// <summary>
        /// 准备时提醒时间（秒）
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int remindTime = 5;

        /// <summary>
        /// 可否取消流程
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool canCancle = true;

        /// <summary>
        /// 每个流程组的唯一标识符
        /// </summary>
        public int uid;

        /// <summary>
        /// 是否显示窗口
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool show = true;
    }

    /// <summary>
    /// 存储流程数据的类，不同流程类型使用不同的数据
    /// </summary>
    public class ProcessData
    {
        /// <summary>
        /// 流程类型
        /// </summary>
        public ProcessState state;

        /// <summary>
        /// 第一个字符串数据
        /// </summary>
        public string stringData1;

        /// <summary>
        /// 第二个字符串数据（未来备用）
        /// </summary>
        public string stringData2;

        /// <summary>
        /// 数字数据
        /// </summary>
        public double doubleData;

        /// <summary>
        /// 布尔值数据
        /// </summary>
        public bool boolData;
    }

    public enum ProcessState
    {
        start,
        audio,
        read,
        cmd,
        wait,
        clear,
        shutDown,
    }
}
