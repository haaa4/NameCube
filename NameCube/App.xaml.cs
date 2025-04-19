using Masuit.Tools.Logging;
using NameCube.Setting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Threading;
using System.Windows;

namespace NameCube
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    /// 
    public class allSettings
    {
        /// <summary>
        /// 姓名表
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> Name { get; set; }=new List<string> { "张三" };
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
        /// 开机自启动
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Start { get; set; } = false;
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
    }
    public class onePeopleModeSettings
    {
        /// <summary>
        /// 启用朗读人
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Speech { get; set; } = true;
        /// <summary>
        /// 禁用等待
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Wait { get; set; } = false;
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

    }
    public class startToDo
    {
        /// <summary>
        /// 启用朗读球
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Ball { get; set; } = false;
        /// <summary>
        /// 自动内存清理
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AlwaysCleanMemory { get; set; } = false;
        /// <summary>
        /// 自动关机小时
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int HourPowerOff { get; set; } = 20;
        /// <summary>
        /// 自动关机分钟
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int MinPowerOff { get; set; } = 35;
        /// <summary>
        /// 是否启用自动关机
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool PowerOff { get; set; } = false;

    }
    public class memoryFactorModeSettings
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
    }
    public class BatchModeSettings
    {
        /// <summary>
        /// 是否为数字模式
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool NumberMode { get; set; }= false;
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
        public bool Repetition { get; set; }=false;
    }
    public class BirdSettings
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
        /// 无反应后自动吸附
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AutoAbsord { get; set; } = false;
        /// <summary>
        /// 透明度
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)] 
        public int diaphaneity { get; set; } = 0;
    }


    public class Json
    {
        /// <summary>
        /// 全局设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)] 
        public allSettings AllSettings { get; set; }= new allSettings();
        /// <summary>
        /// 单人模式设置
        /// </summary>

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public onePeopleModeSettings OnePeopleModeSettings { get; set; }=new onePeopleModeSettings();
        /// <summary>
        /// 悬浮球设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)] 
        public BirdSettings BirdSettings { get; set; }=new BirdSettings();
        /// <summary>
        /// 启动设置
        /// </summary>
     
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)] 
        public startToDo StartToDo { get; set; } = new startToDo();
        /// <summary>
        /// 记忆因子模式设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)] 
        public memoryFactorModeSettings MemoryFactorModeSettings { get; set; } = new memoryFactorModeSettings();
        /// <summary>
        /// 批量模式设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)] 
        public BatchModeSettings BatchModeSettings { get; set; } = new BatchModeSettings();
        /// <summary>
        /// 当前版本
        /// </summary>
        public string Version="Alpha-6" ;


    }
    /// <summary>
    /// 全局资源
    /// </summary>
    public static class GlobalVariables
    {
        /// <summary>
        /// 所有窗口的设置资源
        /// </summary>
        public static Json json = new Json(); // 默认初始化
        /// <summary>
        /// 配置文件储存地址
        /// </summary>
        public static string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NameCube");
        /// <summary>
        /// 保存设置
        /// </summary>
        public static void SaveJson()
        {
            LogManager.Info("保存设置");

            string configPath = Path.Combine(configDir, "config.json");

            try
            {
                // 确保目录存在
                Directory.CreateDirectory(configDir);

                string jsonString = JsonConvert.SerializeObject(json);
                File.WriteAllText(configPath, jsonString);
            }
            catch (Exception ex)
            {
                // 记录错误或提示用户
                System.Windows.MessageBox.Show($"保存配置失败: {ex.Message}");
                LogManager.Error(ex);
            }
        }

    }
    
    public partial class App : System.Windows.Application
    {
        Mutex mutex;

        public App()
        {
            this.Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            bool ret;
            mutex = new Mutex(true, "NameCube", out ret);
            if (!ret&&!File.Exists(Path.Combine(GlobalVariables.configDir, "START")))
            {
                System.Windows.MessageBox.Show("哥们，你已经启动一个实例了,看看系统托盘吧（笑 \n" +
                    "翻译：你已经启动了软件");
                Environment.Exit(0);
            }
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configDir = Path.Combine(appDataPath, "NameCube");
            string configPath = Path.Combine(configDir, "config.json");
            LogManager.LogDirectory = Path.Combine(configDir, "logs");
            if(File.Exists(Path.Combine(GlobalVariables.configDir, "START")))
            {
                File.Delete(Path.Combine(GlobalVariables.configDir, "START"));
            }
            try
            {
                // 确保目录存在
                Directory.CreateDirectory(configDir);
                GlobalVariables.json = new Json
                {
                    AllSettings = new allSettings
                    {
                        Name = new List<string> { "张三" },
                        Dark = false,
                        Volume = 100,
                        Speed = 0,
                        Start = false,
                        SystemSpeech = false,
                        Top = true
                    },
                    StartToDo = new startToDo
                    {
                        Ball = false,
                        AlwaysCleanMemory = false
                    },
                    BirdSettings = new BirdSettings
                    {
                        StartWay = 4,
                        UseDefinedImage = false,
                        AdsorbValue = 60,
                        AutoAbsord = false,
                        diaphaneity = 0,
                    },

                    OnePeopleModeSettings = new onePeopleModeSettings
                    {
                        Speech = true,
                        Wait = false,
                        Locked = false,
                        Speed = 20,
                    },
                    MemoryFactorModeSettings = new memoryFactorModeSettings
                    {
                        Speech = true,
                        Locked = false,
                        Speed = 20,
                    },
                    BatchModeSettings = new BatchModeSettings
                    {
                        NumberMode = false,
                        Number = 53,
                        Index = 10,
                        Repetition = false
                    }

                };
                if (!File.Exists(configPath))
                {
                    LogManager.Info("找不到文件");
                    // 初始化默认配置
                    GlobalVariables.SaveJson(); // 确保保存成功
                }

                // 再次检查文件是否存在（防止异步问题）
                if (File.Exists(configPath))
                {
                    var jsonString = File.ReadAllText(configPath);
                    JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ObjectCreationHandling = ObjectCreationHandling.Replace,
                        DefaultValueHandling = DefaultValueHandling.Populate
                    };
                    GlobalVariables.json = JsonConvert.DeserializeObject<Json>(jsonString);
                    GlobalVariables.json.Version = "Alpha-6";
                }
                else
                {
                    throw new FileNotFoundException("配置文件未找到且初始化失败。");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
            LogManager.Info("程序启动");
            MainWindow mainWindow = new MainWindow();
              
        }

        
    }
}
