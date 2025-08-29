using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Masuit.Tools.Logging;
using NameCube.Mode;
using NameCube.Setting;
using Newtonsoft.Json;
using Wpf.Ui.Controls;
using Brush = System.Windows.Media.Brush;
using FontFamily = System.Windows.Media.FontFamily;
using Path = System.IO.Path;

/*
  This is all fucking shit!!
            ▃▆█▇▄▖
▟◤▖　　　◥█▎
   ◢◤　 ▐　　　 　▐▉
▗◤　　　▂　▗▖　　▕█▎
◤　▗▅▖◥▄　▀◣　　█▊
▐　▕▎◥▖◣◤　　　　◢██
█◣　◥▅█▀　　　　▐██◤
▐█▙▂　　     　◢██◤
◥██◣　　　　◢▄◤
 ▀██▅▇▀

 */
namespace NameCube
{
    /// <summary>
    /// 重置数据的类
    /// </summary>
    internal class InitializationAll
    {
        /// <summary>
        /// 初始化数据
        /// </summary>
        public static void InitializeData()
        {
            GlobalVariables.json = new Json
            {
                AllSettings = new allSettings
                {
                    Name = new List<string> { "张三", "李四", "王五" },
                    Dark = false,
                    Volume = 100,
                    Speed = 0,
                    SystemSpeech = false,
                    Top = true,
                    NameCubeMode = 0,
                    color = (Brush)new BrushConverter().ConvertFromInvariantString("#30d7d7"),
                    UpdataGet = 0,
                },
                StartToDo = new startToDo
                {
                    Ball = false,
                    AlwaysCleanMemory = false,
                    AutoUpdata = true,
                },
                BirdSettings = new BirdSettings
                {
                    StartWay = 4,
                    UseDefinedImage = false,
                    AdsorbValue = 60,
                    AutoAbsord = false,
                    diaphaneity = 0,
                    StartLocationWay = 0,
                    StartLocationX = 0,
                    StartLocationY = 0,
                    Width = 50,
                    Height = 50,
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
                    Locked = false,
                },
                NumberModeSettings = new NumberModeSettings
                {
                    Num = 53,
                    Speak = true,
                    Locked = false,
                    Speed = 20,
                },
                PrepareModeSetting = new PrepareModeSetting
                {
                    Speak = true,
                    Locked = false,
                    Speed = 20,
                    Name = new List<string>(),
                },
                MemoryModeSettings = new MemoryModeSettings
                {
                    Speak = true,
                    Locked = false,
                    Speed = 20,
                    AutoAddFile = true,
                },
                ShortCutKey = new ShortCutKey { keysGrounp = new List<ShortCut>() },
            };
        }

