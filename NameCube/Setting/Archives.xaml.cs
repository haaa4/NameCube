// /*
//  * NameCube-<点名器>
//  * Copyright (C) 2025-2026 haaa4
//  *
//  * This program is free software: you can redistribute it and/or modify
//  * it under the terms of the GNU General Public License as published by
//  * the Free Software Foundation, either version 3 of the License, or
//  * (at your option) any later version.
//  *
//  * This program is distributed in the hope that it will be useful,
//  * but WITHOUT ANY WARRANTY

using Microsoft.Win32;
using NameCube.Function;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NameCube.Setting
{
    /// <summary>
    /// Archives.xaml 的交互逻辑
    /// </summary>
    public partial class Archives : Page
    {
        public class AllName
        {
            public string Name { get; set; }
        }

        public ObservableCollection<AllName> AllNames { get; set; } = new ObservableCollection<AllName>();

        public Archives()
        {
            InitializeComponent();
            DataContext = this;

            try
            {
                Log.Debug("Archives页面初始化");

                if (GlobalVariablesData.config.AllSettings.Name != null)
                {
                    int nameCount = GlobalVariablesData.config.AllSettings.Name.Count;
                    Log.Information("加载学生名单，数量: {Count}", nameCount);

                    for (int i = 1; i <= nameCount; i++)
                    {
                        AllNames.Add(new AllName
                        {
                            Name = GlobalVariablesData.config.AllSettings.Name[i - 1]
                        });
                    }

                    if (nameCount >= 120)
                    {
                        Log.Warning("学生名单数量超过120，显示警告");
                    }
                }
                else
                {
                    Log.Debug("学生名单为空，初始化空列表");
                    GlobalVariablesData.config.AllSettings.Name = new List<string>();
                }

                UpdateNameCount();

                Log.Information("Archives页面初始化完成，显示 {Count} 个姓名", AllNames.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Archives页面初始化时发生异常");
            }
        }

        // ---------------------------------------------------------------- 界面状态

        /// <summary>
        /// 刷新“共 N 个名字”、空名单提示和超过 120 个的警告条。
        /// </summary>
        private void UpdateNameCount()
        {
            int count = AllNames.Count;
            if (NameCountText != null)
            {
                NameCountText.Text = count == 0 ? "共 0 个名字" : $"共 {count} 个名字";
            }

            if (WarningInfoBar != null)
            {
                WarningInfoBar.IsOpen = count >= 120;
            }

            UpdateEmptyHint();
        }

        /// <summary>
        /// 名单为空、或者搜索没有结果时，在表格中间显示一句提示。
        /// </summary>
        private void UpdateEmptyHint()
        {
            if (EmptyHint == null)
            {
                return;
            }

            string filter = FilterTextBox?.Text?.Trim() ?? string.Empty;
            string text = null;

            if (AllNames.Count == 0)
            {
                text = "名单还是空的，点上方「批量添加」可以一次粘贴多个名字";
            }
            else if (filter.Length > 0 && !CollectionViewSource.GetDefaultView(AllNames).Cast<object>().Any())
            {
                text = $"没有找到包含「{filter}」的名字";
            }

            EmptyHint.Text = text ?? string.Empty;
            EmptyHint.Visibility = text == null ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// 名字很多时用关键字过滤（不影响真实名单，只影响表格显示）。
        /// </summary>
        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string filter = FilterTextBox.Text?.Trim() ?? string.Empty;
                ICollectionView view = CollectionViewSource.GetDefaultView(AllNames);
                if (view == null)
                {
                    return;
                }

                if (filter.Length == 0)
                {
                    view.Filter = null;
                }
                else
                {
                    view.Filter = item =>
                        item is AllName name && name.Name != null
                        && name.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                view.Refresh();
                UpdateEmptyHint();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "过滤学生名单时发生异常");
            }
        }

        /// <summary>
        /// 把若干名字一次性加进列表（自动去首尾空格、跳过空行和已存在的名字），只在最后保存一次配置。
        /// </summary>
        /// <returns>实际添加的数量和被跳过的数量。</returns>
        private (int Added, int Skipped) AddNames(IEnumerable<string> names, string source)
        {
            int added = 0;
            int skipped = 0;

            if (GlobalVariablesData.config.AllSettings.Name == null)
            {
                GlobalVariablesData.config.AllSettings.Name = new List<string>();
            }

            // 用 Ordinal 比较：名字是专有名词，不做大小写/区域性折叠，避免把“李明”和“李 明”之类混在一起
            var existing = new HashSet<string>(
                GlobalVariablesData.config.AllSettings.Name, StringComparer.Ordinal);

            foreach (string raw in names)
            {
                string name = raw?.Trim() ?? string.Empty;
                if (name.Length == 0)
                {
                    skipped++;
                    continue;
                }

                if (!existing.Add(name))
                {
                    skipped++;
                    continue;
                }

                GlobalVariablesData.config.AllSettings.Name.Add(name);
                AllNames.Add(new AllName { Name = name });
                added++;
            }

            if (added > 0)
            {
                Log.Information("从{Source}添加 {Added} 个学生姓名，跳过 {Skipped} 个", source, added, skipped);
                UpdateNameCount();
                GlobalVariablesData.SaveConfig();
                Log.Debug("学生姓名添加完成，当前总数: {Count}", GlobalVariablesData.config.AllSettings.Name.Count);
            }
            else
            {
                Log.Debug("从{Source}没有添加任何姓名，跳过 {Skipped} 个", source, skipped);
            }

            return (added, skipped);
        }

        // ---------------------------------------------------------------- 批量添加

        /// <summary>
        /// 批量添加：弹出一个多行输入框，用户一行一个名字，确定后一次性加进名单。
        /// </summary>
        private async void BulkAddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("打开批量添加名字对话框");

                string input = await ShowBulkInputAsync();
                if (input == null)
                {
                    Log.Debug("用户取消批量添加");
                    return;
                }

                List<string> lines = SplitLines(input).ToList();
                if (lines.Count == 0)
                {
                    SnackBarFunction.ShowSnackBarInSettingWindow(
                        "没有读到任何名字，请一行输入一个名字", Wpf.Ui.Controls.ControlAppearance.Caution);
                    return;
                }

                (int added, int skipped) = AddNames(lines, "批量添加");

                if (added == 0)
                {
                    SnackBarFunction.ShowSnackBarInSettingWindow(
                        $"这些名字都已经在名单里了（跳过 {skipped} 个）", Wpf.Ui.Controls.ControlAppearance.Caution);
                    return;
                }

                string message = skipped > 0
                    ? $"已添加 {added} 个名字，跳过 {skipped} 个重复的"
                    : $"已添加 {added} 个名字";
                SnackBarFunction.ShowSnackBarInSettingWindow(message, Wpf.Ui.Controls.ControlAppearance.Success);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "批量添加学生姓名时发生异常");
                SnackBarFunction.ShowSnackBarInSettingWindow(
                    "批量添加失败，可以查看日志了解详情", Wpf.Ui.Controls.ControlAppearance.Danger);
            }
        }

        /// <summary>
        /// 弹出多行输入框。优先用设置窗口自带的 ContentDialog（外观统一），拿不到宿主窗口时用一个简单的小窗口兜底。
        /// </summary>
        /// <returns>用户输入的内容；取消时返回 null。</returns>
        private async Task<string> ShowBulkInputAsync()
        {
            var hint = new Wpf.Ui.Controls.TextBlock
            {
                Margin = new Thickness(0, 8, 0, 0),
                Appearance = Wpf.Ui.Controls.TextColor.Secondary,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Text = BuildBulkHint(string.Empty),
            };

            var textBox = new Wpf.Ui.Controls.TextBox
            {
                AcceptsReturn = true,
                Height = 200,
                Margin = new Thickness(0, 10, 0, 0),
                PlaceholderText = "张三" + Environment.NewLine + "李四" + Environment.NewLine + "王五",
                TextWrapping = TextWrapping.NoWrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            };

            var panel = new StackPanel { MinWidth = 380 };
            panel.Children.Add(new Wpf.Ui.Controls.TextBlock
            {
                Text = "一行一个名字，多余的空白行和已经存在的名字会自动跳过。",
                TextWrapping = TextWrapping.Wrap,
            });
            panel.Children.Add(textBox);
            panel.Children.Add(hint);

            textBox.TextChanged += (_, _) => hint.Text = BuildBulkHint(textBox.Text);

            if (Window.GetWindow(this) is SettingsWindow host && host.RootContentDialogPresenter != null)
            {
                var dialog = new Wpf.Ui.Controls.ContentDialog
                {
                    Title = "批量添加名字",
                    Content = panel,
                    PrimaryButtonText = "添加",
                    CloseButtonText = "取消",
                    DefaultButton = Wpf.Ui.Controls.ContentDialogButton.Primary,
                };
                dialog.DialogHostEx = host.RootContentDialogPresenter;

                // 输入为空时禁用“添加”，避免点出空操作的提示
                dialog.IsPrimaryButtonEnabled = false;
                textBox.TextChanged += (_, _) =>
                {
                    dialog.IsPrimaryButtonEnabled = SplitLines(textBox.Text).Any();
                };

                textBox.Loaded += (_, _) => textBox.Focus();

                return await dialog.ShowAsync() == Wpf.Ui.Controls.ContentDialogResult.Primary
                    ? textBox.Text
                    : null;
            }

            return ShowBulkInputWindow(panel, textBox);
        }

        /// <summary>
        /// 没有设置窗口宿主时的兜底输入窗口（例如独立打开该页面做测试）。
        /// </summary>
        private string ShowBulkInputWindow(StackPanel panel, Wpf.Ui.Controls.TextBox textBox)
        {
            var dialogWindow = new Window
            {
                Title = "批量添加名字",
                Width = 520,
                Height = 420,
                MinWidth = 420,
                MinHeight = 320,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                Content = new Grid { Margin = new Thickness(20) },
            };

            var layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid.SetRow(panel, 1);
            var buttons = new StackPanel
            {
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                Orientation = Orientation.Horizontal,
            };

            var cancel = new Button { Content = "取消", MinWidth = 80 };
            var confirm = new Button
            {
                Content = "添加",
                Margin = new Thickness(8, 0, 0, 0),
                MinWidth = 80,
                IsEnabled = false,
            };
            textBox.TextChanged += (_, _) => confirm.IsEnabled = SplitLines(textBox.Text).Any();
            cancel.Click += (_, _) => dialogWindow.DialogResult = false;
            confirm.Click += (_, _) => dialogWindow.DialogResult = true;
            buttons.Children.Add(cancel);
            buttons.Children.Add(confirm);

            Grid.SetRow(buttons, 2);
            layout.Children.Add(panel);
            layout.Children.Add(buttons);
            dialogWindow.Content = layout;
            textBox.Loaded += (_, _) => textBox.Focus();

            return dialogWindow.ShowDialog() == true ? textBox.Text : null;
        }

        private static IEnumerable<string> SplitLines(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Enumerable.Empty<string>();
            }

            return text
                .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0);
        }

        private static string BuildBulkHint(string text)
        {
            int total = SplitLines(text).Count();
            if (total == 0)
            {
                return "支持从表格、聊天记录里直接复制多行粘贴过来。";
            }

            var existing = GlobalVariablesData.config.AllSettings.Name;
            int duplicate = existing == null
                ? 0
                : SplitLines(text).Count(line => existing.Contains(line, StringComparer.Ordinal));

            return duplicate > 0
                ? $"共 {total} 个名字，其中 {duplicate} 个已经在名单里，会被跳过。"
                : $"共 {total} 个名字，确定后一次性添加。";
        }

        // ---------------------------------------------------------------- 从文件导入

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("开始从文本文档导入学生名单");

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "从文本文档导入";
                openFileDialog.Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    Log.Debug("用户选择文件: {FilePath}", openFileDialog.FileName);

                    // 导入是“替换”而不是“追加”，名单非空时先说清楚
                    if (AllNames.Count > 0
                        && !await ConfirmAsync(
                            "导入名单",
                            $"导入会用文件里的名字替换当前名单（现在有 {AllNames.Count} 个名字），确定继续吗？\n\n只想追加的话，可以用「批量添加」，它会自动跳过重复的名字。"))
                    {
                        Log.Debug("用户取消导入，保留原名单");
                        return;
                    }

                    // 先读完文件再动名单，避免文件读失败时把原有名单清空
                    var imported = new List<string>();
                    var seen = new HashSet<string>(StringComparer.Ordinal);
                    try
                    {
                        foreach (string line in File.ReadLines(openFileDialog.FileName))
                        {
                            string name = line.Trim();
                            if (name.Length > 0 && seen.Add(name))
                            {
                                imported.Add(name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "读取导入文件时发生异常");
                        MessageBoxFunction.ShowMessageBoxError($"读取文件失败：{ex.Message}");
                        return;
                    }

                    if (imported.Count == 0)
                    {
                        Log.Warning("导入文件里没有可用名字，名单保持不变");
                        SnackBarFunction.ShowSnackBarInSettingWindow(
                            "文件里没有读到名字，名单保持不变", Wpf.Ui.Controls.ControlAppearance.Caution);
                        return;
                    }

                    AllNames.Clear();
                    GlobalVariablesData.config.AllSettings.Name.Clear();
                    foreach (string name in imported)
                    {
                        AllNames.Add(new AllName { Name = name });
                        GlobalVariablesData.config.AllSettings.Name.Add(name);
                    }

                    Log.Information("成功导入 {Count} 个学生姓名", imported.Count);
                    UpdateNameCount();
                    GlobalVariablesData.SaveConfig();
                }
                else
                {
                    Log.Debug("用户取消文件选择");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理文件导入时发生异常");
            }
        }

        // ---------------------------------------------------------------- 单个添加

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                string nameToAdd = AddNameTextBox.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(nameToAdd))
                {
                    Log.Warning("尝试添加空姓名");
                    SnackBarFunction.ShowSnackBarInSettingWindow("大家好，我叫无名氏\n翻译：添加内容为空！", Wpf.Ui.Controls.ControlAppearance.Caution);
                    return;
                }

                (int added, int skipped) = AddNames(new[] { nameToAdd }, "手动添加");

                if (added == 0 && skipped > 0)
                {
                    SnackBarFunction.ShowSnackBarInSettingWindow(
                        $"“{nameToAdd}”已经在名单里了", Wpf.Ui.Controls.ControlAppearance.Caution);
                    return;
                }

                AddNameTextBox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "添加学生姓名时发生异常");
            }
        }

        // ---------------------------------------------------------------- 清空 / 删除 / 示例

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentCount = AllNames.Count;
                if (currentCount == 0)
                {
                    SnackBarFunction.ShowSnackBarInSettingWindow(
                        "名单已经是空的啦", Wpf.Ui.Controls.ControlAppearance.Info);
                    return;
                }

                if (!await ConfirmAsync("清空名单", $"确定要清空全部 {currentCount} 个名字吗？此操作不可撤销。"))
                {
                    Log.Debug("用户取消清空名单");
                    return;
                }

                Log.Warning("清空所有学生名单，当前数量: {Count}", currentCount);

                AllNames.Clear();
                GlobalVariablesData.config.AllSettings.Name.Clear();
                FilterTextBox.Text = string.Empty;
                UpdateNameCount();
                GlobalVariablesData.SaveConfig();

                Log.Information("学生名单已清空");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "清空学生名单时发生异常");
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AllNames.Count != 0)
                {
                    Button button = sender as Button;
                    if (button != null && button.CommandParameter is AllName allnames)
                    {
                        Log.Information("删除学生姓名: {Name}", allnames.Name);

                        AllNames.Remove(allnames);
                        GlobalVariablesData.config.AllSettings.Name.Remove(allnames.Name);
                        GlobalVariablesData.SaveConfig();

                        UpdateNameCount();

                        Log.Debug("姓名删除成功，剩余数量: {Count}", GlobalVariablesData.config.AllSettings.Name.Count);
                    }
                    else
                    {
                        Log.Warning("删除操作获取姓名信息失败");
                    }
                }
                else
                {
                    Log.Debug("学生名单为空，无需删除");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "删除学生姓名时发生异常");
            }
        }

        private void AddNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    Log.Debug("通过回车键触发添加姓名");
                    e.Handled = true;
                    Button_Click_1(sender, e);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "处理回车键添加姓名时发生异常");
            }
        }

        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("添加示例学生名单");

                if (AllNames.Count > 0
                    && !await ConfirmAsync(
                        "使用示例名单",
                        $"示例名单会替换当前名单（现在有 {AllNames.Count} 个名字），确定继续吗？"))
                {
                    Log.Debug("用户取消载入示例名单");
                    return;
                }

                AllNames.Clear();
                GlobalVariablesData.config.AllSettings.Name.Clear();

                List<string> Names = new List<string>
                {
                    "王梓轩", "李若汐", "张峻豪", "陈思妍", "刘翊航",
                    "杨雨桐", "黄皓宇", "周瑾萱", "吴泽楷", "徐婉晴",
                    "孙慕辰", "朱语桐", "何昱珩", "郭婧瑶", "林子墨",
                    "许若琳", "郑皓然", "冯悦彤", "邓嘉树", "蒋舒然",
                    "蔡景行", "沈奕辰", "韩雨萌", "谢明哲", "曹沐妍",
                    "丁昊然", "魏瑾瑜", "苏允哲", "杜心怡", "卢泽言",
                    "叶思远", "潘晨熙", "姚星野", "董一诺", "袁梦琪",
                    "夏子谦", "薛梓萌", "钟雨墨", "谭睿泽", "邵清妍",
                };

                (int added, _) = AddNames(Names, "示例名单");

                SnackBarFunction.ShowSnackBarInSettingWindow(
                    $"已载入 {added} 个示例姓名", Wpf.Ui.Controls.ControlAppearance.Success);

                Log.Information("示例名单添加完成，共 {Count} 个姓名", added);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "添加示例名单时发生异常");
            }
        }

        /// <summary>
        /// 会覆盖名单的操作前弹一次确认框：优先用设置窗口自带的 ContentDialog（外观统一），
        /// 拿不到宿主窗口时退化成系统 MessageBox。
        /// </summary>
        private async Task<bool> ConfirmAsync(string title, string message)
        {
            try
            {
                if (Window.GetWindow(this) is SettingsWindow host && host.RootContentDialogPresenter != null)
                {
                    var dialog = new Wpf.Ui.Controls.ContentDialog
                    {
                        Title = title,
                        Content = new Wpf.Ui.Controls.TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
                        PrimaryButtonText = "确定",
                        CloseButtonText = "取消",
                    };
                    dialog.DialogHostEx = host.RootContentDialogPresenter;
                    return await dialog.ShowAsync() == Wpf.Ui.Controls.ContentDialogResult.Primary;
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "弹出确认框失败，改用系统对话框");
            }

            return System.Windows.MessageBox.Show(
                       message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning)
                   == MessageBoxResult.Yes;
        }

        private void AddNameTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            GlobalVariablesData.SaveConfig();
        }
    }
}
