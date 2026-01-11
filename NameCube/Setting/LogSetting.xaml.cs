using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace NameCube.Setting
{
    /// <summary>
    /// LogSetting.xaml 的交互逻辑
    /// </summary>
    public partial class LogSetting : Page
    {
        private static readonly ILogger _logger = Log.ForContext<LogSetting>();
        bool canChange;
        public LogSetting()
        {
            InitializeComponent();
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("打开日志文件夹");
            Process.Start(System.IO.Path.Combine(GlobalVariables.configDir, "logs"));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _logger.Debug("LogSetting 开始初始化");
            canChange = false;
            LogLevelComboBox.SelectedIndex = GlobalVariables.json.AllSettings.LogLevel;
            canChange = true;
            _logger.Debug("LogSetting 页面加载完成");
        }

        private void LogLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!canChange) return;
            GlobalVariables.json.AllSettings.LogLevel=LogLevelComboBox.SelectedIndex;
            InitializeSerilogAgain();
            GlobalVariables.SaveJson();
            _logger.Information("日志级别已修改：{level}",GlobalVariables.json.AllSettings.LogLevel);
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
