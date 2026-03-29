using NameCube.Function;
using NameCube.ToolBox.AutomaticProcessPages.ProcessPages;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.ToolBox.AutomaticProcessPages
{
    /// <summary>
    /// DebugWindows.xaml 的交互逻辑
    /// </summary>
    public partial class DebugWindows : Window
    {
        public DebugWindows(Page page)
        {
            Log.Information("创建调试窗口");
            try
            {
                InitializeComponent();
                MainFrame.Navigate(page);
                Log.Information("调试窗口初始化完成，页面类型: {PageType}", page?.GetType().Name);

                // 注册页面事件
                if (page is ReadyPage readyPage)
                {
                    readyPage.RequestParentAction += HandleChildRequest;
                    Log.Debug("注册就绪页面事件处理器");
                }
                else if (page is StartFiles startFiles)
                {
                    startFiles.RequestParentAction += HandleChildRequest;
                    Log.Debug("注册启动文件页面事件处理器");
                }
                else if (page is AudioPage audioPage)
                {
                    audioPage.RequestParentAction += HandleChildRequest;
                    Log.Debug("注册音频页面事件处理器");
                }
                else if (page is ReadPage readPage)
                {
                    readPage.RequestParentAction += HandleChildRequest;
                    Log.Debug("注册阅读页面事件处理器");
                }
                else if (page is CmdPage cmdPage)
                {
                    cmdPage.RequestParentAction += HandleChildRequest;
                    Log.Debug("注册CMD页面事件处理器");
                }
                else if (page is WaitPage waitPage)
                {
                    waitPage.RequestParentAction += HandleChildRequest;
                    Log.Debug("注册等待页面事件处理器");
                }
                else if (page is ClearPage clearPage)
                {
                    clearPage.RequestParentAction += HandleChildRequest;
                    Log.Debug("注册清理内存页面事件处理器");
                }
                else if (page is PowerOffPage powerOffPage)
                {
                    powerOffPage.RequestParentAction += HandleChildRequest;
                    Log.Debug("注册关机页面事件处理器");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "创建调试窗口时发生错误");
                throw;
            }
        }

        public void Exit(string data = null)
        {
            Log.Information("调试窗口关闭请求，数据: {Data}", data ?? "无");
            if (data != null)
            {
                MessageBoxFunction.ShowMessageBoxInfo(data);
            }
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => this.Close());
            }
            else
            {
                this.Close();
            }
        }

        private void HandleChildRequest(string data)
        {
            Log.Debug("接收到子页面请求，数据: {Data}", data);
            Exit(data);
        }

        protected override void OnClosed(EventArgs e)
        {
            Log.Information("调试窗口正在关闭");
            if (MainFrame.Content is ReadyPage readyPage)
            {
                readyPage.RequestParentAction -= HandleChildRequest;
                Log.Debug("注销就绪页面事件处理器");
            }
            base.OnClosed(e);
            Log.Information("调试窗口已关闭");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Log.Debug("调试窗口关闭中，清理页面内容");
            //关闭界面
            MainFrame.Content = null;
        }
    }
}