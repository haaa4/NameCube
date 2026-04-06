using NameCube.GlobalVariables.DataClass;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Brushes = System.Windows.Media.Brushes;

namespace NameCube.ToolBox.AutomaticProcessPages
{
    /// <summary>
    /// SchedulePage.xaml 的交互逻辑
    /// </summary>
    public partial class SchedulePage : Page
    {
        // 命名规则冲突，之后把他改了
        public ObservableCollection<string> picker1 { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> picker2 { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> picker3 { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> processesGroupList { get; } = new ObservableCollection<string>();

        private List<ProcessGroup> processGroups { get; } = new List<ProcessGroup>();
        private List<ProcessGroup> ToChooseProcessGroup = new List<ProcessGroup>();
        private List<int> keyList = new List<int>();
        public List<int> indexList = new List<int>();

        public ObservableCollection<string> processesChooseList { get; } = new ObservableCollection<string>();

        private int indexToHour(int index) => index / 3600;
        private int indexToMinute(int index) => index % 3600 / 60;
        private int indexToSecond(int index) => index % 3600 % 60;
        private int TimeToIndex(int hour, int minute, int second) => hour * 3600 + minute * 60 + second;

        public SchedulePage()
        {
            Log.Information("初始化时间表页面");
            InitializeComponent();
            DataContext = this;

            // 初始化时间选择器
            for (int i = 0; i <= 23; i++) picker1.Add(i.ToString());
            for (int i = 0; i <= 59; i++)
            {
                picker2.Add(i.ToString());
                picker3.Add(i.ToString());
            }

            RefreshList();
            RefreshChooesList();
            Log.Information("时间表页面初始化完成");
        }

        private string IndexToTimeString(int index)
        {
            int hour = indexToHour(index);
            int minute = indexToMinute(index);
            int second = indexToSecond(index);
            return $"{hour:00}:{minute:00}:{second:00}";
        }

        private void RefreshList()
        {
            Log.Debug("刷新时间表列表");
            keyList.Clear();
            indexList.Clear();
            processesGroupList.Clear();
            processGroups.Clear();

            if (GlobalVariablesData.config.AutomaticProcess.processesSchedule == null)
            {
                Log.Debug("时间表为空，创建新字典");
                GlobalVariablesData.config.AutomaticProcess.processesSchedule = new Dictionary<int, List<ProcessGroupUid>>();
            }

            int scheduleCount = 0;
            // 注意：遍历时可能修改字典，使用 ToList() 创建副本
            foreach (var kvp in GlobalVariablesData.config.AutomaticProcess.processesSchedule.ToList())
            {
                int key = kvp.Key;
                List<ProcessGroupUid> uidList = kvp.Value;

                // 从后向前遍历，以便安全删除
                for (int i = uidList.Count - 1; i >= 0; i--)
                {
                    ProcessGroupUid uidObj = uidList[i];
                    ProcessGroup pg = uidObj.GetProcessGroup();

                    if (pg == null)
                    {
                      
                        Log.Warning("时间表项对应的流程组已被删除 (UID: {Uid})，已自动移除该项", uidObj.uid);
                        SnackBarFunction.ShowSnackBarInToolBoxWindow("未找到"+uidObj.uid+"对应的流程组，已自动删除该时间表项",Wpf.Ui.Controls.ControlAppearance.Caution);
                        uidList.RemoveAt(i);
    
                    }
                    else
                    {
                        // 有效项：添加到显示列表
                        string timeString = IndexToTimeString(key);
                        processesGroupList.Add(timeString + " " + pg.name);
                        processGroups.Add(pg);
                        keyList.Add(key);
                        indexList.Add(i);   // 注意：此时 i 是删除无效项后的新索引
                        scheduleCount++;
                    }
                }

                // 如果该时间点下所有项都被删除，则移除整个 Key
                if (uidList.Count == 0)
                {
                    GlobalVariablesData.config.AutomaticProcess.processesSchedule.Remove(key);
                }
            }

            processesGroupList.Add("新建时间流程表...");
            Log.Information("时间表列表刷新完成，有效计划数: {ScheduleCount}", scheduleCount);
        }

        private void RefreshChooesList()
        {
            Log.Debug("刷新可选择的流程组列表");
            processesChooseList.Clear();
            ToChooseProcessGroup.Clear();

            int processGroupCount = GlobalVariablesData.config.AutomaticProcess.processGroups?.Count ?? 0;
            Log.Information("当前有 {ProcessGroupCount} 个流程组可用", processGroupCount);

            foreach (ProcessGroup processGroup in GlobalVariablesData.config.AutomaticProcess.processGroups)
            {
                processesChooseList.Add(processGroup.name);
                ToChooseProcessGroup.Add(processGroup);
            }
        }

        private void TimeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Log.Debug("时间表列表选择改变，当前选中项索引: {SelectedIndex}", TimeListView.SelectedIndex);

            if (TimeListView.SelectedIndex == processesGroupList.Count - 1)
            {
                Log.Debug("用户选择'新建时间流程表...'");
                Part2.IsEnabled = true;
                TimeListView.UnselectAll();
                TimePicker1.SelectedIndex = -1;
                TimePicker2.SelectedIndex = -1;
                TimePicker3.SelectedIndex = -1;
                ProcessesPicker.SelectedIndex = -1;
                SaveButton.IsEnabled = false;
                AddButton.IsEnabled = true;
            }
            else if (TimeListView.SelectedIndex != -1)
            {
                int selectedIndex = TimeListView.SelectedIndex;
                int key = keyList[selectedIndex];
                int idx = indexList[selectedIndex];
                ProcessGroup processGroup = processGroups[selectedIndex]; // 直接从缓存中获取，已在RefreshList中保证有效

                Log.Information("用户选择时间表项: {Time} {ProcessGroupName}, UID: {Uid}",
                    IndexToTimeString(key), processGroup.name, processGroup.uid);

                Part2.IsEnabled = true;
                TimePicker1.SelectedIndex = indexToHour(key);
                TimePicker2.SelectedIndex = indexToMinute(key);
                TimePicker3.SelectedIndex = indexToSecond(key);

                canDelMore = false;
                ProcessesPicker.SelectedIndex = -1;
                ProcessesPicker.SelectedItem = processGroup.name;
                SaveButton.IsEnabled = true;
                AddButton.IsEnabled = false;
                canDelMore = true;
            }
            else
            {
                return;
            }

            ProcessesGroupTextBlock.Foreground = Brushes.Black;
            HourTextBlock.Foreground = Brushes.Black;
            MinutesTextBlock.Foreground = Brushes.Black;
            SecondTextBlock.Foreground = Brushes.Black;
        }

        private bool canDelMore = true;

        private bool HaveTheSame(ProcessGroup processGroup)
        {
            foreach (var group in GlobalVariablesData.config.AutomaticProcess.processGroups)
            {
                if (processGroup.uid == group.uid) return true;
            }
            return false;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击添加时间表按钮");
            ProcessesGroupTextBlock.Foreground = Brushes.Black;
            HourTextBlock.Foreground = Brushes.Black;
            MinutesTextBlock.Foreground = Brushes.Black;
            SecondTextBlock.Foreground = Brushes.Black;
            bool canAdd = true;

            if (ProcessesPicker.SelectedIndex == -1)
            {
                ProcessesGroupTextBlock.Foreground = Brushes.Red;
                canAdd = false;
                Log.Warning("未选择流程组");
            }
            if (TimePicker1.SelectedIndex == -1)
            {
                HourTextBlock.Foreground = Brushes.Red;
                canAdd = false;
                Log.Warning("未选择小时");
            }
            if (TimePicker2.SelectedIndex == -1)
            {
                MinutesTextBlock.Foreground = Brushes.Red;
                canAdd = false;
                Log.Warning("未选择分钟");
            }
            if (TimePicker3.SelectedIndex == -1)
            {
                SecondTextBlock.Foreground = Brushes.Red;
                canAdd = false;
                Log.Warning("未选择秒");
            }

            if (!canAdd) return;

            int timeKey = TimeToIndex(TimePicker1.SelectedIndex, TimePicker2.SelectedIndex, TimePicker3.SelectedIndex);
            ProcessGroup selectedGroup = GlobalVariablesData.config.AutomaticProcess.processGroups[ProcessesPicker.SelectedIndex];

            ProcessGroupUid uidObj = new ProcessGroupUid { uid = selectedGroup.uid };

            Log.Information("添加时间表: 时间 {Hour:00}:{Minute:00}:{Second:00}, 流程组: {ProcessGroupName} (UID:{Uid})",
                TimePicker1.SelectedIndex, TimePicker2.SelectedIndex, TimePicker3.SelectedIndex,
                selectedGroup.name, selectedGroup.uid);

            if (GlobalVariablesData.config.AutomaticProcess.processesSchedule.ContainsKey(timeKey))
            {
                GlobalVariablesData.config.AutomaticProcess.processesSchedule[timeKey].Add(uidObj);
            }
            else
            {
                GlobalVariablesData.config.AutomaticProcess.processesSchedule.Add(timeKey, new List<ProcessGroupUid> { uidObj });
            }

            GlobalVariablesData.SaveConfig();
            Part2.IsEnabled = false;
            RefreshList();
            RefreshChooesList();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TimeListView.MaxHeight = e.NewSize.Height - 60;
            Log.Debug("页面大小改变，新高度: {NewHeight}, 设置列表最大高度: {MaxHeight}",
                e.NewSize.Height, e.NewSize.Height - 60);
        }

        private void ProcessesPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 不再需要处理“*”项，因为已删除项不会出现在选择列表中
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击保存时间表修改");

            int selected = TimeListView.SelectedIndex;
            if (selected < 0 || selected >= processGroups.Count)
            {
                Log.Warning("保存失败：无效的选中项");
                return;
            }

            int oldKey = keyList[selected];
            int idx = indexList[selected];
            ProcessGroup oldGroup = processGroups[selected];
            ProcessGroupUid oldUid = GlobalVariablesData.config.AutomaticProcess.processesSchedule[oldKey][idx];

            int newKey = TimeToIndex(TimePicker1.SelectedIndex, TimePicker2.SelectedIndex, TimePicker3.SelectedIndex);
            ProcessGroup newGroup = null;
            if (ProcessesPicker.SelectedIndex >= 0)
            {
                newGroup = ToChooseProcessGroup[ProcessesPicker.SelectedIndex];
            }

            // 移除原项
            GlobalVariablesData.config.AutomaticProcess.processesSchedule[oldKey].RemoveAt(idx);
            if (GlobalVariablesData.config.AutomaticProcess.processesSchedule[oldKey].Count == 0)
            {
                GlobalVariablesData.config.AutomaticProcess.processesSchedule.Remove(oldKey);
            }

            // 决定是否添加新项（如果新时间或新流程组有变化）
            bool shouldAdd = false;
            ProcessGroupUid newUid = null;

            if (newGroup != null)
            {
                // 检查是否真的发生了变化（时间或流程组不同）
                if (newKey != oldKey || newGroup.uid != oldGroup.uid)
                {
                    shouldAdd = true;
                    newUid = new ProcessGroupUid { uid = newGroup.uid };
                    Log.Information("修改时间表: 从 {OldTime} {OldProcess} 修改为 {NewTime} {NewProcess} (UID:{NewUid})",
                        IndexToTimeString(oldKey), oldGroup.name,
                        IndexToTimeString(newKey), newGroup.name, newGroup.uid);
                }
                else
                {
                    Log.Information("时间与流程组均未变化，无需保存");
                }
            }
            else
            {
                Log.Warning("新选择的流程组无效，仅删除原项");
            }

            if (shouldAdd && newUid != null)
            {
                if (GlobalVariablesData.config.AutomaticProcess.processesSchedule.ContainsKey(newKey))
                {
                    GlobalVariablesData.config.AutomaticProcess.processesSchedule[newKey].Add(newUid);
                }
                else
                {
                    GlobalVariablesData.config.AutomaticProcess.processesSchedule.Add(newKey, new List<ProcessGroupUid> { newUid });
                }
            }

            GlobalVariablesData.SaveConfig();
            RefreshList();
            RefreshChooesList();
            Part2.IsEnabled = false;
        }

        private void ListViewContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (TimeListView.SelectedItem != null && TimeListView.SelectedIndex != processesGroupList.Count - 1)
            {
                ListViewContextMenu.Visibility = Visibility.Visible;
                Log.Debug("显示时间表上下文菜单");
            }
            else
            {
                ListViewContextMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户请求删除时间表项");
            if (TimeListView.SelectedItem != null && TimeListView.SelectedIndex != processesGroupList.Count - 1)
            {
                int selected = TimeListView.SelectedIndex;
                ProcessGroup processGroup = processGroups[selected];
                Log.Information("删除时间表项: {Time} {ProcessGroupName}",
                    IndexToTimeString(keyList[selected]), processGroup.name);

                GlobalVariablesData.config.AutomaticProcess.processesSchedule[keyList[selected]].RemoveAt(indexList[selected]);
                if (GlobalVariablesData.config.AutomaticProcess.processesSchedule[keyList[selected]].Count == 0)
                {
                    GlobalVariablesData.config.AutomaticProcess.processesSchedule.Remove(keyList[selected]);
                }
                GlobalVariablesData.SaveConfig();
                RefreshList();
                RefreshChooesList();
                Part2.IsEnabled = false;
            }
        }
    }
}