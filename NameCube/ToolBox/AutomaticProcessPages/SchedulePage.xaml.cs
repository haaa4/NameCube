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
            InitializeComponent();
            DataContext = this;
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
            keyList.Clear();
            indexList.Clear();
            processesGroupList.Clear();
            processGroups.Clear();
            if (GlobalVariables.json.automaticProcess.processesSchedule == null)
            {
                GlobalVariables.json.automaticProcess.processesSchedule =
                    new Dictionary<int, List<ProcessGroup>>();
            }
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
                        processesGroupList.Add(
                            IndexToTimeString(key)
                                + " "
                                + GlobalVariables
                                    .json
                                    .automaticProcess
                                    .processesSchedule[key][i]
                                    .name
                        );
                        processGroups.Add(
                            GlobalVariables.json.automaticProcess.processesSchedule[key][i]
                        );
                        keyList.Add(key);
                        indexList.Add(i);
                    }
                }
            }
            processesGroupList.Add("新建时间流程表...");
        }

        private void RefreshChooesList()
        {
            processesChooseList.Clear();
            ToChooseProcessGroup.Clear();
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
                Part2.IsEnabled = true;
                int selectedIndex = TimeListView.SelectedIndex;
                int index = keyList[selectedIndex];
                TimePicker1.SelectedIndex = indexToHour(index);
                TimePicker2.SelectedIndex = indexToMinute(index);
                TimePicker3.SelectedIndex = indexToSecond(index);
                ProcessGroup processGroup = GlobalVariables.json.automaticProcess.processesSchedule[
                    keyList[selectedIndex]
                ][indexList[selectedIndex]];
                canDelMore = false;
                ProcessesPicker.SelectedIndex = -1;
                ProcessesPicker.SelectedItem = processGroup.name;
                if (ProcessesPicker.SelectedItem == null || !HaveTheSame(processGroup))
                {
                    processesChooseList.Add("*" + processGroup.name + "(已删除)");
                    ProcessesPicker.SelectedItem = "*" + processGroup.name + "(已删除)";
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
            ProcessesGroupTextBlock.Foreground = Brushes.Black;
            HourTextBlock.Foreground = Brushes.Black;
            MinutesTextBlock.Foreground = Brushes.Black;
            SecondTextBlock.Foreground = Brushes.Black;
            bool canAdd = true;
            if (ProcessesPicker.SelectedIndex == -1)
            {
                ProcessesGroupTextBlock.Foreground = Brushes.Red;
                canAdd = false;
            }
            if (TimePicker1.SelectedIndex == -1)
            {
                HourTextBlock.Foreground = Brushes.Red;
                canAdd = false;
            }
            if (TimePicker2.SelectedIndex == -1)
            {
                MinutesTextBlock.Foreground = Brushes.Red;
                canAdd = false;
            }
            if (TimePicker3.SelectedIndex == -1)
            {
                SecondTextBlock.Foreground = Brushes.Red;
                canAdd = false;
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
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessesPicker.SelectedItem.ToString()[0] == '*')
            {
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
            }
            else
            {
                ListViewContextMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (
                TimeListView.SelectedItem != null
                && TimeListView.SelectedIndex != processesGroupList.Count
            )
            {
                int selected = TimeListView.SelectedIndex;
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
