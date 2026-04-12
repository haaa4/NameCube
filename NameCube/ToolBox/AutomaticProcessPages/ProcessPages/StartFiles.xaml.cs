using NameCube.Function;
using Serilog;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessPages
{
    /// <summary>
    /// StartFiles.xaml 的交互逻辑
    /// </summary>
    public partial class StartFiles : Page
    {
        private string pathToStart;
        private bool debug;

        public event Action<string> RequestParentAction;

        public event Action<int> EndThePageAction;

        private void CallEndThePage(int ret = 0)
        {
            Log.Debug("调用结束页面，返回码: {ReturnCode}", ret);
            EndThePageAction?.Invoke(ret);
        }

        private void CallParentMethodDebug(string data)
        {
            Log.Debug("调用父页面调试方法，数据: {Data}", data);
            RequestParentAction?.Invoke(data);
        }

        public StartFiles(string path, bool isDebug = false, bool show = true)
        {
            Log.Information("初始化启动文件页面 - 路径: {Path}, 调试模式: {Debug}, 显示: {Show}",
                path, isDebug, show);
            InitializeComponent();
            pathToStart = path;
            debug = isDebug;
            if (!show)
            {
                Log.Debug("页面设置为不显示模式，自动触发加载");
                Page_Loaded(null, null);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Information("开始启动文件: {Path}", pathToStart);
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = pathToStart,
<<<<<<< HEAD
                    UseShellExecute = true 
=======
                    UseShellExecute = true
>>>>>>> c69be5c4950bc482a4a0fd3c6e85e97a8d570b2d
                };
                Process.Start(psi);
                Log.Information("文件启动成功");
                if (debug)
                {
                    CallParentMethodDebug("启动文件成功");
                }
                else
                {
                    CallEndThePage();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "启动文件失败");
                MessageBoxFunction.ShowMessageBoxError(ex.Message, true, ex);
                if (debug)
                {
                    CallParentMethodDebug($"启动文件失败: {ex.Message}");
                }
                else
                {
                    //无视风险，继续运行！（bushi
                    Log.Warning("启动文件失败，但继续执行后续流程");
                    CallEndThePage();
                }
            }
        }
    }
}