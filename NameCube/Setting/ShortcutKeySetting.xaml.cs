using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Controls;
using ComboBox = System.Windows.Controls.ComboBox;

namespace NameCube.Setting
{
    /// <summary>
    /// ShortcutKeySetting.xaml 的交互逻辑
    /// </summary>
    public partial class ShortcutKeySetting : System.Windows.Controls.Page
    {
        public ShortcutKeySetting()
        {
            InitializeComponent();
            canChange = false;

            canChange = true;
        }
        bool IsChoosing = false, canChange = false;

        private async void CardAction_Click(object sender, RoutedEventArgs e)
        {
            List<Key> keys = new List<Key>();
            int openway = 0;
            var dialog = new Wpf.Ui.Controls.ContentDialog();
            var button = new Wpf.Ui.Controls.Button()
            {
                Name = "KeyChooseButton",
                Content = "编辑",
            };
            button.Click += (seder, args) =>
            {
                if (dialog.Content is Wpf.Ui.Controls.StackPanel panel)
                {
                    var KeyChooseButton = panel.Children.OfType<Wpf.Ui.Controls.Button>().FirstOrDefault();
                    var KeyText = panel.Children.OfType<Wpf.Ui.Controls.TextBlock>().FirstOrDefault();
                    if (IsChoosing)
                    {
                        KeyChooseButton.Content = "编辑";
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        mainWindow.CanUseShortCutKey = true;
                        IsChoosing = false;
                    }
                    else
                    {
                        keys = new List<Key>();
                        KeyText.Text = "";
                        KeyChooseButton.Content = "完成";
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        mainWindow.CanUseShortCutKey = false;
                        IsChoosing = true;
                    }
                }

            };
            button.KeyDown += (seder, args) =>
            {
                if (dialog.Content is Wpf.Ui.Controls.StackPanel panel)
                {
                    var KeyText = panel.Children.OfType<Wpf.Ui.Controls.TextBlock>().FirstOrDefault();
                    if (IsChoosing && keys.Count <= 4)
                    {
                        Key key = args.Key;
                        foreach (Key key1 in keys)
                        {
                            if (key1 == key)
                            {
                                return;
                            }
                        }
                        keys.Add(key);
                        KeyText.Text = "";
                        foreach (Key key2 in keys)
                        {
                            KeyText.Text = KeyText.Text + key2.ToString() + " ";
                        }
                        KeyText.Text.Remove(KeyText.Text.Length - 1);
                        GlobalVariables.SaveJson();
                    }
                }

            };

            dialog = new Wpf.Ui.Controls.ContentDialog()
            {
                CloseButtonText = "取消",
                PrimaryButtonText = "确定",
                Title = "输入保存名",
                Content = new Wpf.Ui.Controls.StackPanel
                {
                    Children =
                    {
                        button,
                        new Wpf.Ui.Controls.TextBlock()
                        {
                            Name = "KeyText",
                            FontSize = 20,
                        },
                        new ComboBox()
                        {
                            ItemsSource = new List<string>()
                            {
                                "无",
                                "单人模式",
                                "因子模式",
                                "批量模式",
                                "数字模式",
                                "预备模式",
                                "记忆模式",
                            },
                            SelectedIndex = 0,
                        },
                    },
                },
                DialogHost = Host,
            };
            Wpf.Ui.Controls.ContentDialogResult contentDialogResult= await dialog.ShowAsync();
            if (contentDialogResult == Wpf.Ui.Controls.ContentDialogResult.None)
            {
                dialog.Hide();
            }
            else
            {
                if (dialog.Content is Wpf.Ui.Controls.StackPanel panel)
                {
                    var combox = panel.Children.OfType<ComboBox>().FirstOrDefault();
                    if (keys.Count == 0)
                    {
                        MessageBoxFunction.ShowMessageBoxWarning("快捷键为空");
                    }
                    else
                    {
                        ShortCut shortCut = new ShortCut()
                        {
                            keys = keys,
                            openWay = combox.SelectedIndex,
                        };
                        GlobalVariables.json.ShortCutKey.keysGrounp.Add(shortCut);
                    }

                }
            }
        }

        //private void Operation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if(canChange)
        //    {
        //        GlobalVariables.json.ShortCutKey.Way = Operation.SelectedIndex;
        //        GlobalVariables.SaveJson();
        //    }
        //}

        //private void Page_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    var mainWindow = Application.Current.MainWindow as MainWindow;
        //    mainWindow.CanUseShortCutKey = true;
        //}

    }
}
