using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
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
using Masuit.Tools;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
using Wpf.Ui.Controls;
using ComboBox = System.Windows.Controls.ComboBox;
using FontWeights = System.Windows.FontWeights;
using StackPanel = System.Windows.Controls.StackPanel;
using TextBlock = Wpf.Ui.Controls.TextBlock;

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
            InitializeShortCutKey();
        }

        bool IsChoosing = false;

        private void Choosing(bool isChossing)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.isChoosing = isChossing;
        }

        public void InitializeShortCutKey()
        {
            if (GlobalVariables.json.ShortCutKey.keysGrounp.Count <= 0)
            {
                ShortCutHost.Children.Clear();
            }
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ShortCutHost.Children.Clear();
                WaitingProcessBar.Visibility = Visibility.Visible;
                AddCardAction.IsEnabled = false;
            });
            List<string> items = new List<string>
            {
                "无",
                "单人模式",
                "因子模式",
                "批量模式",
                "数字模式",
                "预备模式",
                "记忆模式",
                "主页",
            };
            for (int i = 0; i < GlobalVariables.json.ShortCutKey.keysGrounp.Count; i++)
            {
                string shortCut = "";
                if (GlobalVariables.json.ShortCutKey.keysGrounp[i].keys.Count <= 0)
                {
                    shortCut = "Unknow";
                }
                else
                {
                    foreach (Key key in GlobalVariables.json.ShortCutKey.keysGrounp[i].keys)
                    {
                        shortCut = shortCut + key.ToString() + " ";
                    }
                }
                Thickness thickness = new Thickness
                {
                    Left = 5,
                    Top = 5,
                    Right = 5,
                    Bottom = 5,
                };
                string Text;
                if (GlobalVariables.json.ShortCutKey.keysGrounp[i].ProcessGroup == null)
                {
                    if (GlobalVariables.json.ShortCutKey.keysGrounp[i].openWay <= 7 && GlobalVariables.json.ShortCutKey.keysGrounp[i].openWay>=0)
                    {
                        Text = items[GlobalVariables.json.ShortCutKey.keysGrounp[i].openWay];
                    }
                    else
                    {
                        Text = "???";
                    }
                }
                else
                {
                    Text = GlobalVariables.json.ShortCutKey.keysGrounp[i].ProcessGroup.name;
                }
                CardAction action = new CardAction()
                {
                    Uid = i.ToString(),
                    Content = new StackPanel()
                    {
                        Children =
                        {
                            new TextBlock()
                            {
                                Text =
                                    Text
                                    + "("
                                    + GlobalVariables.json.ShortCutKey.keysGrounp[i].LastChangeTime
                                    + ")",
                                FontSize = 20,
                                FontWeight = FontWeights.Black,
                            },
                            new TextBlock() { Text = shortCut, FontSize = 20 },
                        },
                    },
                    Margin = thickness,
                };
                action.Click += async (sender, e) =>
                {
                    ShortCut newShortCut = await ChangeData(action.Uid.ToInt32(0));
                    if(newShortCut!=null&&newShortCut.ProcessGroup==null&&newShortCut.openWay==-2)
                    {
                        GlobalVariables.json.ShortCutKey.keysGrounp[action.Uid.ToInt32(0)].keys = newShortCut.keys;
                        GlobalVariables.json.ShortCutKey.keysGrounp[action.Uid.ToInt32(0)].LastChangeTime = newShortCut.LastChangeTime;
                        GlobalVariables.SaveJson();
                        InitializeShortCutKey();
                    }
                    else if (newShortCut != null && newShortCut.LastChangeTime != "Delete")
                    {
                        GlobalVariables.json.ShortCutKey.keysGrounp[action.Uid.ToInt32(0)] =
                            newShortCut;
                        GlobalVariables.SaveJson();
                        InitializeShortCutKey();
                    }
                    else if (newShortCut != null && newShortCut.LastChangeTime == "Delete")
                    {
                        GlobalVariables.json.ShortCutKey.keysGrounp.RemoveAt(action.Uid.ToInt32(0));
                        GlobalVariables.SaveJson();
                        InitializeShortCutKey();
                    }
                };
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ShortCutHost.Children.Add(action);
                });
            }
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                WaitingProcessBar.Visibility = Visibility.Collapsed;
                AddCardAction.IsEnabled = true;
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) { }

        private int HaveTheSame(ProcessGroup processGroup)
        {
            for (int i = 0; i < GlobalVariables.json.automaticProcess.processGroups.Count; i++)
            {
                if (processGroup.uid == GlobalVariables.json.automaticProcess.processGroups[i].uid)
                {
                    return i;
                }
            }

            return -1;
        }

        private async Task<ShortCut> ChangeData(int uid)
        {
            ShortCut shortCut = GlobalVariables.json.ShortCutKey.keysGrounp[uid];
            List<Key> keys = new List<Key>();
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
                    var KeyChooseButton = panel
                        .Children.OfType<Wpf.Ui.Controls.Button>()
                        .FirstOrDefault();
                    var KeyText = panel
                        .Children.OfType<Wpf.Ui.Controls.TextBlock>()
                        .FirstOrDefault();
                    if (IsChoosing)
                    {
                        KeyChooseButton.Content = "编辑";
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        mainWindow.CanUseShortCutKey = true;
                        IsChoosing = false;
                        Choosing(false);
                    }
                    else
                    {
                        keys = new List<Key>();
                        KeyText.Text = "";
                        KeyChooseButton.Content = "完成";
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        mainWindow.CanUseShortCutKey = false;
                        IsChoosing = true;
                        Choosing(true);
                    }
                }
            };
            button.KeyDown += (seder, args) =>
            {
                if (dialog.Content is Wpf.Ui.Controls.StackPanel panel)
                {
                    var KeyText = panel
                        .Children.OfType<Wpf.Ui.Controls.TextBlock>()
                        .FirstOrDefault();
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
                    }
                }
            };
            string keyText = "";
            foreach (Key key3 in shortCut.keys)
            {
                keyText = keyText + key3.ToString() + " ";
            }
            keys = GlobalVariables.json.ShortCutKey.keysGrounp[uid].keys;
            List<string> itemsource = new List<string>()
            {
                "无",
                "单人模式",
                "因子模式",
                "批量模式",
                "数字模式",
                "预备模式",
                "记忆模式",
            };
            int selected;
            for (int i = 0; i < GlobalVariables.json.automaticProcess.processGroups.Count; i++)
            {
                itemsource.Add(GlobalVariables.json.automaticProcess.processGroups[i].name);
            }
            if (shortCut.ProcessGroup == null)
            {
                selected = shortCut.openWay;
            }
            else
            {
                if (HaveTheSame(shortCut.ProcessGroup) != -1)
                {
                    selected = HaveTheSame(shortCut.ProcessGroup) + 7;
                }
                else
                {
                    itemsource.Add("*" + shortCut.ProcessGroup.name + "(已删除)");
                    selected = itemsource.Count - 1;
                }
            }

            dialog = new Wpf.Ui.Controls.ContentDialog()
            {
                CloseButtonText = "取消",
                PrimaryButtonText = "确定",
                IsSecondaryButtonEnabled = true,
                SecondaryButtonText = "删除",
                Title = "修改按键组",
                Content = new Wpf.Ui.Controls.StackPanel
                {
                    Children =
                    {
                        button,
                        new Wpf.Ui.Controls.TextBlock()
                        {
                            Name = "KeyText",
                            FontSize = 20,
                            Text = keyText,
                        },
                        new ComboBox() { ItemsSource = itemsource, SelectedIndex = selected },
                    },
                },
                DialogHost = Host,
            };
            Wpf.Ui.Controls.ContentDialogResult contentDialogResult = await dialog.ShowAsync();
            Choosing(false);
            if (contentDialogResult == Wpf.Ui.Controls.ContentDialogResult.None)
            {
                dialog.Hide();
                return null;
            }
            else if (contentDialogResult == Wpf.Ui.Controls.ContentDialogResult.Secondary)
            {
                ShortCut throwShortCut = new ShortCut()
                {
                    LastChangeTime = "Delete",
                    //提醒需要删除
                };
                return throwShortCut;
            }
            else
            {
                if (dialog.Content is Wpf.Ui.Controls.StackPanel panel)
                {
                    var combox = panel.Children.OfType<ComboBox>().FirstOrDefault();
                    if (keys.Count == 0)
                    {
                        MessageBoxFunction.ShowMessageBoxWarning("快捷键为空");
                        return null;
                    }
                    else if (HaveSameKeys(GlobalVariables.json.ShortCutKey.keysGrounp, keys) > 1)
                    {
                        MessageBoxFunction.ShowMessageBoxWarning("快捷键不得重复");
                        return null;
                    }
                    else
                    {
                        ShortCut throwShortCut;
                        if (combox.SelectedIndex <= 6)
                        {
                            throwShortCut = new ShortCut()
                            {
                                keys = keys,
                                openWay = combox.SelectedIndex,
                                LastChangeTime = DateTime.Now.ToString("F"),
                            };
                        }
                        else if (combox.SelectedItem.ToString()[0] != '*')
                        {
                            throwShortCut = new ShortCut()
                            {
                                keys = keys,
                                ProcessGroup = GlobalVariables.json.automaticProcess.processGroups[
                                    combox.SelectedIndex - 7
                                ],
                                LastChangeTime = DateTime.Now.ToString("F"),
                            };
                        }
                        else
                        {
                            //这里把“已删除”的保存做完
                            throwShortCut = new ShortCut() { keys = keys, LastChangeTime = DateTime.Now.ToString("F"),openWay=-2};

                        }
                        return throwShortCut;
                    }
                }
                return null;
            }
        }

        private async void CardAction_Click(object sender, RoutedEventArgs e)
        {
            List<Key> keys = new List<Key>();
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
                    var KeyChooseButton = panel
                        .Children.OfType<Wpf.Ui.Controls.Button>()
                        .FirstOrDefault();
                    var KeyText = panel
                        .Children.OfType<Wpf.Ui.Controls.TextBlock>()
                        .FirstOrDefault();
                    if (IsChoosing)
                    {
                        KeyChooseButton.Content = "编辑";
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        mainWindow.CanUseShortCutKey = true;
                        IsChoosing = false;
                        Choosing(false);
                    }
                    else
                    {
                        keys = new List<Key>();
                        KeyText.Text = "";
                        KeyChooseButton.Content = "完成";
                        var mainWindow = Application.Current.MainWindow as MainWindow;
                        mainWindow.CanUseShortCutKey = false;
                        IsChoosing = true;
                        Choosing(true);
                    }
                }
            };
            button.KeyDown += (seder, args) =>
            {
                if (dialog.Content is Wpf.Ui.Controls.StackPanel panel)
                {
                    var KeyText = panel
                        .Children.OfType<Wpf.Ui.Controls.TextBlock>()
                        .FirstOrDefault();
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
            List<string> itemsSource = new List<string>()
            {
                "无",
                "单人模式",
                "因子模式",
                "批量模式",
                "数字模式",
                "预备模式",
                "记忆模式",
            };
            for(int i=0;i<GlobalVariables.json.automaticProcess.processGroups.Count;i++)
            {
                itemsSource.Add(GlobalVariables.json.automaticProcess.processGroups[i].name);
            }

            dialog = new Wpf.Ui.Controls.ContentDialog()
            {
                CloseButtonText = "取消",
                PrimaryButtonText = "确定",
                Title = "新建按键组",
                Content = new Wpf.Ui.Controls.StackPanel
                {
                    Children =
                    {
                        button,
                        new Wpf.Ui.Controls.TextBlock() { Name = "KeyText", FontSize = 20 },
                        new ComboBox()
                        {
                            ItemsSource =itemsSource,
                            SelectedIndex = 0,
                        },
                    },
                },
                DialogHost = Host,
            };
            Wpf.Ui.Controls.ContentDialogResult contentDialogResult = await dialog.ShowAsync();
            Choosing(false);
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
                    else if (HaveSameKeys(GlobalVariables.json.ShortCutKey.keysGrounp, keys) > 0)
                    {
                        MessageBoxFunction.ShowMessageBoxWarning("快捷键不得重复");
                    }
                    else
                    {
                        ShortCut shortCut;
                        if(combox.SelectedIndex<=6)
                        {
                            shortCut = new ShortCut()
                            {
                                keys = keys,
                                openWay = combox.SelectedIndex,
                                LastChangeTime = DateTime.Now.ToString("F"),
                            };
                        }
                        else
                        {
                            shortCut = new ShortCut()
                            {
                                keys = keys,
                                ProcessGroup = GlobalVariables.json.automaticProcess.processGroups[combox.SelectedIndex - 7],
                                LastChangeTime = DateTime.Now.ToString("F"),
                            };
                        }
                            GlobalVariables.json.ShortCutKey.keysGrounp.Add(shortCut);
                        GlobalVariables.SaveJson();
                        InitializeShortCutKey();
                    }
                }
            }
        }

        private bool IsTheSameKey(List<Key> keys1, List<Key> keys2)
        {
            if (keys1.Count == keys2.Count)
            {
                bool SameAs = true;
                foreach (Key key in keys1)
                {
                    if (!keys2.Contains(key))
                    {
                        SameAs = false;
                        break;
                    }
                }
                return SameAs;
            }
            else
            {
                return false;
            }
        }

        private int HaveSameKeys(List<ShortCut> shortCuts, List<Key> keys)
        {
            int haveSame = 0;
            foreach (ShortCut shortCut in shortCuts)
            {
                if (IsTheSameKey(keys, shortCut.keys))
                {
                    haveSame++;
                    break;
                }
            }
            return haveSame;
        }
    }
}
