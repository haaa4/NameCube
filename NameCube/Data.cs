using Masuit.Tools.Logging;
using NameCube.Mode;
using NameCube.Setting;
using Newtonsoft.Json;
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
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace NameCube
{
    /// <summary>
    /// 重置数据的类
    /// </summary>
    internal class InitializationAll
    {
        /// <summary>
        /// 重置数据
        /// </summary>
        public static void InitializationData()
        {
            if(GlobalVariables.json.AllSettings==null)
            {
                GlobalVariables.json.AllSettings=new allSettings();
            }
            if(GlobalVariables.json.StartToDo==null)
            {
                GlobalVariables.json.StartToDo=new startToDo();
            }
            if(GlobalVariables.json.BirdSettings==null)
            {
                GlobalVariables.json.BirdSettings=new BirdSettings();
            }
            if(GlobalVariables.json.OnePeopleModeSettings==null)
            {
                GlobalVariables.json.OnePeopleModeSettings=new onePeopleModeSettings();
            }
            if(GlobalVariables.json.MemoryFactorModeSettings==null)
            {
                GlobalVariables.json.MemoryFactorModeSettings=new memoryFactorModeSettings();
            }
            if (GlobalVariables.json.BatchModeSettings == null)
            {
                GlobalVariables.json.BatchModeSettings=new BatchModeSettings();
            }
            if(GlobalVariables.json.NumberModeSettings==null)
            {
                GlobalVariables.json.NumberModeSettings=new NumberModeSettings();
            }
            if (GlobalVariables.json.PrepareModeSetting == null)
            {
                GlobalVariables.json.PrepareModeSetting = new PrepareModeSetting(); 
            }
            if (GlobalVariables.json.MemoryModeSettings==null)
            {
                GlobalVariables.json.MemoryModeSettings = new MemoryModeSettings();
            }
        }
    }
    public class allSettings
    {
        /// <summary>
        /// 姓名表
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public List<string> Name { get; set; } = new List<string> { "张三" };
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
        /// <summary>
        /// 学号魔方模式（0:长期运行模式 1：单次运行模式）
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public int NameCubeMode { get; set; } = 0;
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
        [JsonProperty (DefaultValueHandling = DefaultValueHandling.Populate)]
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
        public List<string> Name { get; set; }= new List<string>();
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
        public onePeopleModeSettings OnePeopleModeSettings { get; set; } = new onePeopleModeSettings();
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
        public memoryFactorModeSettings MemoryFactorModeSettings { get; set; } = new memoryFactorModeSettings();
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
        public PrepareModeSetting PrepareModeSetting { get; set; }=new PrepareModeSetting();
        /// <summary>
        /// 记忆模式设置
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public MemoryModeSettings MemoryModeSettings { get; set; }=new MemoryModeSettings();



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
                MessageBoxFunction.ShowMessageBoxWarning($"保存配置失败: {ex.Message}");
                LogManager.Error(ex);
            }
        }
        /// <summary>
        /// 当前版本
        /// </summary>
        public static string Version = "V1.0.0 Beta-1";
    }
    internal class MessageBoxFunction
    {
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

        public static void ShowMessageBoxError(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Content = message;
                LogManager.Error("严重错误", message);
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
            File.WriteAllText(Path.Combine(GlobalVariables.configDir, "START"), "The cake is a lie");
            Application.Current.Shutdown();
            Process.Start(Application.ResourceAssembly.Location, string.Join(" ", args.Skip(1)));
        }
    }

}
