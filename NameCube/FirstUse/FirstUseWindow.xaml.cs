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
using Wpf.Ui.Controls;
using Wpf.Ui.Violeta.Controls;
using NameCube.Setting;
using NameCube.FirstUse;
using Application = System.Windows.Application;
using Masuit.Tools.Logging;

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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            now++;
            if (now == 1)
            {
                LoadPage("选择启动模式", "在合适的场景使用合适的模式，可以更好地发挥作用", new NameCubeMode());
            }
            else if (now == 2)
            {
                LoadPage("设置名单", "名单是一切点名的开始，如果没有合适名单，就试试默认名单吧", new Archives());
            }
            else if (now == 3)
            {
                if (GlobalVariables.json.AllSettings.NameCubeMode == 0)
                {
                    LoadPage("悬浮球设置", "悬浮球是一个很好的东西，可以更快地启动主窗口", new BirdSettings());
                }
                else
                {
                    LoadPage("完成！", "所有基本设置均已完成，更多设置请在应用设置里查看", new About());
                    GlobalVariables.SaveJson();
                }
            }
            else if (now == 4)
            {
                if (GlobalVariables.json.AllSettings.NameCubeMode == 0)
                {
                    LoadPage("其他......", "这里还有一些其他设置，也许有些用", new Other());
                }
                else
                {
                    AppFunction.Restart();
                }
            }
            else if (now == 5)
            {
                LoadPage("完成！", "所有基本设置均已完成，更多设置请在应用设置里查看", new About());
                GlobalVariables.SaveJson();
            }
            else if(now ==6)
            {
                AppFunction.Restart();
            }
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "从压缩包导入";
            openFileDialog.Filter = "压缩包 (*.zip)|*.zip|所有文件 (*.*)|*.*";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Directory.Delete(GlobalVariables.configDir, true);
                    Directory.CreateDirectory(GlobalVariables.configDir);
                    var SevenZipCompressor = new SevenZipCompressor(null);
                    SevenZipCompressor.Decompress(openFileDialog.FileName, GlobalVariables.configDir);
                    MessageBoxFunction.ShowMessageBoxInfo("添加成功，请自行启动软件");
                    LogManager.Info("程序退出");
                    System.Windows.Application.Current.Shutdown();
                    
                }
                catch (Exception ex)
                {
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                }

            }


        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var ssb = FindResource("StartStoryBoard") as Storyboard;
            if (ssb != null)
            {
                ssb.Completed += FirstUseWindow_Completed;
                ssb.Begin();
            }
        }

        private void FirstUseWindow_Completed(object sender, EventArgs e)
        {
            border.Visibility= Visibility.Collapsed;
        }
        private void LoadPage(string title, string text, Page page)
        {
            var loadStoryBoard = FindResource("LoadStoryBoard") as Storyboard;
            LoadBoard.Visibility = Visibility.Visible;
            if (loadStoryBoard != null)
            {
                // 使用 lambda 创建一个符合 EventHandler 签名的方法
                loadStoryBoard.Completed += (s, e) => LoadingPage(title, text, page);
                loadStoryBoard.Begin();
            }
        }
        private void LoadingPage(string title, string text, Page page)
        {
            NavigationMenu.Navigate(page);
            TitleGet.Text = title;
            Text.Text = text;
            var loadedStoryBoard = FindResource("LoadedStoryBoard") as Storyboard;
            if(loadedStoryBoard!= null)
            {
                loadedStoryBoard.Completed += (s,e)=>LoadedPage();
                loadedStoryBoard.Begin();
            }
        }
        private void LoadedPage()
        {
            LoadBoard.Visibility = Visibility.Collapsed;
        }
    }
    }
