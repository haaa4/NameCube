using Masuit.Tools.Logging;
using NameCube.Setting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

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
            if (!ret && !File.Exists(Path.Combine(GlobalVariables.configDir, "START")))
            {
                RepeatWarning repeat = new RepeatWarning();
                repeat.Show();
                repeat.Activate();
                repeat.WindowState = WindowState.Normal;
                return;
            }
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string configDir = Path.Combine(appDataPath, "NameCube");
            string configPath = Path.Combine(configDir, "config.json");
            LogManager.LogDirectory = Path.Combine(configDir, "logs");
            if (File.Exists(Path.Combine(GlobalVariables.configDir, "START")))
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
                        Top = true,
                        NameCubeMode = 0
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
                        Name = new List<string>()
                    },
                    MemoryModeSettings = new MemoryModeSettings
                    {
                        Speak = true,
                        Locked = false,
                        Speed = 20,
                        AutoAddFile = true,
                    },

                };
                if (!File.Exists(configPath))
                {
                    LogManager.Info("找不到文件");
                    // 初始化默认配置
                    FirstUse.FirstUseWindow firstUseWindow = new FirstUse.FirstUseWindow();
                    firstUseWindow.Show();
                    firstUseWindow.Activate();
                    firstUseWindow.WindowState = WindowState.Normal;
                    return;// 确保保存成功
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
                    InitializationAll.InitializationData();
                }
                else
                {
                    throw new FileNotFoundException("配置文件未找到且初始化失败。");
                }
            }
            catch (Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError($"启动失败: {ex.Message}");
                Environment.Exit(1);
            }
            try
            {
                if (Directory.Exists(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary")))
                {
                    Directory.Delete(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary"), true);
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(ex);
            }
            LogManager.Info("程序启动");
            MainWindow mainWindow = new MainWindow();
            if (GlobalVariables.json.AllSettings.NameCubeMode==0)
            {
                InitializeTrayIcon();
            }
            else
            {
                mainWindow.Show();
            }


        }
        NotifyIcon _notifyIcon;
        private void InitializeTrayIcon()
        {

            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath),
                Visible = true,
                Text = "点鸣魔方"
            };

            // 添加右键菜单
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("显示窗口", null, (s, e) => ShowMainWindowAsync());
            contextMenu.Items.Add("小工具", null, (s, e) => ShowToolboxWindowAsync());
            contextMenu.Items.Add("设置", null, (s, e) => ShowSettingsWindowAsync());
            contextMenu.Items.Add("重启", null, (s, e) => AppFunction.Restart());
            contextMenu.Items.Add("退出", null, (s, e) => ExitApp());
            _notifyIcon.ContextMenuStrip = contextMenu;

            // 双击托盘图标显示窗口
            _notifyIcon.DoubleClick += (s, e) => ShowMainWindowAsync();
        }
        private async Task ShowMainWindowAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Show();
                    mainWindow.Activate();
                    mainWindow.NavigationMenu.Navigate(typeof(Mode.Home));
                }
            });
        }
        private async Task ShowSettingsWindowAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var settingsWindow = Application.Current.Windows.OfType<SettingsWindow>().FirstOrDefault();

                if (settingsWindow == null)
                {
                    // 创建新实例
                    settingsWindow = new SettingsWindow();
                }

                // 确保窗口可见并激活
                settingsWindow.Show();
                settingsWindow.Activate();
                settingsWindow.WindowState = WindowState.Normal;
            });
        }
        private async Task ShowToolboxWindowAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var toolboxWindow = Application.Current.Windows.OfType<ToolBox.ToolboxWindow>().FirstOrDefault();

                if (toolboxWindow == null)
                {
                    // 创建新实例
                    toolboxWindow = new ToolBox.ToolboxWindow();
                }

                // 确保窗口可见并激活
                toolboxWindow.Show();
                toolboxWindow.Activate();
                toolboxWindow.WindowState = WindowState.Normal;
            });
        }
       
        private void ExitApp()
        {
            _notifyIcon.Dispose(); // 清理托盘图标
            Application.Current.Shutdown(); // 手动关闭应用
        }
    }
}
