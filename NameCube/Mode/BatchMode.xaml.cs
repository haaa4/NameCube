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

            GlobalVariables.json.BatchModeSettings.NumberMode = NameSwitch.IsChecked.Value;
            Numberbox.IsEnabled = NameSwitch.IsChecked.Value;
            if (!GlobalVariables.json.BatchModeSettings.NumberMode)
            {
                IndexText.Visibility = Visibility.Collapsed;
                Numberbox.Visibility = Visibility.Collapsed;
                Log.Debug("切换到姓名模式");
            }
            else
            {
                IndexText.Visibility = Visibility.Visible;
                Numberbox.Visibility = Visibility.Visible;
                Log.Debug("切换到数字模式，数量: {Number}", GlobalVariables.json.BatchModeSettings.Number);
            }
            GlobalVariables.SaveJson();
        }

        private void Numberbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int originalValue = GlobalVariables.json.BatchModeSettings.Number;
            GlobalVariables.json.BatchModeSettings.Number = Numberbox.Text.ToInt32(-114514);

            if (GlobalVariables.json.BatchModeSettings.Number == -114514)
            {
                Log.Warning("数字输入无效，重置为默认值53");
                GlobalVariables.json.BatchModeSettings.Number = 53;
                Numberbox.Value = 53;
            }
            else
            {
                Log.Debug("数字输入变化: {Original} -> {New}", originalValue, GlobalVariables.json.BatchModeSettings.Number);
            }

            GlobalVariables.SaveJson();
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

                if (GlobalVariables.json.AllSettings.Name.Count <= 2)
                {
                    Log.Warning("学生名单数量不足: {Count} <= 2", GlobalVariables.json.AllSettings.Name.Count);
                    SnackBarFunction.ShowSnackBarInMainWindow("学生名单怎么空空的？\n翻译：学生人数少于3人", Wpf.Ui.Controls.ControlAppearance.Caution);
                    ResetButtonStates();
                    return;
                }

                Log.Debug("批量抽取配置: 模式={Mode}, 抽取数量={Index}, 总数={Number}, 允许重复={Repetition}",
                    GlobalVariables.json.BatchModeSettings.NumberMode ? "数字模式" : "姓名模式",
                    GlobalVariables.json.BatchModeSettings.Index,
                    GlobalVariables.json.BatchModeSettings.NumberMode ? GlobalVariables.json.BatchModeSettings.Number : GlobalVariables.json.AllSettings.Name.Count,
                    GlobalVariables.json.BatchModeSettings.Repetition);

                if (GlobalVariables.json.BatchModeSettings.NumberMode)
                {
                    if (GlobalVariables.json.BatchModeSettings.Index > GlobalVariables.json.BatchModeSettings.Number && !GlobalVariables.json.BatchModeSettings.Repetition)
                    {
                        Log.Warning("抽取数量大于可抽取数量: {Index} > {Number}",
                            GlobalVariables.json.BatchModeSettings.Index,
                            GlobalVariables.json.BatchModeSettings.Number);
                        SnackBarFunction.ShowSnackBarInMainWindow("无 中 生 有\n翻译：抽取数量大于实际可抽取数量", Wpf.Ui.Controls.ControlAppearance.Caution);
                        ResetButtonStates();
                        return;
                    }
                    for (int i = 1; i <= GlobalVariables.json.BatchModeSettings.Number; i++)
                    {
                        numbers.Add(i);
                    }
                }
                else
                {
                    if (GlobalVariables.json.BatchModeSettings.Index > GlobalVariables.json.AllSettings.Name.Count && !GlobalVariables.json.BatchModeSettings.Repetition)
                    {
                        Log.Warning("抽取数量大于可抽取数量: {Index} > {Count}",
                            GlobalVariables.json.BatchModeSettings.Index,
                            GlobalVariables.json.AllSettings.Name.Count);
                        SnackBarFunction.ShowSnackBarInMainWindow("无 中 生 有\n翻译：抽取数量大于实际可抽取数量", Wpf.Ui.Controls.ControlAppearance.Caution);
                        ResetButtonStates();
                        return;
                    }
                    for (int i = 1; i <= GlobalVariables.json.AllSettings.Name.Count; i++)
                    {
                        numbers.Add(i);
                    }
                }

                Random random = new Random();
                int now;
                List<string> selectedNames = new List<string>();

                for (int i = 1; i <= GlobalVariables.json.BatchModeSettings.Index; i++)
                {
                    now = random.StrictNext(numbers.Count);
                    if (GlobalVariables.json.BatchModeSettings.NumberMode)
                    {
                        string selectedNumber = numbers[now].ToString();
                        AllNames.Add(new AllName { Name = selectedNumber });
                        selectedNames.Add(selectedNumber);
                        Log.Debug("第 {Index} 次抽取: 数字 {Number}", i, selectedNumber);
                    }
                    else
                    {
                        string selectedName = GlobalVariables.json.AllSettings.Name[numbers[now] - 1];
                        AllNames.Add(new AllName { Name = selectedName });
                        selectedNames.Add(selectedName);
                        Log.Debug("第 {Index} 次抽取: 姓名 {Name}", i, selectedName);
                    }
                    if (!GlobalVariables.json.BatchModeSettings.Repetition)
                    {
                        numbers.RemoveAt(now);
                    }
                }

                if (!GlobalVariables.json.BatchModeSettings.Locked)
                {
                    Numberbox.IsEnabled = true;
                    NameSwitch.IsEnabled = true;
                    Indexbox.IsEnabled = true;
                    ReCheckBox.IsEnabled = true;
                }

                if (GlobalVariables.json.BatchModeSettings.LastName != null)
                {
                    GlobalVariables.json.BatchModeSettings.LastName.Clear();
                }
                else
                {
                    GlobalVariables.json.BatchModeSettings.LastName = new List<string>();
                }

                foreach (AllName allName in AllNames)
                {
                    GlobalVariables.json.BatchModeSettings.LastName.Add(allName.Name);
                }

                Log.Information("批量抽取完成: 共抽取 {Count} 个{Type}, 结果: {Results}",
                    selectedNames.Count,
                    GlobalVariables.json.BatchModeSettings.NumberMode ? "数字" : "姓名",
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
            int originalValue = GlobalVariables.json.BatchModeSettings.Index;
            GlobalVariables.json.BatchModeSettings.Index = Indexbox.Text.ToInt32(-114514);

            if (GlobalVariables.json.BatchModeSettings.Index == -114514)
            {
                Log.Warning("索引输入无效，重置为默认值10");
                GlobalVariables.json.BatchModeSettings.Index = 10;
                Indexbox.Value = 10;
            }
            else
            {
                Log.Debug("索引输入变化: {Original} -> {New}", originalValue, GlobalVariables.json.BatchModeSettings.Index);
            }

            GlobalVariables.SaveJson();
        }

        private void ReCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("重复抽取开关: {IsChecked}", ReCheckBox.IsChecked.Value);
            GlobalVariables.json.BatchModeSettings.Repetition = ReCheckBox.IsChecked.Value;
            GlobalVariables.SaveJson();
        }

        private void Numberbox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (Numberbox.Value == null)
            {
                Log.Debug("数字框值为空，重置为53");
                Numberbox.Value = 53;
            }
            GlobalVariables.json.BatchModeSettings.Number = (int)Numberbox.Value;
            Log.Debug("数字框值变化: {Value}", Numberbox.Value);
            GlobalVariables.SaveJson();
        }

        private void Indexbox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (Indexbox.Value == null)
            {
                Log.Debug("索引框值为空，重置为10");
                Indexbox.Value = 10;
            }
            GlobalVariables.json.BatchModeSettings.Index = (int)Indexbox.Value;
            Log.Debug("索引框值变化: {Value}", Indexbox.Value);
            GlobalVariables.SaveJson();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("BatchMode页面加载");
                Numberbox.Value = GlobalVariables.json.BatchModeSettings.Number;
                NameSwitch.IsChecked = GlobalVariables.json.BatchModeSettings.NumberMode;
                Numberbox.IsEnabled = GlobalVariables.json.BatchModeSettings.NumberMode;
                Indexbox.Value = GlobalVariables.json.BatchModeSettings.Index;
                ReCheckBox.IsChecked = GlobalVariables.json.BatchModeSettings.Repetition;

                if (!GlobalVariables.json.BatchModeSettings.NumberMode)
                {
                    IndexText.Visibility = Visibility.Collapsed;
                    Numberbox.Visibility = Visibility.Collapsed;
                    Log.Debug("初始化为姓名模式");
                }

                if (GlobalVariables.json.BatchModeSettings.Locked)
                {
                    NameSwitch.IsEnabled = false;
                    Numberbox.IsEnabled = false;
                    Indexbox.IsEnabled = false;
                    ReCheckBox.IsEnabled = false;
                    Log.Debug("页面设置为锁定状态");
                }

                if (GlobalVariables.json.BatchModeSettings.LastName != null && GlobalVariables.json.BatchModeSettings.LastName.Count != 0)
                {
                    Log.Debug("加载上次抽取结果，数量: {Count}", GlobalVariables.json.BatchModeSettings.LastName.Count);
                    foreach (string name in GlobalVariables.json.BatchModeSettings.LastName)
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