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
                        Repetition = false,
                        Locked=false,
                    },
                    NumberModeSettings = new NumberModeSettings
                    { 
                        Num=53,
                        Speak=true,
                        Locked=false,
                        Speed=20,
                    },
                    PrepareModeSetting = new PrepareModeSetting
                    {
                        Speak = true,
                        Locked = false,
                        Speed = 20,
                        Name=new List<string>()
                    },
                    MemoryModeSettings = new MemoryModeSettings
                    {
                        Speak = true,
                        Locked = false,
                        Speed = 20,
                    },

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
                    InitializationAll.InitializationData();
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
            try
            {
                Directory.Delete(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary"),true);
            }
            catch
            {
                
            }
            LogManager.Info("程序启动");
            MainWindow mainWindow = new MainWindow();
              
        }

        
    }
}
