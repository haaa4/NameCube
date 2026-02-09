using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Wpf.Ui.Controls;
using StackPanel = System.Windows.Controls.StackPanel;

namespace NameCube.Setting
{
    /// <summary>
    /// About.xaml 的交互逻辑
    /// </summary>
    public partial class About : Page
    {
        int clickTimes = 0;

        public About()
        {
            InitializeComponent();
            Log.Debug("About页面初始化完成");
        }

        private async void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                clickTimes++;
                Log.Verbose("关于图标被点击，次数: {ClickTimes}", clickTimes);

                if (clickTimes >= 10)
                {
                    Log.Information("触发Debug模式入口");

                    var dialog = new Wpf.Ui.Controls.ContentDialog();
                    dialog = new ContentDialog()
                    {
                        CloseButtonText = "取消",
                        PrimaryButtonText = "确定",
                        Title = "进入Debug模式",
                        DialogHost = Host,
                        Content = new StackPanel()
                        {
                            Children =
                            {
                                new System.Windows.Controls.TextBlock()
                                {
                                    Text = "请输入对应的密码，开启对应的Debug模式"
                                },
                                new Wpf.Ui.Controls.TextBox()
                            }
                        }
                    };

                    Wpf.Ui.Controls.ContentDialogResult contentDialogResult = await dialog.ShowAsync();

                    if (contentDialogResult == Wpf.Ui.Controls.ContentDialogResult.None)
                    {
                        Log.Debug("用户取消Debug模式输入");
                        dialog.Hide();
                    }
                    else
                    {
                        if (dialog.Content is StackPanel panel)
                        {
                            var password = panel.Children.OfType<Wpf.Ui.Controls.TextBox>().FirstOrDefault();
                            string enteredPassword = password?.Text ?? string.Empty;

                            Log.Debug("用户输入Debug密码，长度: {Length}", enteredPassword.Length);

                            switch (enteredPassword)
                            {
                                case "0d612c12d2ac33625bf3e0351b6f5e4f73829fa8":
                                    Log.Information("开启自动流程Debug模式");
                                    GlobalVariablesData.config.AutomaticProcess.debug = true;
                                    GlobalVariablesData.SaveConfig();
                                    SnackBarFunction.ShowSnackBarInSettingWindow("自动流程Debug模式已开启", ControlAppearance.Success);
                                    break;
                                case "c53d2f1a9a8499bcb477be56c31caa5c76ae60f5":
                                    Log.Information("开启崩溃调试模式");
                                    GlobalVariablesData.config.AllSettings.debug = true;
                                    GlobalVariablesData.SaveConfig();
                                    SnackBarFunction.ShowSnackBarInSettingWindow("崩溃调试已开启", ControlAppearance.Success);
                                    break;
                                case "7a7bc4496e501462270ce7f6f8023c96d32098d8":
                                    Log.Information("开启因子模式调试模式");
                                    GlobalVariablesData.config.MemoryFactorModeSettings.debug = true;
                                    GlobalVariablesData.SaveConfig();
                                    SnackBarFunction.ShowSnackBarInSettingWindow("因子模式调试已开启", ControlAppearance.Success);
                                    break;
                                default:
                                    Log.Warning("Debug密码错误: {Password}", enteredPassword);
                                    SnackBarFunction.ShowSnackBarInSettingWindow("密码错误", Wpf.Ui.Controls.ControlAppearance.Caution);
                                    break;
                            }
                        }
                        else
                        {
                            Log.Warning("Debug对话框内容解析失败");
                        }
                    }
                    clickTimes = 0; // 重置点击次数
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理Debug模式入口时发生异常");
            }
        }
    }
}