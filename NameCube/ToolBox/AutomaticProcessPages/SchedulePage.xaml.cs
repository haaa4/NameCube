using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Serilog;

namespace NameCube.ToolBox.AutomaticProcessPages
{
    /// <summary>
    /// SchedulePage.xaml 的交互逻辑
    /// </summary>
    public partial class SchedulePage : Page
    {
        //命名规则冲突，之后把他改了
        public ObservableCollection<string> picker1 { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> picker2 { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> picker3 { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> processesGroupList { get; } =
            new ObservableCollection<string>();
        private List<ProcessGroup> processGroups { get; } = new List<ProcessGroup>();
        private List<ProcessGroup> ToChooseProcessGroup = new List<ProcessGroup>();
        private List<int> keyList = new List<int>();
        public List<int> indexList = new List<int>();
        public ObservableCollection<string> processesChooseList { get; } =
            new ObservableCollection<string>();

        private int indexToHour(int index)
        {
            return index / 3600;
        }

        private int indexToMinute(int index)
        {
            return index % 3600 / 60;
        }

        private int indexToSecond(int index)
        {
            return index % 3600 % 60;
        }

        private int TimeToIndex(int hour, int minute, int second)
        {
            return hour * 3600 + minute * 60 + second;
        }

        public SchedulePage()
        {
            Log.Information("初始化时间表页面");
            InitializeComponent();
            DataContext = this;

            // 初始化时间选择器
            for (int i = 0; i <= 23; i++)
            {
                picker1.Add(i.ToString());
            }
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
            string hourStr,
                minuteStr,
                secondStr;
            if (hour <= 9)
            {
                hourStr = "0" + hour.ToString();
            }
            else
            {
                hourStr = hour.ToString();
            }
            int minute = indexToMinute(index);
            if (minute <= 9)
            {
                minuteStr = "0" + minute.ToString();
            }
            else
            {
                minuteStr = minute.ToString();
            }
            int second = indexToSecond(index);
            if (second <= 9)
            {
                secondStr = "0" + second.ToString();
            }
            else
            {
                secondStr = second.ToString();
            }
            return hourStr + ":" + minuteStr + ":" + secondStr;
        }

        private void RefreshList()
        {
            Log.Debug("刷新时间表列表");
            keyList.Clear();
            indexList.Clear();
            processesGroupList.Clear();
            processGroups.Clear();

            if (GlobalVariables.json.automaticProcess.processesSchedule == null)
            {
                Log.Debug("时间表为空，创建新字典");
                GlobalVariables.json.automaticProcess.processesSchedule =
                    new Dictionary<int, List<ProcessGroup>>();
            }

            int scheduleCount = 0;
            for (int key = 0; key <= 86399; key++)
            {
                if (GlobalVariables.json.automaticProcess.processesSchedule.ContainsKey(key))
                {
                    for (
                        int i = 0;
                        i < GlobalVariables.json.automaticProcess.processesSchedule[key].Count;
                        i++
                    )
                    {
                        string timeString = IndexToTimeString(key);
                        string processName = GlobalVariables
                            .json
                            .automaticProcess
                            .processesSchedule[key][i]
                            .name;

                        processesGroupList.Add(timeString + " " + processName);
                        processGroups.Add(
                            GlobalVariables.json.automaticProcess.processesSchedule[key][i]
                        );
                        keyList.Add(key);
                        indexList.Add(i);
                        scheduleCount++;
                    }
                }
            }
            processesGroupList.Add("新建时间流程表...");
            Log.Information("时间表列表刷新完成，共 {ScheduleCount} 个计划", scheduleCount);
        }

        private void RefreshChooesList()
        {
            Log.Debug("刷新可选择的流程组列表");
            processesChooseList.Clear();
            ToChooseProcessGroup.Clear();

            int processGroupCount = GlobalVariables.json.automaticProcess.processGroups?.Count ?? 0;
            Log.Information("当前有 {ProcessGroupCount} 个流程组可用", processGroupCount);

            foreach (
                ProcessGroup processGroup in GlobalVariables.json.automaticProcess.processGroups
            )
            {
                processesChooseList.Add(processGroup.name);
                ToChooseProcessGroup.Add(processGroup);
            }
        }

        private void TimeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Log.Debug("时间表列表选择改变，当前选中项索引: {SelectedIndex}", TimeListView.SelectedIndex);
            RefreshChooesList();

            if (
                processesChooseList.Count > 0
                && processesChooseList[processesChooseList.Count - 1][0] == '*'
            )
            {
                processesChooseList.RemoveAt(processesChooseList.Count - 1);
            }

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
                int index = keyList[selectedIndex];
                ProcessGroup processGroup = GlobalVariables.json.automaticProcess.processesSchedule[
                    keyList[selectedIndex]
                ][indexList[selectedIndex]];

                Log.Information("用户选择时间表项: {Time} {ProcessGroupName}, UID: {Uid}",
                    IndexToTimeString(index), processGroup.name, processGroup.uid);

                Part2.IsEnabled = true;
                TimePicker1.SelectedIndex = indexToHour(index);
                TimePicker2.SelectedIndex = indexToMinute(index);
                TimePicker3.SelectedIndex = indexToSecond(index);

                canDelMore = false;
                ProcessesPicker.SelectedIndex = -1;
                ProcessesPicker.SelectedItem = processGroup.name;

                if (ProcessesPicker.SelectedItem == null || !HaveTheSame(processGroup))
                {
                    processesChooseList.Add("*" + processGroup.name + "(已删除)");
                    ProcessesPicker.SelectedItem = "*" + processGroup.name + "(已删除)";
                    Log.Warning("选择的流程组已被删除或不存在: {ProcessGroupName}", processGroup.name);
                }

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
            for (int i = 0; i < GlobalVariables.json.automaticProcess.processGroups.Count; i++)
            {
                if (processGroup.uid == GlobalVariables.json.automaticProcess.processGroups[i].uid)
                {
                    return true;
                }
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

            if (canAdd)
            {
                if (
                    processesChooseList.Count > 0
                    && processesChooseList[processesChooseList.Count - 1][0] == '*'
                )
                {
                    processesChooseList.RemoveAt(processesChooseList.Count - 1);
                }

                int timeKey = TimeToIndex(
                    TimePicker1.SelectedIndex,
                    TimePicker2.SelectedIndex,
                    TimePicker3.SelectedIndex
                );
                ProcessGroup processGroup = GlobalVariables.json.automaticProcess.processGroups[
                    ProcessesPicker.SelectedIndex
                ];

                Log.Information("添加时间表: 时间 {Hour:00}:{Minute:00}:{Second:00}, 流程组: {ProcessGroupName}",
                    TimePicker1.SelectedIndex, TimePicker2.SelectedIndex, TimePicker3.SelectedIndex,
                    processGroup.name);

                if (GlobalVariables.json.automaticProcess.processesSchedule.ContainsKey(timeKey))
                {
                    GlobalVariables
                        .json.automaticProcess.processesSchedule[timeKey]
                        .Add(processGroup);
                }
                else
                {
                    GlobalVariables.json.automaticProcess.processesSchedule.Add(
                        timeKey,
                        new List<ProcessGroup>() { processGroup }
                    );
                }

                GlobalVariables.SaveJson();
                Part2.IsEnabled = false;
                RefreshList();
                RefreshChooesList();
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TimeListView.MaxHeight = e.NewSize.Height - 60;
            Log.Debug("页面大小改变，新高度: {NewHeight}, 设置列表最大高度: {MaxHeight}",
                e.NewSize.Height, e.NewSize.Height - 60);
        }

        private void ProcessesPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (
                canDelMore
                && ProcessesPicker.SelectedIndex != -1
                && ProcessesPicker.SelectedItem.ToString() != ""
                && processesChooseList[processesChooseList.Count - 1][0] == '*'
            )
            {
                processesChooseList.RemoveAt(processesChooseList.Count - 1);
                Log.Debug("从选择器中移除了已删除的流程组标记");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击保存时间表修改");

            if (ProcessesPicker.SelectedItem.ToString()[0] == '*')
            {
                Log.Warning("尝试保存已删除的流程组");
                int selected = TimeListView.SelectedIndex;
                ProcessGroup processGroup = processGroups[selected];
                GlobalVariables
                    .json.automaticProcess.processesSchedule[keyList[selected]]
                    .RemoveAt(indexList[selected]);
                int newKey = TimeToIndex(
                    TimePicker1.SelectedIndex,
                    TimePicker2.SelectedIndex,
                    TimePicker3.SelectedIndex
                );
                if (GlobalVariables.json.automaticProcess.processesSchedule.ContainsKey(newKey))
                {
                    GlobalVariables
                        .json.automaticProcess.processesSchedule[newKey]
                        .Add(processGroup);
                }
                else
                {
                    GlobalVariables.json.automaticProcess.processesSchedule.Add(
                        newKey,
                        new List<ProcessGroup>() { processGroup }
                    );
                }
            }
            else
            {
                int selected = TimeListView.SelectedIndex;
                ProcessGroup processGroup = processGroups[selected];
                ProcessGroup newProcessGroup = ToChooseProcessGroup[ProcessesPicker.SelectedIndex];

                Log.Information("修改时间表: 从 {OldTime} {OldProcess} 修改为 {NewTime} {NewProcess}",
                    IndexToTimeString(keyList[selected]), processGroup.name,
                    IndexToTimeString(TimeToIndex(
                        TimePicker1.SelectedIndex,
                        TimePicker2.SelectedIndex,
                        TimePicker3.SelectedIndex
                    )), newProcessGroup.name);

                GlobalVariables
                    .json.automaticProcess.processesSchedule[keyList[selected]]
                    .RemoveAt(indexList[selected]);
                int newKey = TimeToIndex(
                    TimePicker1.SelectedIndex,
                    TimePicker2.SelectedIndex,
                    TimePicker3.SelectedIndex
                );
                if (GlobalVariables.json.automaticProcess.processesSchedule.ContainsKey(newKey))
                {
                    GlobalVariables
                        .json.automaticProcess.processesSchedule[newKey]
                        .Add(newProcessGroup);
                }
                else
                {
                    GlobalVariables.json.automaticProcess.processesSchedule.Add(
                        newKey,
                        new List<ProcessGroup>() { newProcessGroup }
                    );
                }
            }

            GlobalVariables.SaveJson();
            RefreshList();
            RefreshChooesList();
            Part2.IsEnabled = false;
        }

        private void ListViewContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (
                TimeListView.SelectedItem != null
                && TimeListView.SelectedIndex != processesGroupList.Count
            )
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
            if (
                TimeListView.SelectedItem != null
                && TimeListView.SelectedIndex != processesGroupList.Count
            )
            {
                int selected = TimeListView.SelectedIndex;
                ProcessGroup processGroup = processGroups[selected];

                Log.Information("删除时间表项: {Time} {ProcessGroupName}",
                    IndexToTimeString(keyList[selected]), processGroup.name);

                GlobalVariables
                    .json.automaticProcess.processesSchedule[keyList[selected]]
                    .RemoveAt(indexList[selected]);
                GlobalVariables.SaveJson();
                RefreshList();
                RefreshChooesList();
                Part2.IsEnabled = false;
            }
        }
    }
}