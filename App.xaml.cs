using Masuit.Tools.Logging;
using NameCube.Setting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
        public List<string> Name { get; set; }
        /// <summary>
        /// 黑暗模式
        /// </summary>
        public bool Dark { get; set; }
        /// <summary>
        /// 朗读音量
        /// </summary>
        public int Volume { get; set; }
        /// <summary>
        /// 朗读速度
        /// </summary>
        public int Speed { get; set; }
        /// <summary>
        /// 开机自启动
        /// </summary>
        public bool Start { get; set; }
        /// <summary>
        /// 使用系统设置的朗读
        /// </summary>
        public bool SystemSpeech { get; set; }
        /// <summary>
        /// 主窗口始终置顶
        /// </summary>
        public bool Top { get; set; }
    }
    public class onePeopleModeSettings
    {
        /// <summary>
        /// 启用朗读人
        /// </summary>
        public bool Speech { get; set; }
        /// <summary>
        /// 禁用等待
        /// </summary>
        public bool Wait { get; set; }
        /// <summary>
        /// 是否允许修改
        /// </summary>
        public bool Locked { get; set; }
        /// <summary>
        /// 主界面字体跳动速度
        /// </summary>
        public int Speed { get; set; }

    }
    public class startToDo
    {
        /// <summary>
        /// 启用朗读球
        /// </summary>
        public bool Ball { get; set; }
        /// <summary>
        /// 自动内存清理
        /// </summary>
        public bool AlwaysCleanMemory { get; set; }
    }
    public class memoryFactorModeSettings
    {
        /// <summary>
        /// 启用朗读
        /// </summary>
        public bool Speech { set; get; }
        /// <summary>
        /// 是否允许修改
        /// </summary>
        public bool Locked { get; set; }
        /// <summary>
        /// 主界面字体跳动速度
        /// </summary>
        public int Speed { get; set; }
    }
    public class BatchModeSettings
    {
        /// <summary>
        /// 是否为数字模式
        /// </summary>
        public bool NumberMode { get; set; }
        /// <summary>
        /// 数字数量
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// 抽取数量
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 允许重复
        /// </summary>
        public bool Repetition { get; set; }
    }


    public class Json
    {
        /// <summary>
        /// 全局设置
        /// </summary>
        public allSettings AllSettings { get; set; }
        /// <summary>
        /// 单人模式设置
        /// </summary>

        public onePeopleModeSettings OnePeopleModeSettings { get; set; }
        /// <summary>
        /// 启动设置
        /// </summary>
        public startToDo StartToDo { get; set; }
        /// <summary>
        /// 记忆因子模式设置
        /// </summary>
        public memoryFactorModeSettings MemoryFactorModeSettings { get; set; }
        /// <summary>
        /// 批量模式设置
        /// </summary>
        public BatchModeSettings BatchModeSettings { get; set; }
        /// <summary>
        /// 当前版本
        /// </summary>
        public string Version="Alpha-5" ;


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

                string jsonString = JsonSerializer.Serialize(json);
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
            if (!ret)
            {
                System.Windows.MessageBox.Show("哥们，你已经启动一个实例了,看看系统托盘吧（笑 \n" +
                    "翻译：你已经启动了软件");
                Environment.Exit(0);
            }
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configDir = Path.Combine(appDataPath, "NameCube");
            string configPath = Path.Combine(configDir, "config.json");
            LogManager.LogDirectory = Path.Combine(configDir, "logs");

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
                    string jsonString = File.ReadAllText(configPath);
                    GlobalVariables.json = JsonSerializer.Deserialize<Json>(jsonString);
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
            SettingsWindow settingsWindow = new SettingsWindow();
        }
    }
}