        /// <summary>
        /// 保证数据非null
        /// </summary>
        public static void KeepDataNotNull()
        {
            if (GlobalVariables.json.AllSettings == null)
            {
                GlobalVariables.json.AllSettings = new allSettings();
            }
            if (GlobalVariables.json.StartToDo == null)
            {
                GlobalVariables.json.StartToDo = new startToDo();
            }
            if (GlobalVariables.json.BirdSettings == null)
            {
                GlobalVariables.json.BirdSettings = new BirdSettings();
            }
            if (GlobalVariables.json.OnePeopleModeSettings == null)
            {
                GlobalVariables.json.OnePeopleModeSettings = new onePeopleModeSettings();
            }
            if (GlobalVariables.json.MemoryFactorModeSettings == null)
            {
                GlobalVariables.json.MemoryFactorModeSettings = new memoryFactorModeSettings();
            }
            if (GlobalVariables.json.BatchModeSettings == null)
            {
                GlobalVariables.json.BatchModeSettings = new BatchModeSettings();
            }
            if (GlobalVariables.json.NumberModeSettings == null)
            {
                GlobalVariables.json.NumberModeSettings = new NumberModeSettings();
            }
            if (GlobalVariables.json.PrepareModeSetting == null)
            {
                GlobalVariables.json.PrepareModeSetting = new PrepareModeSetting();
            }
            if (GlobalVariables.json.MemoryModeSettings == null)
            {
                GlobalVariables.json.MemoryModeSettings = new MemoryModeSettings();
            }
            if (GlobalVariables.json.AllSettings.color == null)
            {
                GlobalVariables.json.AllSettings.color = (Brush)
                    new BrushConverter().ConvertFromInvariantString("#30d7d7");
            }
            if (GlobalVariables.json.BirdSettings.diaphaneity == 0)
            {
                GlobalVariables.json.BirdSettings.diaphaneity = 100;
            }
            if (GlobalVariables.json.AllSettings.Font == null)
            {
                GlobalVariables.json.AllSettings.Font = new FontFamily("Arial");
            }
            if (GlobalVariables.json.OnePeopleModeSettings.Speed == 0)
            {
                GlobalVariables.json.OnePeopleModeSettings.Speed = 20;
            }
            if (GlobalVariables.json.MemoryFactorModeSettings.Speed == 0)
            {
                GlobalVariables.json.MemoryFactorModeSettings.Speed = 20;
            }
            if (GlobalVariables.json.NumberModeSettings.Speed == 0)
            {
                GlobalVariables.json.NumberModeSettings.Speed = 20;
            }
            if (GlobalVariables.json.PrepareModeSetting.Speed == 0)
            {
                GlobalVariables.json.PrepareModeSetting.Speed = 20;
            }
            if (GlobalVariables.json.MemoryModeSettings.Speed == 0)
            {
                GlobalVariables.json.MemoryModeSettings.Speed = 20;
            }
            if (GlobalVariables.json.ShortCutKey == null)
            {
                GlobalVariables.json.ShortCutKey = new ShortCutKey();
            }
            if (GlobalVariables.json.ShortCutKey.keysGrounp == null)
            {
                GlobalVariables.json.ShortCutKey.keysGrounp = new List<ShortCut>();
            }
            if (GlobalVariables.json.automaticProcess == null)
            {
                GlobalVariables.json.automaticProcess = new AutomaticProcess();
            }
            //未来的开发请注意！！！！
            GlobalVariables.json.AllSettings.LowMemoryMode = false;
        }
    }

    public class allSettings
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
            (Brush)new BrushConverter().ConvertFromInvariantString("#30d7d7");

        /// <summary>
        /// 字体类型
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public FontFamily Font { get; set; } = new FontFamily("Arial");

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

        /// <summary>
        /// 上一次抽取的姓名
        /// </summary>
        public string LastName { get; set; }
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
        /// 自动请求升级
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AutoUpdata { get; set; } = true;
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

        /// <summary>
        /// 保底人姓名
        /// </summary>
        public string MaxName { get; set; }

        /// <summary>
        /// 保底人重复次数
        /// </summary>
        public int MaxTimes { get; set; }

