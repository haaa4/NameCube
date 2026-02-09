using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Serilog;

namespace NameCube.Mode
{
    /// <summary>
    /// BatchMode.xaml 的交互逻辑
    /// </summary>
    public partial class BatchMode : Page
    {
        public ObservableCollection<AllName> AllNames { get; set; } = new ObservableCollection<AllName>();
        public class AllName
        {
            public string Name { get; set; }
        }

        public BatchMode()
        {
            InitializeComponent();
            DataContext = this;
            Log.Debug("BatchMode页面初始化完成");
        }

        private void NameSwitch_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("点击名称切换开关，当前状态: {IsChecked}", NameSwitch.IsChecked.Value);

            GlobalVariablesData.config.BatchModeSettings.NumberMode = NameSwitch.IsChecked.Value;
            Numberbox.IsEnabled = NameSwitch.IsChecked.Value;
            if (!GlobalVariablesData.config.BatchModeSettings.NumberMode)
            {
                IndexText.Visibility = Visibility.Collapsed;
                Numberbox.Visibility = Visibility.Collapsed;
                Log.Debug("切换到姓名模式");
            }
            else
            {
                IndexText.Visibility = Visibility.Visible;
                Numberbox.Visibility = Visibility.Visible;
                Log.Debug("切换到数字模式，数量: {Number}", GlobalVariablesData.config.BatchModeSettings.Number);
            }
            GlobalVariablesData.SaveConfig();
        }

        private void Numberbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int originalValue = GlobalVariablesData.config.BatchModeSettings.Number;
            GlobalVariablesData.config.BatchModeSettings.Number = Numberbox.Text.ToInt32(-114514);

            if (GlobalVariablesData.config.BatchModeSettings.Number == -114514)
            {
                Log.Warning("数字输入无效，重置为默认值53");
                GlobalVariablesData.config.BatchModeSettings.Number = 53;
                Numberbox.Value = 53;
            }
            else
            {
                Log.Debug("数字输入变化: {Original} -> {New}", originalValue, GlobalVariablesData.config.BatchModeSettings.Number);
            }

            GlobalVariablesData.SaveConfig();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("开始批量抽取操作");
                StartButton.IsEnabled = false;
                Numberbox.IsEnabled = false;
                NameSwitch.IsEnabled = false;
                Indexbox.IsEnabled = false;
                ReCheckBox.IsEnabled = false;
                List<int> numbers = new List<int>();
                AllNames.Clear();

                if (GlobalVariablesData.config.AllSettings.Name.Count <= 2)
                {
                    Log.Warning("学生名单数量不足: {Count} <= 2", GlobalVariablesData.config.AllSettings.Name.Count);
                    SnackBarFunction.ShowSnackBarInMainWindow("学生名单怎么空空的？\n翻译：学生人数少于3人", Wpf.Ui.Controls.ControlAppearance.Caution);
                    ResetButtonStates();
                    return;
                }

                Log.Debug("批量抽取配置: 模式={Mode}, 抽取数量={Index}, 总数={Number}, 允许重复={Repetition}",
                    GlobalVariablesData.config.BatchModeSettings.NumberMode ? "数字模式" : "姓名模式",
                    GlobalVariablesData.config.BatchModeSettings.Index,
                    GlobalVariablesData.config.BatchModeSettings.NumberMode ? GlobalVariablesData.config.BatchModeSettings.Number : GlobalVariablesData.config.AllSettings.Name.Count,
                    GlobalVariablesData.config.BatchModeSettings.Repetition);

                if (GlobalVariablesData.config.BatchModeSettings.NumberMode)
                {
                    if (GlobalVariablesData.config.BatchModeSettings.Index > GlobalVariablesData.config.BatchModeSettings.Number && !GlobalVariablesData.config.BatchModeSettings.Repetition)
                    {
                        Log.Warning("抽取数量大于可抽取数量: {Index} > {Number}",
                            GlobalVariablesData.config.BatchModeSettings.Index,
                            GlobalVariablesData.config.BatchModeSettings.Number);
                        SnackBarFunction.ShowSnackBarInMainWindow("无 中 生 有\n翻译：抽取数量大于实际可抽取数量", Wpf.Ui.Controls.ControlAppearance.Caution);
                        ResetButtonStates();
                        return;
                    }
                    for (int i = 1; i <= GlobalVariablesData.config.BatchModeSettings.Number; i++)
                    {
                        numbers.Add(i);
                    }
                }
                else
                {
                    if (GlobalVariablesData.config.BatchModeSettings.Index > GlobalVariablesData.config.AllSettings.Name.Count && !GlobalVariablesData.config.BatchModeSettings.Repetition)
                    {
                        Log.Warning("抽取数量大于可抽取数量: {Index} > {Count}",
                            GlobalVariablesData.config.BatchModeSettings.Index,
                            GlobalVariablesData.config.AllSettings.Name.Count);
                        SnackBarFunction.ShowSnackBarInMainWindow("无 中 生 有\n翻译：抽取数量大于实际可抽取数量", Wpf.Ui.Controls.ControlAppearance.Caution);
                        ResetButtonStates();
                        return;
                    }
                    for (int i = 1; i <= GlobalVariablesData.config.AllSettings.Name.Count; i++)
                    {
                        numbers.Add(i);
                    }
                }

                Random random = new Random();
                int now;
                List<string> selectedNames = new List<string>();

