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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;
using Application = System.Windows.Application;

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
                NavigationMenu.Navigate(typeof(FirstUse.NameCubeMode));
                TitleGet.Text = "选择启动模式";
                Text.Text = "在合适的场景使用合适的模式，可以更好地发挥作用";
            }
            else if (now == 2)
            {
                NavigationMenu.Navigate(typeof(Setting.Archives));
                TitleGet.Text = "设置名单";
                Text.Text = "名单是一切点名的开始，如果没有合适名单，就试试默认名单吧";
            }
            else if (now == 3)
            {
                if (GlobalVariables.json.AllSettings.NameCubeMode == 0)
                {
                    NavigationMenu.Navigate(typeof(FirstUse.BirdSettings));
                    TitleGet.Text = "悬浮球设置";
                    Text.Text = "悬浮球是一个很好的东西，可以更快地启动主窗口";
                }
                else
                {
                    GlobalVariables.SaveJson();
                    NavigationMenu.Navigate(typeof(Setting.About));
                    TitleGet    .Text = "完成！";
                    Text.Text = "所有基本设置均已完成，更多设置请在应用设置里查看";
                }
            }
            else if (now == 4)
            {
                if (GlobalVariables.json.AllSettings.NameCubeMode == 0)
                {
                    NavigationMenu.Navigate(typeof(FirstUse.Other));
                    TitleGet.Text = "其他......";
                    Text.Text = "这里还有一些其他设置，也许有些用";
                }
                else
                {
                    AppFunction.Restart();
                }
            }
            else if (now == 5)
            {
                GlobalVariables.SaveJson();
                NavigationMenu.Navigate(typeof(Setting.About));
                TitleGet.Text = "完成！";
                Text.Text ="所有基本设置均已完成，更多设置请在应用设置里查看";
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
                    System.Windows.Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                }

            }


        }
    }
    }
