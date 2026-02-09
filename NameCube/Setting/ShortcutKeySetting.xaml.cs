using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NameCube.GlobalVariables.DataClass;
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
using Serilog; // 添加Serilog引用

namespace NameCube.Setting
{
    /// <summary>
    /// ShortcutKeySetting.xaml 的交互逻辑
    /// </summary>
    public partial class ShortcutKeySetting : System.Windows.Controls.Page
    {
        private static readonly ILogger _logger = Log.ForContext<ShortcutKeySetting>(); // 添加Serilog日志实例

        public ShortcutKeySetting()
        {
            InitializeComponent();
            InitializeShortCutKey();
            _logger.Debug("ShortcutKeySetting 初始化完成");
        }

        bool IsChoosing = false;

        private void Choosing(bool isChossing)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.isChoosing = isChossing;
        }

        public void InitializeShortCutKey()
        {
            _logger.Information("开始初始化快捷键设置");

            if (GlobalVariablesData.config.ShortCutKey.keysGrounp.Count <= 0)
            {
                ShortCutHost.Children.Clear();
                _logger.Debug("快捷键分组为空，清空界面");
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

            for (int i = 0; i < GlobalVariablesData.config.ShortCutKey.keysGrounp.Count; i++)
            {
                string shortCut = "";
                if (GlobalVariablesData.config.ShortCutKey.keysGrounp[i].keys.Count <= 0)
                {
                    shortCut = "Unknow";
                    _logger.Warning("第 {Index} 个快捷键组没有按键", i);
                }
                else
                {
                    foreach (Key key in GlobalVariablesData.config.ShortCutKey.keysGrounp[i].keys)
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
                if (GlobalVariablesData.config.ShortCutKey.keysGrounp[i].ProcessGroup == null)
                {
                    if (GlobalVariablesData.config.ShortCutKey.keysGrounp[i].openWay <= 7 && GlobalVariablesData.config.ShortCutKey.keysGrounp[i].openWay >= 0)
                    {
                        Text = items[GlobalVariablesData.config.ShortCutKey.keysGrounp[i].openWay];
                    }
                    else
                    {
                        Text = "???";
                        _logger.Warning("未知的打开方式: {OpenWay}", GlobalVariablesData.config.ShortCutKey.keysGrounp[i].openWay);
                    }
                }
                else
                {
                    Text = GlobalVariablesData.config.ShortCutKey.keysGrounp[i].ProcessGroup.name;
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
                                    + GlobalVariablesData.config.ShortCutKey.keysGrounp[i].LastChangeTime
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
                    _logger.Information("点击修改快捷键组 {Uid}", action.Uid);
                    ShortCut newShortCut = await ChangeData(action.Uid.ToInt32(0));
                    if (newShortCut != null && newShortCut.ProcessGroup == null && newShortCut.openWay == -2)
                    {
                        GlobalVariablesData.config.ShortCutKey.keysGrounp[action.Uid.ToInt32(0)].keys = newShortCut.keys;
                        GlobalVariablesData.config.ShortCutKey.keysGrounp[action.Uid.ToInt32(0)].LastChangeTime = newShortCut.LastChangeTime;
                        GlobalVariablesData.SaveConfig();
                        InitializeShortCutKey();
                    }
                    else if (newShortCut != null && newShortCut.LastChangeTime != "Delete")
                    {
                        GlobalVariablesData.config.ShortCutKey.keysGrounp[action.Uid.ToInt32(0)] =
                            newShortCut;
                        GlobalVariablesData.SaveConfig();
                        InitializeShortCutKey();
                    }
                    else if (newShortCut != null && newShortCut.LastChangeTime == "Delete")
                    {
                        _logger.Information("删除快捷键组 {Uid}", action.Uid);
                        GlobalVariablesData.config.ShortCutKey.keysGrounp.RemoveAt(action.Uid.ToInt32(0));
                        GlobalVariablesData.SaveConfig();
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
                _logger.Information("快捷键初始化完成，共 {Count} 个快捷键组", GlobalVariablesData.config.ShortCutKey.keysGrounp.Count);
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _logger.Debug("ShortcutKeySetting 页面加载完成");
        }

        private int HaveTheSame(ProcessGroup processGroup)
        {
            for (int i = 0; i < GlobalVariablesData.config.AutomaticProcess.processGroups.Count; i++)
            {
                if (processGroup.uid == GlobalVariablesData.config.AutomaticProcess.processGroups[i].uid)
                {
                    return i;
                }
            }
            return -1;
        }

        private async Task<ShortCut> ChangeData(int uid)
        {
            _logger.Information("开始修改快捷键组 {Uid}", uid);

            try
            {
                ShortCut shortCut = GlobalVariablesData.config.ShortCutKey.keysGrounp[uid];
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
                            _logger.Debug("结束快捷键编辑");
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
                            _logger.Debug("开始快捷键编辑");
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
                                    _logger.Debug("检测到重复按键: {Key}", key);
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
                            _logger.Debug("添加按键: {Key}，当前按键数: {Count}", key, keys.Count);
                        }
                    }
                };

                string keyText = "";
                foreach (Key key3 in shortCut.keys)
                {
                    keyText = keyText + key3.ToString() + " ";
                }
                keys = GlobalVariablesData.config.ShortCutKey.keysGrounp[uid].keys;

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
                for (int i = 0; i < GlobalVariablesData.config.AutomaticProcess.processGroups.Count; i++)
                {
                    itemsource.Add(GlobalVariablesData.config.AutomaticProcess.processGroups[i].name);
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
                        _logger.Warning("快捷键组 {Uid} 关联的处理组已删除", uid);
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
                    _logger.Information("用户取消修改快捷键组 {Uid}", uid);
                    dialog.Hide();
                    return null;
                }
                else if (contentDialogResult == Wpf.Ui.Controls.ContentDialogResult.Secondary)
                {
                    ShortCut throwShortCut = new ShortCut()
                    {
                        LastChangeTime = "Delete",
                    };
                    _logger.Information("用户选择删除快捷键组 {Uid}", uid);
                    return throwShortCut;
                }
                else
                {
                    if (dialog.Content is Wpf.Ui.Controls.StackPanel panel)
                    {
                        var combox = panel.Children.OfType<ComboBox>().FirstOrDefault();
                        if (keys.Count == 0)
                        {
                            SnackBarFunction.ShowSnackBarInSettingWindow("快捷键为空", ControlAppearance.Caution);
                            _logger.Warning("快捷键组 {Uid} 的快捷键为空", uid);
                            return null;
                        }
                        else if (HaveSameKeys(GlobalVariablesData.config.ShortCutKey.keysGrounp, keys) > 1)
                        {
                            SnackBarFunction.ShowSnackBarInSettingWindow("快捷键不得重复", Wpf.Ui.Controls.ControlAppearance.Caution);
                            _logger.Warning("快捷键组 {Uid} 的快捷键重复", uid);
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
                                _logger.Information("快捷键组 {Uid} 更新为打开方式: {OpenWay}", uid, combox.SelectedIndex);
                            }
                            else if (combox.SelectedItem.ToString()[0] != '*')
                            {
                                throwShortCut = new ShortCut()
                                {
                                    keys = keys,
                                    ProcessGroup = GlobalVariablesData.config.AutomaticProcess.processGroups[
                                        combox.SelectedIndex - 7
                                    ],
                                    LastChangeTime = DateTime.Now.ToString("F"),
                                };
                                _logger.Information("快捷键组 {Uid} 更新为处理组: {ProcessGroup}", uid, combox.SelectedItem);
                            }
                            else
                            {
                                throwShortCut = new ShortCut() { keys = keys, LastChangeTime = DateTime.Now.ToString("F"), openWay = -2 };
                                _logger.Warning("快捷键组 {Uid} 关联的处理组已删除，仅更新按键", uid);
                            }
                            return throwShortCut;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "修改快捷键组 {Uid} 时发生异常", uid);
                throw;
            }
        }

        private async void CardAction_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("开始创建新的快捷键组");

            try
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
                            _logger.Debug("结束快捷键编辑");
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
                            _logger.Debug("开始快捷键编辑");
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
                                    _logger.Debug("检测到重复按键: {Key}", key);
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
                            GlobalVariablesData.SaveConfig();
                            _logger.Debug("添加按键: {Key}，当前按键数: {Count}", key, keys.Count);
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

                for (int i = 0; i < GlobalVariablesData.config.AutomaticProcess.processGroups.Count; i++)
                {
                    itemsSource.Add(GlobalVariablesData.config.AutomaticProcess.processGroups[i].name);
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
                    _logger.Information("用户取消创建新快捷键组");
                    dialog.Hide();
                }
                else
                {
                    if (dialog.Content is Wpf.Ui.Controls.StackPanel panel)
                    {
                        var combox = panel.Children.OfType<ComboBox>().FirstOrDefault();
                        if (keys.Count == 0)
                        {
                            SnackBarFunction.ShowSnackBarInSettingWindow("快捷键为空", Wpf.Ui.Controls.ControlAppearance.Caution);
                            _logger.Warning("新建快捷键组的快捷键为空");
                        }
                        else if (HaveSameKeys(GlobalVariablesData.config.ShortCutKey.keysGrounp, keys) > 0)
                        {
                            SnackBarFunction.ShowSnackBarInSettingWindow("快捷键不得重复", Wpf.Ui.Controls.ControlAppearance.Caution);
                            _logger.Warning("新建快捷键组的快捷键重复");
                        }
                        else
                        {
                            ShortCut shortCut;
                            if (combox.SelectedIndex <= 6)
                            {
                                shortCut = new ShortCut()
                                {
                                    keys = keys,
                                    openWay = combox.SelectedIndex,
                                    LastChangeTime = DateTime.Now.ToString("F"),
                                };
                                _logger.Information("创建新的快捷键组，打开方式: {OpenWay}", combox.SelectedIndex);
                            }
                            else
                            {
                                shortCut = new ShortCut()
                                {
                                    keys = keys,
                                    ProcessGroup = GlobalVariablesData.config.AutomaticProcess.processGroups[combox.SelectedIndex - 7],
                                    LastChangeTime = DateTime.Now.ToString("F"),
                                };
                                _logger.Information("创建新的快捷键组，处理组: {ProcessGroup}", combox.SelectedItem);
                            }

                            GlobalVariablesData.config.ShortCutKey.keysGrounp.Add(shortCut);
                            GlobalVariablesData.SaveConfig();
                            InitializeShortCutKey();
                            _logger.Information("新快捷键组创建成功，当前总数: {Count}", GlobalVariablesData.config.ShortCutKey.keysGrounp.Count);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "创建新快捷键组时发生异常");
                throw;
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