        /// <summary>
        /// 上一次抽取的姓名
        /// </summary>
        public string LastName { get; set; }
    }

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

    public class NumberModeSettings
    {
        /// <summary>
        /// 用户之前的数字
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Num { get; set; } = 53;

        /// <summary>
        /// 是否启用朗读
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Speak { get; set; } = true;

        /// <summary>
        /// 是否允许修改
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Locked { set; get; } = false;

        /// <summary>
        /// 跳动速度
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Speed { get; set; } = 20;

        /// <summary>
        /// 上一次抽取的数字
        /// </summary>
        public string LastName { get; set; }
    }

    public class PrepareModeSetting
    {
        /// <summary>
        /// 是否启用朗读
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Speak { get; set; } = true;

        /// <summary>
        /// 是否允许修改
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Locked { set; get; } = false;

        /// <summary>
        /// 跳动速度
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int Speed { get; set; } = 20;

        /// <summary>
        /// 上次的准备列表
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> Name { get; set; } = new List<string>();

        /// <summary>
        /// 上一次抽取的姓名
        /// </summary>
        public string LastName { get; set; }
    }

    public class ShortCut
    {
        /// <summary>
        /// 快捷键
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<Key> keys { get; set; } = new List<Key>();

        /// <summary>
        /// 启动方式(0=无,1=单人模式,2=因子模式,3=批量模式,4=数字模式,5=预备模式,6=记忆模式,7=主页)
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int openWay { get; set; } = -1;
        /// <summary>
        /// 触发的自动流程(为null时则以openway为准)
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ProcessGroup ProcessGroup { get; set; } = null;

        /// <summary>
        /// 上一次修改时间
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string LastChangeTime { get; set; } = "-1";
    }

    public class MemoryModeSettings
    {
        /// <summary>
        /// 启用朗读
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool Speak { set; get; } = true;

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

        /// <summary>
        /// 自动增加临时名单
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AutoAddFile { get; set; } = true;

        /// <summary>
        /// 上一次抽取的姓名
        /// </summary>
        public string LastName { get; set; }
    }

    public class ShortCutKey
    {
        /// <summary>
        /// 快捷键组合
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<ShortCut> keysGrounp = new List<ShortCut>();
    }

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
        public Dictionary<int,List<ProcessGroup>> processesSchedule { get; set; } = new Dictionary<int,List<ProcessGroup>>();
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

    public class Json
    {
        /// <summary>
        /// 全局设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public allSettings AllSettings { get; set; } = new allSettings();

        /// <summary>
        /// 单人模式设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public onePeopleModeSettings OnePeopleModeSettings { get; set; } =
            new onePeopleModeSettings();

        /// <summary>
        /// 悬浮球设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public BirdSettings BirdSettings { get; set; } = new BirdSettings();

        /// <summary>
        /// 启动设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public startToDo StartToDo { get; set; } = new startToDo();

        /// <summary>
        /// 记忆因子模式设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public memoryFactorModeSettings MemoryFactorModeSettings { get; set; } =
            new memoryFactorModeSettings();

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
        public AutomaticProcess automaticProcess { get; set; } = new AutomaticProcess();
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
        public static string configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NameCube"
        );

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
                MessageBoxFunction.ShowMessageBoxWarning($"保存配置失败: {ex.Message}");
                LogManager.Error(ex);
            }
        }

        /// <summary>
        /// 当前版本
        /// </summary>
        //Bro为什么这样写！！？
        public static string Version = "V1.1";
        public static bool IsBeta = false;
        public static bool ret = false;
    }

    internal class MessageBoxFunction
    {
        /// <summary>
        /// 创建一个提示信息框
        /// </summary>
        /// <param name="message">提醒消息</param>
        public static void ShowMessageBoxInfo(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Content = message;
                messageBox.Title = "提示";
                messageBox.CloseButtonText = "知道了";
                messageBox.ShowDialogAsync();
            });
        }

        /// <summary>
        /// 创建一个警告信息框
        /// </summary>
        /// <param name="message">警告消息</param>
        public static void ShowMessageBoxWarning(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Content = message;
                messageBox.Title = "警告";
                messageBox.CloseButtonText = "知道了";
                messageBox.ShowDialogAsync();
            });
        }

        /// <summary>
        /// 创建一个报错信息框
        /// </summary>
        /// <param name="message">报错信息</param>
        /// <param name="Log">是否写入日志</param
        /// <param name="exception">输入日志的报错（可选）</param>
        public static void ShowMessageBoxError(
            string message,
            bool Log = true,
            Exception exception = null
        )
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Content = message;
                if (Log)
                {
                    if (exception != null)
                    {
                        LogManager.Error("严重错误", exception);
                    }
                    else
                    {
                        LogManager.Error("严重错误", message);
                    }
                }
                messageBox.Title = "错误";
                messageBox.CloseButtonText = "知道了";
                messageBox.ShowDialogAsync();
            });
        }
    }

    internal class AppFunction
    {
        public static void Restart()
        {
            string[] args = Environment.GetCommandLineArgs();
            File.WriteAllText(
                Path.Combine(GlobalVariables.configDir, "START"),
                "The cake is a lie"
            );
            Application.Current.Shutdown();
            Process.Start(Application.ResourceAssembly.Location, string.Join(" ", args.Skip(1)));
        }
    }
}