                for (int i = 1; i <= GlobalVariablesData.config.BatchModeSettings.Index; i++)
                {
                    now = random.StrictNext(numbers.Count);
                    if (GlobalVariablesData.config.BatchModeSettings.NumberMode)
                    {
                        string selectedNumber = numbers[now].ToString();
                        AllNames.Add(new AllName { Name = selectedNumber });
                        selectedNames.Add(selectedNumber);
                        Log.Debug("第 {Index} 次抽取: 数字 {Number}", i, selectedNumber);
                    }
                    else
                    {
                        string selectedName = GlobalVariablesData.config.AllSettings.Name[numbers[now] - 1];
                        AllNames.Add(new AllName { Name = selectedName });
                        selectedNames.Add(selectedName);
                        Log.Debug("第 {Index} 次抽取: 姓名 {Name}", i, selectedName);
                    }
                    if (!GlobalVariablesData.config.BatchModeSettings.Repetition)
                    {
                        numbers.RemoveAt(now);
                    }
                }

                if (!GlobalVariablesData.config.BatchModeSettings.Locked)
                {
                    Numberbox.IsEnabled = true;
                    NameSwitch.IsEnabled = true;
                    Indexbox.IsEnabled = true;
                    ReCheckBox.IsEnabled = true;
                }

                if (GlobalVariablesData.config.BatchModeSettings.LastName != null)
                {
                    GlobalVariablesData.config.BatchModeSettings.LastName.Clear();
                }
                else
                {
                    GlobalVariablesData.config.BatchModeSettings.LastName = new List<string>();
                }

                foreach (AllName allName in AllNames)
                {
                    GlobalVariablesData.config.BatchModeSettings.LastName.Add(allName.Name);
                }

                Log.Information("批量抽取完成: 共抽取 {Count} 个{Type}, 结果: {Results}",
                    selectedNames.Count,
                    GlobalVariablesData.config.BatchModeSettings.NumberMode ? "数字" : "姓名",
                    string.Join(", ", selectedNames));

                StartButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "批量抽取过程中发生异常");
                ResetButtonStates();
            }
        }

        private void ResetButtonStates()
        {
            StartButton.IsEnabled = true;
            Numberbox.IsEnabled = true;
            NameSwitch.IsEnabled = true;
            Indexbox.IsEnabled = true;
            ReCheckBox.IsEnabled = true;
        }

        private void Indexbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int originalValue = GlobalVariablesData.config.BatchModeSettings.Index;
            GlobalVariablesData.config.BatchModeSettings.Index = Indexbox.Text.ToInt32(-114514);

            if (GlobalVariablesData.config.BatchModeSettings.Index == -114514)
            {
                Log.Warning("索引输入无效，重置为默认值10");
                GlobalVariablesData.config.BatchModeSettings.Index = 10;
                Indexbox.Value = 10;
            }
            else
            {
                Log.Debug("索引输入变化: {Original} -> {New}", originalValue, GlobalVariablesData.config.BatchModeSettings.Index);
            }

            GlobalVariablesData.SaveConfig();
        }

        private void ReCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("重复抽取开关: {IsChecked}", ReCheckBox.IsChecked.Value);
            GlobalVariablesData.config.BatchModeSettings.Repetition = ReCheckBox.IsChecked.Value;
            GlobalVariablesData.SaveConfig();
        }

        private void Numberbox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (Numberbox.Value == null)
            {
                Log.Debug("数字框值为空，重置为53");
                Numberbox.Value = 53;
            }
            GlobalVariablesData.config.BatchModeSettings.Number = (int)Numberbox.Value;
            Log.Debug("数字框值变化: {Value}", Numberbox.Value);
            GlobalVariablesData.SaveConfig();
        }

        private void Indexbox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (Indexbox.Value == null)
            {
                Log.Debug("索引框值为空，重置为10");
                Indexbox.Value = 10;
            }
            GlobalVariablesData.config.BatchModeSettings.Index = (int)Indexbox.Value;
            Log.Debug("索引框值变化: {Value}", Indexbox.Value);
            GlobalVariablesData.SaveConfig();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("BatchMode页面加载");
                Numberbox.Value = GlobalVariablesData.config.BatchModeSettings.Number;
                NameSwitch.IsChecked = GlobalVariablesData.config.BatchModeSettings.NumberMode;
                Numberbox.IsEnabled = GlobalVariablesData.config.BatchModeSettings.NumberMode;
                Indexbox.Value = GlobalVariablesData.config.BatchModeSettings.Index;
                ReCheckBox.IsChecked = GlobalVariablesData.config.BatchModeSettings.Repetition;

                if (!GlobalVariablesData.config.BatchModeSettings.NumberMode)
                {
                    IndexText.Visibility = Visibility.Collapsed;
                    Numberbox.Visibility = Visibility.Collapsed;
                    Log.Debug("初始化为姓名模式");
                }

                if (GlobalVariablesData.config.BatchModeSettings.Locked)
                {
                    NameSwitch.IsEnabled = false;
                    Numberbox.IsEnabled = false;
                    Indexbox.IsEnabled = false;
                    ReCheckBox.IsEnabled = false;
                    Log.Debug("页面设置为锁定状态");
                }

                if (GlobalVariablesData.config.BatchModeSettings.LastName != null && GlobalVariablesData.config.BatchModeSettings.LastName.Count != 0)
                {
                    Log.Debug("加载上次抽取结果，数量: {Count}", GlobalVariablesData.config.BatchModeSettings.LastName.Count);
                    foreach (string name in GlobalVariablesData.config.BatchModeSettings.LastName)
                    {
                        AllName allNames = new AllName();
                        allNames.Name = name;
                        AllNames.Add(allNames);
                    }
                }

                Log.Information("BatchMode页面加载完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "BatchMode页面加载时发生异常");
            }
        }
    }
}