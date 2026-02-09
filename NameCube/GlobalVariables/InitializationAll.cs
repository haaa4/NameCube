using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using NameCube.GlobalVariables.DataClass;

namespace NameCube.GlobalVariables
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
            GlobalVariablesData.config = new Json
            {
                AllSettings = new AllSettings
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
                StartToDo = new StartToDo
                {
                    Ball = false,
                    AlwaysCleanMemory = false,
                    AutoUpdata = true,
                },
                BirdSettings = new BirdSettingsData
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

                OnePeopleModeSettings = new OnePeopleModeSettings
                {
                    Speech = true,
                    Wait = false,
                    Locked = false,
                    Speed = 20,
                },
                MemoryFactorModeSettings = new MemoryFactorModeSettings
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
            if (GlobalVariablesData.config.AllSettings == null)
            {
                GlobalVariablesData.config.AllSettings = new AllSettings();
            }
            if (GlobalVariablesData.config.StartToDo == null)
            {
                GlobalVariablesData.config.StartToDo = new StartToDo();
            }
            if (GlobalVariablesData.config.BirdSettings == null)
            {
                GlobalVariablesData.config.BirdSettings = new BirdSettingsData();
            }
            if (GlobalVariablesData.config.OnePeopleModeSettings == null)
            {
                GlobalVariablesData.config.OnePeopleModeSettings = new OnePeopleModeSettings();
            }
            if (GlobalVariablesData.config.MemoryFactorModeSettings == null)
            {
                GlobalVariablesData.config.MemoryFactorModeSettings = new MemoryFactorModeSettings();
            }
            if (GlobalVariablesData.config.BatchModeSettings == null)
            {
                GlobalVariablesData.config.BatchModeSettings = new BatchModeSettings();
            }
            if (GlobalVariablesData.config.NumberModeSettings == null)
            {
                GlobalVariablesData.config.NumberModeSettings = new NumberModeSettings();
            }
            if (GlobalVariablesData.config.PrepareModeSetting == null)
            {
                GlobalVariablesData.config.PrepareModeSetting = new PrepareModeSetting();
            }
            if (GlobalVariablesData.config.MemoryModeSettings == null)
            {
                GlobalVariablesData.config.MemoryModeSettings = new MemoryModeSettings();
            }
            if (GlobalVariablesData.config.AllSettings.color == null)
            {
                GlobalVariablesData.config.AllSettings.color = (Brush)
                    new BrushConverter().ConvertFromInvariantString("#30d7d7");
            }
            if (GlobalVariablesData.config.BirdSettings.diaphaneity == 0)
            {
                GlobalVariablesData.config.BirdSettings.diaphaneity = 100;
            }
            //if (GlobalVariablesData.config.AllSettings.Font == null)
            //{
            //    GlobalVariablesData.config.AllSettings.Font = new FontFamily("Arial");
            //}
            if (GlobalVariablesData.config.OnePeopleModeSettings.Speed == 0)
            {
                GlobalVariablesData.config.OnePeopleModeSettings.Speed = 20;
            }
            if (GlobalVariablesData.config.MemoryFactorModeSettings.Speed == 0)
            {
                GlobalVariablesData.config.MemoryFactorModeSettings.Speed = 20;
            }
            if (GlobalVariablesData.config.NumberModeSettings.Speed == 0)
            {
                GlobalVariablesData.config.NumberModeSettings.Speed = 20;
            }
            if (GlobalVariablesData.config.PrepareModeSetting.Speed == 0)
            {
                GlobalVariablesData.config.PrepareModeSetting.Speed = 20;
            }
            if (GlobalVariablesData.config.MemoryModeSettings.Speed == 0)
            {
                GlobalVariablesData.config.MemoryModeSettings.Speed = 20;
            }
            if (GlobalVariablesData.config.ShortCutKey == null)
            {
                GlobalVariablesData.config.ShortCutKey = new ShortCutKey();
            }
            if (GlobalVariablesData.config.ShortCutKey.keysGrounp == null)
            {
                GlobalVariablesData.config.ShortCutKey.keysGrounp = new List<ShortCut>();
            }
            if (GlobalVariablesData.config.AutomaticProcess == null)
            {
                GlobalVariablesData.config.AutomaticProcess = new AutomaticProcess();
            }
            //未来的开发请注意！！！！
            GlobalVariablesData.config.AllSettings.LowMemoryMode = false;
        }
    }
}
