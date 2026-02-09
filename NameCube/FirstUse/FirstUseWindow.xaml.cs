using Masuit.Tools.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NameCube.Function;
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Controls;
using NameCube.Setting;
using NameCube.FirstUse;
using Application = System.Windows.Application;
using Masuit.Tools.Logging;
using Serilog;  

namespace NameCube.FirstUse
{
    /// <summary>
    /// FirstUseWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FirstUseWindow
    {
        int now = 0;
        public FirstUseWindow()
        {
            InitializeComponent();
            Log.Information("FirstUseWindow初始化");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                now++;
                Log.Information("向导下一步按钮点击，当前步骤: {CurrentStep}", now);

                if (now == 1)
                {
                    Log.Information("加载页面: 选择启动模式");
                    LoadPage("选择启动模式", "在合适的场景使用合适的模式，可以更好地发挥作用", new NameCubeMode());
                }
                else if (now == 2)
                {
                    Log.Information("加载页面: 设置名单");
                    LoadPage("设置名单", "名单是一切点名的开始，如果没有合适名单，就试试默认名单吧", new Archives());
                }
                else if (now == 3)
                {
                    if (GlobalVariablesData.config.AllSettings.NameCubeMode == 0)
                    {
                        Log.Information("加载页面: 悬浮球设置");
                        LoadPage("悬浮球设置", "悬浮球是一个很好的东西，可以更快地启动主窗口", new BirdSettings());
                    }
                    else
                    {
                        Log.Information("加载页面: 完成(非悬浮球模式)");
                        LoadPage("完成！", "所有基本设置均已完成，更多设置请在应用设置里查看", new About());
                        GlobalVariablesData.SaveConfig();
                        Log.Information("向导设置已保存");
                    }
                }
                else if (now == 4)
                {
                    if (GlobalVariablesData.config.AllSettings.NameCubeMode == 0)
                    {
                        Log.Information("加载页面: 其他设置");
                        LoadPage("其他......", "这里还有一些其他设置，也许有些用", new Other());
                    }
                    else
                    {
                        Log.Information("非悬浮球模式向导完成，准备重启应用");
                        AppFunction.Restart();
                    }
                }
                else if (now == 5)
                {
                    Log.Information("加载页面: 完成(悬浮球模式)");
                    LoadPage("完成！", "所有基本设置均已完成，更多设置请在应用设置里查看", new About());
                    GlobalVariablesData.SaveConfig();
                    Log.Information("向导设置已保存");
                }
                else if (now == 6)
                {
                    Log.Information("悬浮球模式向导完成，准备重启应用");
                    AppFunction.Restart();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "向导步骤处理时发生错误");
            }
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Log.Information("FirstUseWindow正在关闭，退出应用");
            Application.Current.Shutdown();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("开始从压缩包导入配置");
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "从压缩包导入";
                openFileDialog.Filter = "压缩包 (*.zip)|*.zip|所有文件 (*.*)|*.*";

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Log.Information("用户选择了压缩包: {FileName}", openFileDialog.FileName);

                    try
                    {
                        if (Directory.Exists(GlobalVariablesData.configDir))
                        {
                            Log.Information("删除现有配置目录: {ConfigDir}", GlobalVariablesData.configDir);
                            Directory.Delete(GlobalVariablesData.configDir, true);
                        }

                        Directory.CreateDirectory(GlobalVariablesData.configDir);
                        Log.Information("创建新的配置目录");

                        var SevenZipCompressor = new SevenZipCompressor(null);
                        Log.Information("开始解压压缩包");
                        SevenZipCompressor.Decompress(openFileDialog.FileName, GlobalVariablesData.configDir);

                        MessageBoxFunction.ShowMessageBoxInfo("添加成功，请自行启动软件");
                        Log.Information("压缩包导入成功，准备退出应用");

                        System.Windows.Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "解压压缩包时发生错误");
                        MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    }
                }
                else
                {
                    Log.Information("用户取消了压缩包选择");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "压缩包导入过程中发生错误");
            }
        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("FirstUseWindow加载完成，开始启动动画");
                var ssb = FindResource("StartStoryBoard") as Storyboard;
                if (ssb != null)
                {
                    ssb.Completed += FirstUseWindow_Completed;
                    ssb.Begin();
                    Log.Debug("启动动画开始播放");
                }
                else
                {
                    Log.Warning("未找到启动动画资源");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载启动动画时发生错误");
            }
        }

        private void FirstUseWindow_Completed(object sender, EventArgs e)
        {
            Log.Debug("启动动画播放完成");
            border.Visibility = Visibility.Collapsed;
        }

        private void LoadPage(string title, string text, Page page)
        {
            try
            {
                Log.Debug("开始加载页面: {Title}", title);
                var loadStoryBoard = FindResource("LoadStoryBoard") as Storyboard;
                LoadBoard.Visibility = Visibility.Visible;

                if (loadStoryBoard != null)
                {
                    loadStoryBoard.Completed += (s, e) => LoadingPage(title, text, page);
                    loadStoryBoard.Begin();
                    Log.Debug("页面加载动画开始播放");
                }
                else
                {
                    Log.Warning("未找到页面加载动画资源，直接加载页面");
                    LoadingPage(title, text, page);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载页面动画时发生错误");
                LoadingPage(title, text, page);
            }
        }

        private void LoadingPage(string title, string text, Page page)
        {
            try
            {
                Log.Debug("导航到页面: {PageType}", page.GetType().Name);
                NavigationMenu.Navigate(page);
                TitleGet.Text = title;
                Text.Text = text;

                var loadedStoryBoard = FindResource("LoadedStoryBoard") as Storyboard;
                if (loadedStoryBoard != null)
                {
                    loadedStoryBoard.Completed += (s, e) => LoadedPage();
                    loadedStoryBoard.Begin();
                    Log.Debug("页面加载完成动画开始播放");
                }
                else
                {
                    Log.Warning("未找到页面加载完成动画资源");
                    LoadedPage();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "导航到页面时发生错误");
            }
        }

        private void LoadedPage()
        {
            Log.Debug("页面加载完成");
            LoadBoard.Visibility = Visibility.Collapsed;
        }
    }
}