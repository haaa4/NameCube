using Masuit.Tools.Logging;
using NameCube.Function;
using NameCube.GlobalVariables.DataClass;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using BirdSettings = NameCube.GlobalVariables.DataClass.BirdSettingsData;
using Path = System.IO.Path;

namespace NameCube
{
    public class Json
    {
        /// <summary>
        /// 全局设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public AllSettings AllSettings { get; set; } = new AllSettings();

        /// <summary>
        /// 单人模式设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public OnePeopleModeSettings OnePeopleModeSettings { get; set; } =
            new OnePeopleModeSettings();

        /// <summary>
        /// 悬浮球设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public BirdSettings BirdSettings { get; set; } = new BirdSettings();

        /// <summary>
        /// 启动设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public StartToDo StartToDo { get; set; } = new StartToDo();

        /// <summary>
        /// 势能模式设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public MemoryFactorModeSettings MemoryFactorModeSettings { get; set; } =
            new MemoryFactorModeSettings();

        /// <summary>
        /// 批量模式设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public BatchModeSettings BatchModeSettings { get; set; } = new BatchModeSettings();

        /// <summary>
        /// 数字模式设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public NumberModeSettings NumberModeSettings { get; set; } = new NumberModeSettings();

        /// <summary>
        /// 预备模式设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public PrepareModeSetting PrepareModeSetting { get; set; } = new PrepareModeSetting();

        /// <summary>
        /// 记忆模式设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public MemoryModeSettings MemoryModeSettings { get; set; } = new MemoryModeSettings();

        /// <summary>
        /// 快捷键设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ShortCutKey ShortCutKey { get; set; } = new ShortCutKey();

        /// <summary>
        /// 自动流程设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public AutomaticProcess AutomaticProcess { get; set; } = new AutomaticProcess();

        /// <summary>
        /// 用户相关信息
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ImformationData ImformationData { get; set; } = new ImformationData();

        /// <summary>
        /// 配置文件的上一次运行所在点鸣魔方的版本
        /// </summary>
        public string LastVersion { get; set; }
    }

    /// <summary>
    /// 全局资源
    /// </summary>
    public static class GlobalVariablesData
    {
        /// <summary>
        /// 所有窗口的设置资源
        /// </summary>
        public static Json config = new Json(); // 默认初始化

        /// <summary>
        /// 配置文件储存地址
        /// </summary>
        public static string configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NameCube"
        );

        /// <summary>
        /// 用户数据文件夹
        /// </summary>
        public static string userDataDir = Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "user");

        /// <summary>
        /// 保存设置
        /// </summary>
        public static void SaveConfig()
        {
            Log.Debug("正在保存配置文件到: {ConfigPath}", Path.Combine(configDir, "config.json"));
            string configPath = Path.Combine(configDir, "config.json");
            try
            {
                // 确保目录存在
                Directory.CreateDirectory(configDir);

                string jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configPath, jsonString);
            }
            catch (Exception ex)
            {
                // 记录错误或提示用户
                MessageBoxFunction.ShowMessageBoxWarning($"保存配置失败: {ex.Message}");
                LogManager.Error(ex);
            }
        }

        /// <summary>
        /// 当前版本
        /// </summary>
        public const string VERSION = "V1.3(#l6#)";

        /// <summary>
        /// 当前版本代码
        /// </summary>
        public const int VERSIONCODE = 6;

        /// <summary>
        /// 当前是否为测试版本
        /// </summary>
        public const bool ISBETA = false;

        public static bool ret = false;
    }
}