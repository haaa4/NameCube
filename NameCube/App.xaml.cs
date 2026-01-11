using Microsoft.Toolkit.Uwp.Notifications;
using NameCube.Setting;
using NameCube.WarningWindows;
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
using Serilog;

namespace NameCube
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : System.Windows.Application
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        protected override void OnStartup(StartupEventArgs e)
        {
            // 方法1：系统DPI感知
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
                Log.Information("已启用DPI感知");
            }
            base.OnStartup(e);
        }
        
        Mutex mutex;

        public App()
        {
            this.Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // 初始化Serilog日志
                InitializeSerilog();
                
                Log.Information("应用程序启动开始");
                Log.Debug("启动参数: {@Args}", e.Args);

                GlobalExceptionHandler.Initialize();
                
                System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (sende, args) =>
                {
                    args.SetObserved(); 
                    Log.Error(args.Exception, "未观察到的任务异常");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        new ErrorWindow(args.Exception).ShowDialog();
                    });
                };
                
                bool ret;
                mutex = new Mutex(true, "NameCube", out ret);
                GlobalVariables.ret = ret;
                
                Log.Debug("Mutex创建结果: {Result}, 是否为首次实例: {IsFirstInstance}", ret, ret);

                if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Namecube")))
                {
                    GlobalVariables.configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Namecube");
                    Log.Information("使用应用程序目录下的配置目录: {ConfigDir}", GlobalVariables.configDir);
                }
                else
                {
                    GlobalVariables.configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NameCube");
                    Log.Information("使用AppData配置目录: {ConfigDir}", GlobalVariables.configDir);
                }

                if (!ret && !File.Exists(Path.Combine(GlobalVariables.configDir, "START")))
                {
                    Log.Warning("应用程序重复启动且无START文件，显示重复警告窗口");
                    RepeatWarning repeat = new RepeatWarning();
                    repeat.Show();
                    repeat.Activate();
                    repeat.WindowState = WindowState.Normal;
                    return;
                }
                
                // 处理更新目录
                if(Directory.Exists(Path.Combine(GlobalVariables.configDir, "Updata")))
                {
                    Log.Information("检测到更新目录，处理更新后清理");
                    
                    if(File.Exists(Path.Combine(GlobalVariables.configDir, "UpdataZip.zip")))
                    {
                        File.Delete(Path.Combine(GlobalVariables.configDir, "UpdataZip.zip"));
                        Log.Debug("删除旧的更新压缩包");
                    }
                    
                    if(File.Exists(Path.Combine(GlobalVariables.configDir, "Updata", "Success")))
                    {
                        string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                        Log.Information("更新成功，显示欢迎通知。用户: {Username}, 版本: {Version}", username, GlobalVariables.Version);
                        
                        new ToastContentBuilder()
                            .AddArgument("action", "viewConversation")
                            .AddArgument("conversationId", 9813)
                            .AddText("点鸣魔方")
                            .AddText("点鸣魔方已经升级到" + GlobalVariables.Version + "。" + username + "，欢迎")
                            .Show();
                    }
                    else
                    {
                        Log.Warning("更新未成功完成，显示错误通知");
                        new ToastContentBuilder()
                            .AddArgument("action", "viewConversation")
                            .AddArgument("conversationId", 9813)
                            .AddText("点鸣魔方")
                            .AddText("当前版本是：" + GlobalVariables.Version + "。升级遇到问题？尝试去Github项目询问")
                            .Show();
                    }
                    
                    try
                    {
                        Directory.Delete(Path.Combine(GlobalVariables.configDir, "Updata"), true);
                        Log.Information("清理更新目录成功");
                    }
                    catch(Exception ex)
                    {
                        Log.Error(ex, "清理更新目录失败");
                        MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    }
                }

                string configPath = Path.Combine(GlobalVariables.configDir, "config.json");
                
                // 删除旧的日志目录引用，使用Serilog
                // LogManager.LogDirectory = Path.Combine(GlobalVariables.configDir, "logs"); // 删除此行

                if (File.Exists(Path.Combine(GlobalVariables.configDir, "START")))
                {
                    File.Delete(Path.Combine(GlobalVariables.configDir, "START"));
                    Log.Debug("删除START标记文件");
                }
                
                try
                {
                    // 确保目录存在
                    Directory.CreateDirectory(GlobalVariables.configDir);
                    Log.Debug("确保配置目录存在: {ConfigDir}", GlobalVariables.configDir);
                    
                    InitializationAll.InitializeData();
                    
                    if (!File.Exists(configPath))
                    {
                        Log.Warning("找不到配置文件，启动首次使用向导");
                        GlobalVariables.ret = false;
                        
                        // 初始化默认配置
                        FirstUse.FirstUseWindow firstUseWindow = new FirstUse.FirstUseWindow();
                        firstUseWindow.Show();
                        firstUseWindow.Activate();
                        firstUseWindow.WindowState = WindowState.Normal;
                        return; // 确保保存成功
                    }

                    // 再次检查文件是否存在（防止异步问题）
                    if (File.Exists(configPath))
                    {
                        Log.Debug("加载配置文件: {ConfigPath}", configPath);
                        var jsonString = File.ReadAllText(configPath);
                        
                        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            ObjectCreationHandling = ObjectCreationHandling.Replace,
                            DefaultValueHandling = DefaultValueHandling.Populate
                        };
                        
                        GlobalVariables.json = JsonConvert.DeserializeObject<Json>(jsonString);
                        InitializationAll.KeepDataNotNull();
                        Log.Information("配置文件加载成功");
                    }
                    else
                    {
                        var errorMsg = "配置文件未找到且初始化失败。";
                        Log.Error(errorMsg);
                        throw new FileNotFoundException(errorMsg);
                    }
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "应用程序启动失败");
                    MessageBoxFunction.ShowMessageBoxError($"启动失败: {ex.Message}");
                    Environment.Exit(1);
                }
                
                try
                {
                    string tempDir = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary");
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                        Log.Debug("清理临时目录: {TempDir}", tempDir);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "清理临时目录失败");
                }
                
                StartToDoSomething.GetUpdata();
                Log.Debug("执行GetUpdata完成");
                
                StartToDoSomething.RunAutomaticProcesses();
                Log.Debug("执行RunAutomaticProcesses完成");
                
                // 记录系统信息
                Log.Information("应用程序启动信息:" +
                    $"\n操作系统版本: {Environment.OSVersion}" +
                    $"\n平台类型: {Environment.OSVersion.Platform}" +
                    $"\n是否为64位系统: {Environment.Is64BitOperatingSystem}" +
                    $"\n当前进程是否为64位: {Environment.Is64BitProcess}" +
                    $"\n计算机名: {Environment.MachineName}" +
                    $"\n当前用户名: {Environment.UserName}" +
                    $"\n用户域: {Environment.UserDomainName}" +
                    $"\n处理器核心数: {Environment.ProcessorCount}" +
                    $"\n系统目录: {Environment.SystemDirectory}" +
                    $"\n当前目录: {Environment.CurrentDirectory}" +
                    $"\n进程工作集内存: {Environment.WorkingSet}" +
                    $"\n应用版本: {GlobalVariables.Version}");
                
                MainWindow mainWindow = new MainWindow();
                if (GlobalVariables.json.AllSettings.NameCubeMode != 0)
                {
                    Log.Debug("NameCube模式不为0，显示主窗口");
                    mainWindow.ShowThisWindow();
                }
                InitializeSerilogAgain();
                Notify notify = new Notify();
                Log.Information("应用程序启动完成");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "应用程序启动过程中发生未捕获的异常");
                MessageBoxFunction.ShowMessageBoxError($"应用程序启动失败: {ex.Message}");
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// 初始化Serilog日志配置
        /// </summary>
        private void InitializeSerilog()
        {
            try
            {
                string logDirectory = Path.Combine(GlobalVariables.configDir, "logs");
                string logFilePath = Path.Combine(logDirectory, "NameCube-.log");

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information() // 提升最低日志级别，减少Debug日志
                    .Enrich.FromLogContext()
                    // 使用异步文件写入，避免阻塞主线程
                    .WriteTo.Async(a => a.File(
                        logFilePath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7, // 减少保留天数
                        outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(5))) // 每5秒刷新到磁盘
                    .WriteTo.Console(
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning, // 控制台只显示警告及以上
                        outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}")
                    .CreateLogger();

                Log.Information("Serilog日志系统初始化完成，日志目录: {LogDirectory}", logDirectory);
            }
            catch (Exception ex)
            {
                // 如果日志初始化失败，使用简化的日志配置
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Warning()
                    .WriteTo.Console()
                    .CreateLogger();

                Log.Error(ex, "Serilog日志初始化失败，使用控制台后备日志");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("应用程序退出，退出代码: {ExitCode}", e.ApplicationExitCode);
            
            // 确保日志被刷新
            Log.CloseAndFlush();
            base.OnExit(e);
        }
        /// <summary>
        /// 重新按照用户配置初始化Serilog日志配置
        /// </summary>
        public void InitializeSerilogAgain()
        {
            string logDirectory = Path.Combine(GlobalVariables.configDir, "logs");
            string logFilePath = Path.Combine(logDirectory, "NameCube-.log");
            switch (GlobalVariables.json.AllSettings.LogLevel)
            {
                case 0://调试级别
                    Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Debug()
                   .Enrich.FromLogContext()
                   // 使用异步文件写入，避免阻塞主线程
                   .WriteTo.Async(a => a.File(
                       logFilePath,
                       rollingInterval: RollingInterval.Day,
                       retainedFileCountLimit: 7, // 减少保留天数
                       outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                       shared: true,
                       flushToDiskInterval: TimeSpan.FromSeconds(5))) // 每5秒刷新到磁盘
                   .WriteTo.Console(
                       restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning, // 控制台只显示警告及以上
                       outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}")
                   .CreateLogger();
                    break;
                case 1://调试级别
                    Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Information()
                   .Enrich.FromLogContext()
                   // 使用异步文件写入，避免阻塞主线程
                   .WriteTo.Async(a => a.File(
                       logFilePath,
                       rollingInterval: RollingInterval.Day,
                       retainedFileCountLimit: 7, // 减少保留天数
                       outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",

                       shared: true,
                       flushToDiskInterval: TimeSpan.FromSeconds(5))) // 每5秒刷新到磁盘
                   .WriteTo.Console(
                       restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning, // 控制台只显示警告及以上
                       outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}")
                   .CreateLogger();
                    break;
                case 2://警告级别
                    Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Warning()
                   .Enrich.FromLogContext()
                   // 使用异步文件写入，避免阻塞主线程
                   .WriteTo.Async(a => a.File(
                       logFilePath,
                       rollingInterval: RollingInterval.Day,
                       retainedFileCountLimit: 7, // 减少保留天数
                       outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",

                       shared: true,
                       flushToDiskInterval: TimeSpan.FromSeconds(5))) // 每5秒刷新到磁盘
                   .WriteTo.Console(
                       restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning, // 控制台只显示警告及以上
                       outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}")
                   .CreateLogger();
                    break;
                case 3://错误级别
                    Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Error()
                   .Enrich.FromLogContext()
                   // 使用异步文件写入，避免阻塞主线程
                   .WriteTo.Async(a => a.File(
                       logFilePath,
                       rollingInterval: RollingInterval.Day,
                       retainedFileCountLimit: 7, // 减少保留天数
                       outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                       shared: true,
                       flushToDiskInterval: TimeSpan.FromSeconds(5))) // 每5秒刷新到磁盘
                   .WriteTo.Console(
                       restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning, // 控制台只显示警告及以上
                       outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}")
                   .CreateLogger();
                    break;
                default:
                    //什么也不做
                    break;
            }
            Log.Debug("Serilog二次初始化成功");
        }
    }
}