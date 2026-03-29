using Masuit.Tools.Files;
using NameCube.Function;
using NameCube.Setting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using Wpf.Ui.Controls;
using Application = System.Windows.Application;
using Path = System.IO.Path;

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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "从数据备份包导入";
            openFileDialog.Filter = "数据备份包(*.dataBackup)|*.dataBackup|压缩包（<v1.3)(*.zip)|*.zip|所有文件 (*.*)|*.*";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Log.Information("开始从压缩包恢复配置: {FilePath}", openFileDialog.FileName);
                SnackBarFunction.ShowSnackBarInSettingWindow("正在保存，请稍等", ControlAppearance.Primary, null, 1);

                Task.Run(() =>
                {
                    try
                    {
                        Directory.Delete(GlobalVariablesData.userDataDir, true);
                        Directory.CreateDirectory(GlobalVariablesData.userDataDir);
                        var SevenZipCompressor = new SevenZipCompressor(null);
                        SevenZipCompressor.Decompress(openFileDialog.FileName, GlobalVariablesData.userDataDir);
                        File.Copy(Path.Combine(GlobalVariablesData.userDataDir, "config_backup.json"), Path.Combine(GlobalVariablesData.configDir, "config.json"), true);
                        File.Delete(Path.Combine(GlobalVariablesData.userDataDir, "config_backup.json"));
                        MessageBoxFunction.ShowMessageBoxInfo("覆盖成功，请自行启动软件");
                        Log.Information("配置恢复成功，程序即将重启");

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            Log.Information("程序退出");
                            Application.Current.Shutdown();
                        }));
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "从压缩包恢复配置时发生异常");
                        MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    }
                });
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