using Microsoft.Win32;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using NameCube.Function;
using System.Windows.Controls;
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
                        WarningInfoBar.IsOpen = true;
                    }
                }
                else
                {
                    Log.Debug("学生名单为空，初始化空列表");
                    GlobalVariablesData.config.AllSettings.Name = new List<string>();
                }

                Log.Information("Archives页面初始化完成，显示 {Count} 个姓名", AllNames.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Archives页面初始化时发生异常");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
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

                    AllNames.Clear();
                    GlobalVariablesData.config.AllSettings.Name.Clear();
                    int importedCount = 0;

                    try
                    {
                        foreach (string line in File.ReadLines(openFileDialog.FileName))
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                AllNames.Add(new AllName { Name = line.Trim() });
                                GlobalVariablesData.config.AllSettings.Name.Add(line.Trim());
                                importedCount++;
                            }
                        }

                        Log.Information("成功导入 {Count} 个学生姓名", importedCount);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "导入文件时发生异常");
                        MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    }

                    if (GlobalVariablesData.config.AllSettings.Name.Count >= 120)
                    {
                        Log.Warning("导入后学生名单数量达到 {Count}，显示警告", GlobalVariablesData.config.AllSettings.Name.Count);
                        WarningInfoBar.IsOpen = true;
                    }
                    else
                    {
                        WarningInfoBar.IsOpen = false;
                    }

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

                Log.Information("添加学生姓名: {Name}", nameToAdd);

                AllNames.Add(new AllName { Name = nameToAdd });
                GlobalVariablesData.config.AllSettings.Name.Add(nameToAdd);
                AddNameTextBox.Text = "";

                if (GlobalVariablesData.config.AllSettings.Name.Count >= 120)
                {
                    Log.Warning("添加后学生名单数量达到 {Count}，显示警告", GlobalVariablesData.config.AllSettings.Name.Count);
                    WarningInfoBar.IsOpen = true;
                }

                GlobalVariablesData.SaveConfig();
                Log.Debug("姓名添加成功，当前总数: {Count}", GlobalVariablesData.config.AllSettings.Name.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "添加学生姓名时发生异常");
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                int currentCount = AllNames.Count;
                Log.Warning("清空所有学生名单，当前数量: {Count}", currentCount);

                AllNames.Clear();
                GlobalVariablesData.config.AllSettings.Name.Clear();
                WarningInfoBar.IsOpen = false;
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

                        if (GlobalVariablesData.config.AllSettings.Name.Count < 120)
                        {
                            WarningInfoBar.IsOpen = false;
                        }

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

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("添加示例学生名单");

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

                foreach (string name in Names)
                {
                    AllNames.Add(new AllName { Name = name });
                    GlobalVariablesData.config.AllSettings.Name.Add(name);
                }

                if (GlobalVariablesData.config.AllSettings.Name.Count >= 120)
                {
                    Log.Warning("示例名单数量达到 {Count}，显示警告", GlobalVariablesData.config.AllSettings.Name.Count);
                    WarningInfoBar.IsOpen = true;
                }
                else
                {
                    WarningInfoBar.IsOpen = false;
                }

                GlobalVariablesData.SaveConfig();
                Log.Information("示例名单添加完成，共 {Count} 个姓名", Names.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "添加示例名单时发生异常");
            }
        }

        private void AddNameTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            GlobalVariablesData.SaveConfig();
        }
    }
}