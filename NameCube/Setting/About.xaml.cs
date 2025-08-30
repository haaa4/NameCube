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
        }

        private async void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            clickTimes++;
            if(clickTimes >=10)
            {
                var dialog = new Wpf.Ui.Controls.ContentDialog();
                dialog = new ContentDialog()
                {
                    CloseButtonText = "取消",
                    PrimaryButtonText = "确定",
                    Title = "进入Debug模式",
                    DialogHost=Host,
                    Content = new StackPanel()
                    {
                        Children =
                        {
                            new System.Windows.Controls.TextBlock()
                            {
                                Text="请输入对应的密码，开启对应的Debug模式"
                            },
                            new Wpf.Ui.Controls.TextBox()
                        }
                    }
                };
                Wpf.Ui.Controls.ContentDialogResult contentDialogResult = await dialog.ShowAsync();
                if (contentDialogResult == Wpf.Ui.Controls.ContentDialogResult.None)
                {
                    dialog.Hide();
                }
                else
                {
                    if (dialog.Content is StackPanel panel)
                    {
                        var password = panel.Children.OfType<Wpf.Ui.Controls.TextBox>().FirstOrDefault();
                        switch (password.Text)
                        {
                            case "autoIsGod114514":
                                GlobalVariables.json.automaticProcess.debug = true;
                                GlobalVariables.SaveJson();
                                SnackBarFunction.ShowSnackBarInSettingWindow("自动流程Debug模式已开启",ControlAppearance.Success);
                                break;
                            default:
                                SnackBarFunction.ShowSnackBarInSettingWindow("密码错误", Wpf.Ui.Controls.ControlAppearance.Caution);
                                break;
                        }

                    }
                }
            }
        }


    }
}
