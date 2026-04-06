using Masuit.Tools;
using NameCube.GlobalVariables.DataClass;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
        private static readonly ILogger _logger = Log.ForContext<ShortcutKeySetting>();

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
            if (mainWindow != null) mainWindow.isChoosing = isChossing;
        }

        public void InitializeShortCutKey()
        {
            _logger.Information("开始初始化快捷键设置");

           
            for (int i = GlobalVariablesData.config.ShortCutKey.keysGrounp.Count - 1; i >= 0; i--)
            {
                var sc = GlobalVariablesData.config.ShortCutKey.keysGrounp[i];
                if (sc.ProcessGroup != null)
                {
                    var pg = sc.ProcessGroup.GetProcessGroup();
                    if (pg == null)
                    {
                        _logger.Warning("快捷键组 {Index} 关联的流程组已删除 (UID: {Uid})，自动删除该快捷键组", i, sc.ProcessGroup.uid);
                        GlobalVariablesData.config.ShortCutKey.keysGrounp.RemoveAt(i);
                        SnackBarFunction.ShowSnackBarInSettingWindow("未找到" + sc.ProcessGroup.uid + "对应的流程组，已自动删除该时间表项", Wpf.Ui.Controls.ControlAppearance.Caution);
                        // 若需要用户确认或执行其他操作，可在此处添加代码
                    }
                }
            }
            // 如果有删除操作，保存配置
            GlobalVariablesData.SaveConfig();

            // 清空界面并准备刷新
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ShortCutHost.Children.Clear();
                WaitingProcessBar.Visibility = Visibility.Visible;
                AddCardAction.IsEnabled = false;
            });

            List<string> items = new List<string>
            {
                "无", "单人模式", "因子模式", "批量模式", "数字模式", "预备模式", "记忆模式", "主页"
            };

            for (int i = 0; i < GlobalVariablesData.config.ShortCutKey.keysGrounp.Count; i++)
            {
                var shortCut = GlobalVariablesData.config.ShortCutKey.keysGrounp[i];

                // 构建快捷键显示字符串
                string shortCutStr = "";
                if (shortCut.keys.Count == 0)
                {
                    shortCutStr = "Unknow";
                    _logger.Warning("第 {Index} 个快捷键组没有按键", i);
                }
                else
                {
                    shortCutStr = string.Join(" ", shortCut.keys);
                }

                // 确定显示文本（打开的页面或流程组名称）
                string displayText;
                if (shortCut.ProcessGroup == null)
                {
                    if (shortCut.openWay >= 0 && shortCut.openWay <= 7)
                        displayText = items[shortCut.openWay];
                    else
                    {
                        displayText = "???";
                        _logger.Warning("未知的打开方式: {OpenWay}", shortCut.openWay);
                    }
                }
                else
                {
                    // 通过 ProcessGroupUid 获取实际流程组，若已删除则显示“已删除”
                    var pg = shortCut.ProcessGroup.GetProcessGroup();
                    if (pg != null)
                        displayText = pg.name;
                    else
                    {
                        displayText = $"已删除的流程组(UID:{shortCut.ProcessGroup.uid})";
                        _logger.Warning("快捷键组 {Index} 关联的流程组已删除 (UID: {Uid})", i, shortCut.ProcessGroup.uid);
                    }
                }

                Thickness thickness = new Thickness(5);
                CardAction action = new CardAction()
                {
                    Uid = i.ToString(),
                    Content = new StackPanel()
                    {
                        Children =
                        {
                            new TextBlock()
                            {
                                Text = $"{displayText} ({shortCut.LastChangeTime})",
                                FontSize = 20,
                                FontWeight = FontWeights.Black,
                            },
                            new TextBlock() { Text = shortCutStr, FontSize = 20 },
                        }
                    },
                    Margin = thickness,
                };

                action.Click += async (sender, e) =>
                {
                    _logger.Information("点击修改快捷键组 {Uid}", action.Uid);
                    ShortCut newShortCut = await ChangeData(action.Uid.ToInt32(0));
                    if (newShortCut != null && newShortCut.ProcessGroup == null && newShortCut.openWay == -2)
                    {
                        // 仅更新按键和修改时间
                        var target = GlobalVariablesData.config.ShortCutKey.keysGrounp[action.Uid.ToInt32(0)];
                        target.keys = newShortCut.keys;
                        target.LastChangeTime = newShortCut.LastChangeTime;
                        GlobalVariablesData.SaveConfig();
                        InitializeShortCutKey();
                    }
                    else if (newShortCut != null && newShortCut.LastChangeTime != "Delete")
                    {
                        GlobalVariablesData.config.ShortCutKey.keysGrounp[action.Uid.ToInt32(0)] = newShortCut;
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

                Application.Current.Dispatcher.InvokeAsync(() => ShortCutHost.Children.Add(action));
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
                    return i;
            }
            return -1;
        }

        private async Task<ShortCut> ChangeData(int uid)
        {
            _logger.Information("开始修改快捷键组 {Uid}", uid);

            try
            {
                ShortCut shortCut = GlobalVariablesData.config.ShortCutKey.keysGrounp[uid];
                List<Key> keys = new List<Key>(shortCut.keys);
                var dialog = new Wpf.Ui.Controls.ContentDialog();
                var button = new Wpf.Ui.Controls.Button() { Name = "KeyChooseButton", Content = "编辑" };

                button.Click += (seder, args) =>
                {
                    if (dialog.Content is Wpf.Ui.Controls.StackPanel panel)
                    {
                        var keyChooseButton = panel.Children.OfType<Wpf.Ui.Controls.Button>().FirstOrDefault();
                        var keyTextBlock = panel.Children.OfType<Wpf.Ui.Controls.TextBlock>().FirstOrDefault();
                        if (IsChoosing)
                        {
                            keyChooseButton.Content = "编辑";
                            var mainWindow = Application.Current.MainWindow as MainWindow;
                            if (mainWindow != null) mainWindow.CanUseShortCutKey = true;
                            IsChoosing = false;
                            Choosing(false);
                            _logger.Debug("结束快捷键编辑");
                        }
                        else
                        {
                            keys.Clear();
                            keyTextBlock.Text = "";
                            keyChooseButton.Content = "完成";
                            var mainWindow = Application.Current.MainWindow as MainWindow;
                            if (mainWindow != null) mainWindow.CanUseShortCutKey = false;
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
                        var keyTextBlock = panel.Children.OfType<Wpf.Ui.Controls.TextBlock>().FirstOrDefault();
                        if (IsChoosing && keys.Count <= 4)
                        {
                            Key key = args.Key;
                            if (!keys.Contains(key))
                            {
                                keys.Add(key);
                                keyTextBlock.Text = string.Join(" ", keys);
                                _logger.Debug("添加按键: {Key}，当前按键数: {Count}", key, keys.Count);
                            }
                            else
                            {
                                _logger.Debug("检测到重复按键: {Key}", key);
                            }
                        }
                    }
                };

                // 构建下拉列表项
                List<string> itemsource = new List<string>()
                {
                    "无", "单人模式", "因子模式", "批量模式", "数字模式", "预备模式", "记忆模式"
                };

                foreach (var pg in GlobalVariablesData.config.AutomaticProcess.processGroups)
                    itemsource.Add(pg.name);

                int selectedIndex;
                ProcessGroup actualProcessGroup = null;
                if (shortCut.ProcessGroup != null)
                    actualProcessGroup = shortCut.ProcessGroup.GetProcessGroup();

                if (shortCut.ProcessGroup == null)
                {
                    selectedIndex = shortCut.openWay;
                }
                else
                {
                    if (actualProcessGroup != null)
                    {
                        selectedIndex = HaveTheSame(actualProcessGroup) + 7;
                    }
                    else
                    {
                        itemsource.Add($"*{shortCut.ProcessGroup.uid}(已删除)");
                        selectedIndex = itemsource.Count - 1;
                        _logger.Warning("快捷键组 {Uid} 关联的流程组已删除 (UID: {Uid})", uid, shortCut.ProcessGroup.uid);
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
                            new Wpf.Ui.Controls.TextBlock() { Name = "KeyText", FontSize = 20, Text = string.Join(" ", keys) },
                            new ComboBox() { ItemsSource = itemsource, SelectedIndex = selectedIndex }
                        }
                    },
                    DialogHost = Host,
                };

                Wpf.Ui.Controls.ContentDialogResult result = await dialog.ShowAsync();
                Choosing(false);

                if (result == Wpf.Ui.Controls.ContentDialogResult.None)
                {
                    _logger.Information("用户取消修改快捷键组 {Uid}", uid);
                    dialog.Hide();
                    return null;
                }
                else if (result == Wpf.Ui.Controls.ContentDialogResult.Secondary)
                {
                    _logger.Information("用户选择删除快捷键组 {Uid}", uid);
                    return new ShortCut { LastChangeTime = "Delete" };
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
                            SnackBarFunction.ShowSnackBarInSettingWindow("快捷键不得重复", ControlAppearance.Caution);
                            _logger.Warning("快捷键组 {Uid} 的快捷键重复", uid);
                            return null;
                        }
                        else
                        {
                            ShortCut newShortCut;
                            if (combox.SelectedIndex <= 6)
                            {
                                newShortCut = new ShortCut()
                                {
                                    keys = keys,
                                    openWay = combox.SelectedIndex,
                                    LastChangeTime = DateTime.Now.ToString("F"),
                                    ProcessGroup = null
                                };
                                _logger.Information("快捷键组 {Uid} 更新为打开方式: {OpenWay}", uid, combox.SelectedIndex);
                            }
                            else if (combox.SelectedItem.ToString()[0] != '*')
                            {
                                var selectedGroup = GlobalVariablesData.config.AutomaticProcess.processGroups[combox.SelectedIndex - 7];
                                newShortCut = new ShortCut()
                                {
                                    keys = keys,
                                    ProcessGroup = new ProcessGroupUid { uid = selectedGroup.uid },
                                    LastChangeTime = DateTime.Now.ToString("F"),
                                    openWay = -1
                                };
                                _logger.Information("快捷键组 {Uid} 更新为处理组: {ProcessGroup}", uid, selectedGroup.name);
                            }
                            else
                            {
                                // 选中的是已删除项，只更新按键，不清除原 ProcessGroup 引用（可保留或置 null）
                                newShortCut = new ShortCut()
                                {
                                    keys = keys,
                                    LastChangeTime = DateTime.Now.ToString("F"),
                                    openWay = -2,   // 标记为仅按键更新
                                    ProcessGroup = null
                                };
                                _logger.Warning("快捷键组 {Uid} 关联的处理组已删除，仅更新按键", uid);
                            }
                            return newShortCut;
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
                var button = new Wpf.Ui.Controls.Button() { Name = "KeyChooseButton", Content = "编辑" };

                button.Click += (seder, args) =>
                {
                    if (dialog.Content is Wpf.Ui.Controls.StackPanel panel)
                    {
                        var keyChooseButton = panel.Children.OfType<Wpf.Ui.Controls.Button>().FirstOrDefault();
                        var keyTextBlock = panel.Children.OfType<Wpf.Ui.Controls.TextBlock>().FirstOrDefault();
                        if (IsChoosing)
                        {
                            keyChooseButton.Content = "编辑";
                            var mainWindow = Application.Current.MainWindow as MainWindow;
                            if (mainWindow != null) mainWindow.CanUseShortCutKey = true;
                            IsChoosing = false;
                            Choosing(false);
                            _logger.Debug("结束快捷键编辑");
                        }
                        else
                        {
                            keys.Clear();
                            keyTextBlock.Text = "";
                            keyChooseButton.Content = "完成";
                            var mainWindow = Application.Current.MainWindow as MainWindow;
                            if (mainWindow != null) mainWindow.CanUseShortCutKey = false;
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
                        var keyTextBlock = panel.Children.OfType<Wpf.Ui.Controls.TextBlock>().FirstOrDefault();
                        if (IsChoosing && keys.Count <= 4)
                        {
                            Key key = args.Key;
                            if (!keys.Contains(key))
                            {
                                keys.Add(key);
                                keyTextBlock.Text = string.Join(" ", keys);
                                _logger.Debug("添加按键: {Key}，当前按键数: {Count}", key, keys.Count);
                            }
                            else
                            {
                                _logger.Debug("检测到重复按键: {Key}", key);
                            }
                        }
                    }
                };

                List<string> itemsSource = new List<string>()
                {
                    "无", "单人模式", "因子模式", "批量模式", "数字模式", "预备模式", "记忆模式"
                };
                foreach (var pg in GlobalVariablesData.config.AutomaticProcess.processGroups)
                    itemsSource.Add(pg.name);

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
                            new ComboBox() { ItemsSource = itemsSource, SelectedIndex = 0 }
                        }
                    },
                    DialogHost = Host,
                };

                Wpf.Ui.Controls.ContentDialogResult result = await dialog.ShowAsync();
                Choosing(false);

                if (result == Wpf.Ui.Controls.ContentDialogResult.None)
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
                            SnackBarFunction.ShowSnackBarInSettingWindow("快捷键为空", ControlAppearance.Caution);
                            _logger.Warning("新建快捷键组的快捷键为空");
                        }
                        else if (HaveSameKeys(GlobalVariablesData.config.ShortCutKey.keysGrounp, keys) > 0)
                        {
                            SnackBarFunction.ShowSnackBarInSettingWindow("快捷键不得重复", ControlAppearance.Caution);
                            _logger.Warning("新建快捷键组的快捷键重复");
                        }
                        else
                        {
                            ShortCut newShortCut;
                            if (combox.SelectedIndex <= 6)
                            {
                                newShortCut = new ShortCut()
                                {
                                    keys = keys,
                                    openWay = combox.SelectedIndex,
                                    LastChangeTime = DateTime.Now.ToString("F"),
                                    ProcessGroup = null
                                };
                                _logger.Information("创建新的快捷键组，打开方式: {OpenWay}", combox.SelectedIndex);
                            }
                            else
                            {
                                var selectedGroup = GlobalVariablesData.config.AutomaticProcess.processGroups[combox.SelectedIndex - 7];
                                newShortCut = new ShortCut()
                                {
                                    keys = keys,
                                    ProcessGroup = new ProcessGroupUid { uid = selectedGroup.uid },
                                    LastChangeTime = DateTime.Now.ToString("F"),
                                    openWay = -1
                                };
                                _logger.Information("创建新的快捷键组，处理组: {ProcessGroup}", selectedGroup.name);
                            }

                            GlobalVariablesData.config.ShortCutKey.keysGrounp.Add(newShortCut);
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
            if (keys1.Count != keys2.Count) return false;
            return !keys1.Except(keys2).Any();
        }

        private int HaveSameKeys(List<ShortCut> shortCuts, List<Key> keys)
        {
            return shortCuts.Count(sc => IsTheSameKey(keys, sc.keys));
        }
    }
}