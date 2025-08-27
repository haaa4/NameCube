using Masuit.Tools.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
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
using System.Windows.Media;
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
            GlobalVariables.ret=ret;
            if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Namecube")))
            {
                GlobalVariables.configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Namecube");
            }
            else
            {
                GlobalVariables.configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NameCube");
            }
            if (!ret && !File.Exists(Path.Combine(GlobalVariables.configDir, "START")))
            {
                RepeatWarning repeat = new RepeatWarning();
                repeat.Show();
                repeat.Activate();
                repeat.WindowState = WindowState.Normal;
                return;
            }
            if(Directory.Exists(Path.Combine(GlobalVariables.configDir,"Updata")))
            {
                if(File.Exists(Path.Combine(GlobalVariables.configDir, "UpdataZip.zip")))
                {
                    File.Delete(Path.Combine(GlobalVariables.configDir, "UpdataZip.zip"));
                }
                if(File.Exists(Path.Combine(GlobalVariables.configDir, "Updata","Success")))
                {
                    string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    new ToastContentBuilder()
                            .AddArgument("action", "viewConversation")
                            .AddArgument("conversationId", 9813)
                            .AddText("点鸣魔方")
                            .AddText("点鸣魔方已经升级到"+GlobalVariables.Version+"。"+username+"，欢迎")
                            .Show();
                }
                else
                {
                    new ToastContentBuilder()
                            .AddArgument("action", "viewConversation")
                            .AddArgument("conversationId", 9813)
                            .AddText("点鸣魔方")
                            .AddText("当前版本是："+ GlobalVariables.Version+"。升级遇到问题？尝试去Github项目询问")
                            .Show();
                }
                try
                {
                    Directory.Delete(Path.Combine(GlobalVariables.configDir, "Updata"),true);
                }
                catch(Exception ex)
                {
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                }
            }
            string configPath = Path.Combine(GlobalVariables.configDir, "config.json");
            LogManager.LogDirectory = Path.Combine(GlobalVariables.configDir, "logs");
            if (File.Exists(Path.Combine(GlobalVariables.configDir, "START")))
            {
                File.Delete(Path.Combine(GlobalVariables.configDir, "START"));
            }
            try
            {
                // 确保目录存在
                Directory.CreateDirectory(GlobalVariables.configDir);
                InitializationAll.InitializeData();
                if (!File.Exists(configPath))
                {
                    LogManager.Info("找不到文件");
                    GlobalVariables.ret = false;
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
                    InitializationAll.KeepDataNotNull();
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
            StartToDoSomething.GetUpdata();
            StartToDoSomething.RunAutomaticProcesses();
            LogManager.Info("程序启动");
            MainWindow mainWindow = new MainWindow();
            if (GlobalVariables.json.AllSettings.NameCubeMode != 0)
            {
                mainWindow.ShowThisWindow();
            }
           Notify notify = new Notify();
        }
       



    }
}
