using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
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
using Masuit.Tools;
using Masuit.Tools.DateTimeExt;
using Windows.ApplicationModel.ConversationalAgent;
using Application = System.Windows.Application;

namespace NameCube.ToolBox.AutomaticProcessPages
{
    public partial class AutomaticProcessSetting : Page
    {
        public ObservableCollection<string> ProcessGroups { get; set; } =
            new ObservableCollection<string>();
        public ObservableCollection<string> ProcessKinds { get; set; } =
            new ObservableCollection<string>();
        private bool canChange = false;
        private bool isUserInteraction = true;
        private ProcessGroup selectedProcessGroup;
        private ProcessData selectedProcessData;

        public AutomaticProcessSetting()
        {
            InitializeComponent();
            DataContext = this;
            if (GlobalVariables.json.automaticProcess.processGroups == null)
            {
                GlobalVariables.json.automaticProcess.processGroups = new List<ProcessGroup>();
            }
            Loaded += AutomaticProcessSetting_Loaded;
        }

        /// <summary>
        /// 刷新流程组列表
        /// </summary>
        /// <param name="selectLastOne">选中最后一个</param>
        private void RefreshList(bool selectLastOne = false)
        {
            canChange = false;
            ProcessGroupsListView.UnselectAll();
            ProcessGroups.Clear();
            foreach (
                ProcessGroup processGroup in GlobalVariables.json.automaticProcess.processGroups
            )
            {
                ProcessGroups.Add(processGroup.name);
            }
            ProcessGroups.Add("新建流程组...");
            canChange = true;
            if (selectLastOne)
            {
                ProcessGroupsListView.SelectedIndex = ProcessGroups.Count - 2;
            }
        }

        private string GetMaxName(string get)
        {
            if (get != null && get.Length > 10)
            {
                return get.Substring(0, 10) + "...";
            }
            else if (get == null)
            {
                return "";
            }
            else
            {
                return get;
            }
        }

        private string GetProcessName(ProcessData processData)
        {
            switch (processData.state)
            {
                case ProcessState.start:
                    if (processData.stringData1 != null)
                    {
                        FileInfo fileInfo = new FileInfo(processData.stringData1);
                        return "运行:" + GetMaxName(fileInfo.Name);
                    }
                    else
                    {
                        return "运行？？？";
                    }
                case ProcessState.audio:
                    if (processData.stringData1 != null)
                    {
                        FileInfo fileInfo1 = new FileInfo(processData.stringData1);
                        return "播放音频:" + GetMaxName(fileInfo1.Name);
                    }
                    else
                    {
                        return "播放音频？？？";
                    }
                case ProcessState.read:
                    return "展示文字:" + GetMaxName(processData.stringData1);
                case ProcessState.cmd:
                    return "执行命令:" + GetMaxName(processData.stringData1);
                case ProcessState.wait:
                    return "等待" + processData.doubleData + "秒";
                case ProcessState.clear:
                    return "清理内存";
                case ProcessState.shutDown:
                    if (processData.doubleData == 0)
                        return "立即关机";
                    else if (processData.doubleData == 1)
                        return "一般关机";
                    else if (processData.doubleData == 2)
                        return "强制关机";
                    else
                        return "?";
                default:
                    return "?";
            }
        }

        private void RefreshProcessKinds(
            ProcessGroup processGroup,
            bool selectedLastOne = false,
            int selected = -1
        )
        {
            canChange = false;
            ProcessesListView.UnselectAll();
            ProcessKinds.Clear();
            foreach (ProcessData processData in processGroup.processDatas)
            {
                ProcessKinds.Add(GetProcessName(processData));
            }
            ProcessKinds.Add("新建流程...");
            canChange = true;
            if (selectedLastOne)
            {
                ProcessesListView.SelectedIndex = ProcessKinds.Count - 2;
            }
            if (selected != -1)
            {
                ProcessesListView.SelectedIndex = selected;
            }
        }

        private void AutomaticProcessSetting_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private async void ProcessGroupsListView_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e
        )
        {
            if (canChange && ProcessGroupsListView.SelectedItem != null)
            {
                string selectedItem = ProcessGroupsListView.SelectedItem.ToString();

                if (selectedItem == "新建流程组...")
                {
                    var dialog = new Wpf.Ui.Controls.ContentDialog()
                    {
                        CloseButtonText = "取消",
                        PrimaryButtonText = "创建",
                        Title = "新建流程组",
                        Content = new Wpf.Ui.Controls.TextBox()
                        {
                            MinWidth = 200,
                            MinHeight = 30,
                            Text = "未命名流程组",
                            PlaceholderText = "请输入流程组名称(不能以“*”开头)",
                        },
                        DialogHost = Host,
                    };
                    Wpf.Ui.Controls.ContentDialogResult contentDialogResult =
                        await dialog.ShowAsync();
                    if (contentDialogResult == Wpf.Ui.Controls.ContentDialogResult.Primary)
                    {
                        if (dialog.Content is Wpf.Ui.Controls.TextBox textBox)
                        {
                            if (textBox.Text != "" && textBox.Text[0] != '*')
                            {
                                ProcessGroup newGroup = new ProcessGroup()
                                {
                                    name = textBox.Text,
                                    processDatas = new List<ProcessData>(),
                                    uid = (int)DateTime.Now.GetTotalMilliseconds(),
                                };
                                GlobalVariables.json.automaticProcess.processGroups.Add(newGroup);
                                GlobalVariables.SaveJson();
                            }
                            else
                            {
                                MessageBoxFunction.ShowMessageBoxWarning("命名不符合规定");
                            }
                            RefreshList(true);
                        }
                    }
                    else
                    {
                        dialog.Hide();
                        RefreshList(true);
                    }
                }
                else
                {
                    selectedProcessGroup = GlobalVariables.json.automaticProcess.processGroups[
                        ProcessGroupsListView.SelectedIndex
                    ];

                    RemindTextTextBox.Text = selectedProcessGroup.remindText;
                    RemindTimeNumberBox.Value = selectedProcessGroup.remindTime;
                    CanCancleCheckBox.IsChecked = selectedProcessGroup.canCancle;
                    ShowCheckBox.IsChecked = selectedProcessGroup.show;
                    Part3.IsEnabled = true;
                    ProcessKindComboBox.IsEnabled = false;
                    MainFrame.Content = null;
                    MainFrame.IsEnabled = false;
                    SaveButton.IsEnabled = true;
                    BrowseButton.IsEnabled = true;
                    RefreshProcessKinds(selectedProcessGroup);
                }
            }
        }

        private void ListContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (
                ProcessGroupsListView.SelectedItem == null
                || ProcessGroupsListView.SelectedItem.ToString() == "新建流程组..."
            )
            {
                ListContextMenu.Visibility = Visibility.Collapsed;
            }
            else
            {
                ListContextMenu.Visibility = Visibility.Visible;
            }
        }

        private async void RenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (
                ProcessGroupsListView.SelectedItem == null
                || ProcessGroupsListView.SelectedItem.ToString() == "新建流程组..."
            )
                return;

            string oldName = ProcessGroupsListView.SelectedItem.ToString();
            var processGroup = GlobalVariables.json.automaticProcess.processGroups.FirstOrDefault(
                pg => pg.name == oldName
            );

            if (processGroup == null)
                return;

            var dialog = new Wpf.Ui.Controls.ContentDialog()
            {
                CloseButtonText = "取消",
                PrimaryButtonText = "确认",
                Title = "重命名流程组",
                Content = new Wpf.Ui.Controls.TextBox()
                {
                    MinWidth = 200,
                    MinHeight = 30,
                    Text = oldName,
                    PlaceholderText = "请输入新的流程组名称",
                },
                DialogHost = Host,
            };

            Wpf.Ui.Controls.ContentDialogResult result = await dialog.ShowAsync();
            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                if (
                    dialog.Content is Wpf.Ui.Controls.TextBox textBox
                    && !string.IsNullOrWhiteSpace(textBox.Text)
                )
                {
                    processGroup.name = textBox.Text;
                    GlobalVariables.SaveJson();
                    RefreshList();
                }
            }
        }

        private async void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (
                ProcessGroupsListView.SelectedItem == null
                || ProcessGroupsListView.SelectedItem.ToString() == "新建流程组..."
            )
                return;

            string itemToDelete = ProcessGroupsListView.SelectedItem.ToString();

            var confirmDialog = new Wpf.Ui.Controls.ContentDialog()
            {
                Title = "确认删除",
                Content =
                    $"确定要删除流程组 '{itemToDelete}' 吗？此操作不可恢复。(时间表与快捷键内的流程组不会改变）",
                PrimaryButtonText = "删除",
                CloseButtonText = "取消",
                DialogHost = Host,
            };

            Wpf.Ui.Controls.ContentDialogResult result = await confirmDialog.ShowAsync();
            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                var processGroup =
                    GlobalVariables.json.automaticProcess.processGroups.FirstOrDefault(pg =>
                        pg.name == itemToDelete
                    );

                if (processGroup != null)
                {
                    GlobalVariables.json.automaticProcess.processGroups.Remove(processGroup);
                    Part3.IsEnabled = false;
                    MainFrame.Content = null;
                    ProcessKinds.Clear();
                    SaveButton.IsEnabled = false;
                    BrowseButton.IsEnabled = false;
                    GlobalVariables.SaveJson();
                    RefreshList();
                }
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ProcessGroupsListView.MaxHeight = e.NewSize.Height / 2 - 60;
            ProcessesListView.MaxHeight = e.NewSize.Height / 2 - 60;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            int findGroup = GlobalVariables.json.automaticProcess.processGroups.IndexOf(
                selectedProcessGroup
            );
            if (findGroup != -1)
            {
                selectedProcessGroup.remindText = RemindTextTextBox.Text;
                selectedProcessGroup.remindTime = (int)RemindTimeNumberBox.Value;
                selectedProcessGroup.canCancle = CanCancleCheckBox.IsChecked ?? false;
                selectedProcessGroup.show=ShowCheckBox.IsChecked ?? true;
                if (ProcessKindComboBox.SelectedItem != null)
                {
                    selectedProcessData.state = IndexToProcessState(
                        ProcessKindComboBox.SelectedIndex
                    );
                    selectedProcessData.stringData1 = null;
                    selectedProcessData.stringData2 = null;
                    selectedProcessData.doubleData = double.NaN;
                    selectedProcessData.boolData = false;
                    object getPageContent = MainFrame.Content;
                    if (getPageContent is ProcessSettingPages.StartSettingPage startPage)
                    {
                        selectedProcessData.stringData1 = startPage.path;
                    }
                    else if (getPageContent is ProcessSettingPages.AudioSettingPage audioPage)
                    {
                        selectedProcessData.stringData1 = audioPage.url;
                        selectedProcessData.doubleData = audioPage.waitTime;
                    }
                    else if (getPageContent is ProcessSettingPages.ReadSettingPage readPage)
                    {
                        selectedProcessData.stringData1 = readPage.text;
                        selectedProcessData.doubleData = readPage.time;
                        selectedProcessData.boolData = readPage.read ?? false;
                    }
                    else if (getPageContent is ProcessSettingPages.CmdSettingPage cmdPage)
                    {
                        selectedProcessData.stringData1 = cmdPage.cmd;
                        selectedProcessData.boolData = cmdPage.visibility ?? false;
                    }
                    else if (getPageContent is ProcessSettingPages.WaitTimeSettingPage waitPage)
                    {
                        selectedProcessData.doubleData = waitPage.waitTime;
                    }
                    else if(getPageContent is ProcessSettingPages.ShutDownSettingPage shutDownSettingPage)
                    {
                        selectedProcessData.doubleData = (int)shutDownSettingPage.shutDownWay;
                    }
                        selectedProcessGroup.processDatas[ProcessesListView.SelectedIndex] =
                            selectedProcessData;
                    GlobalVariables.json.automaticProcess.processGroups[findGroup] =
                        selectedProcessGroup;
                }
                GlobalVariables.SaveJson();
                RefreshProcessKinds(selectedProcessGroup, false, ProcessesListView.SelectedIndex);
            }
        }

        private void RemindTimeNumberBox_ValueChanged(
            object sender,
            Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args
        )
        {
            if (canChange)
            {
                if (!(RemindTimeNumberBox.Value > 0))
                {
                    RemindTimeNumberBox.Value = 5;
                }
            }
        }

        private ProcessState IndexToProcessState(int index)
        {
            switch (index)
            {
                case 0:
                    return ProcessState.start;
                case 1:
                    return ProcessState.audio;
                case 2:
                    return ProcessState.read;
                case 3:
                    return ProcessState.cmd;
                case 4:
                    return ProcessState.wait;
                case 5:
                    return ProcessState.clear;
                case 6:
                    return ProcessState.shutDown;
                default:
                    throw new NotImplementedException("找不到属性");
            }
        }

        private void RemindTimeNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (canChange)
            {
                int get = RemindTimeNumberBox.Text.ToInt32(-1);
                if (get != -1)
                {
                    RemindTimeNumberBox.Value = get;
                }
                else
                {
                    RemindTimeNumberBox.Text = null;
                    RemindTimeNumberBox.Value = 5;
                }
            }
        }

        private void ProcessesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canChange && ProcessesListView.SelectedItem != null)
            {
                string selectedItem = ProcessesListView.SelectedItem.ToString();
                if (selectedItem == "新建流程...")
                {
                    if(ProcessKinds.Count>1)
                    {
                        string lastSelectedItem = ProcessKinds[ProcessKinds.Count - 2].ToString();
                        if (lastSelectedItem == "立即关机" || lastSelectedItem == "一般关机" || lastSelectedItem == "强制关机")
                        {
                            MessageBoxFunction.ShowMessageBoxWarning("自动关机流程后不能加入新流程");
                            ProcessesListView.UnselectAll();
                            return;
                        }
                    }           
                    ProcessData processData = new ProcessData()
                    {
                        state = ProcessState.wait,
                        doubleData = 5,
                    };
                    selectedProcessGroup.processDatas.Add(processData);
                    GlobalVariables.SaveJson();
                    RefreshProcessKinds(selectedProcessGroup, true);
                }
                else
                {
                    selectedProcessData = selectedProcessGroup.processDatas[
                        ProcessesListView.SelectedIndex
                    ];
                    ProcessKindComboBox.IsEnabled = true;
                    int lastSelectedIndex = ProcessesListView.SelectedIndex;
                    isUserInteraction = false;
                    switch (selectedProcessData.state)
                    {
                        case ProcessState.start:
                            ProcessKindComboBox.SelectedIndex = 0;
                            break;
                        case ProcessState.audio:
                            ProcessKindComboBox.SelectedIndex = 1;
                            break;
                        case ProcessState.read:
                            ProcessKindComboBox.SelectedIndex = 2;
                            break;
                        case ProcessState.cmd:
                            ProcessKindComboBox.SelectedIndex = 3;
                            break;
                        case ProcessState.wait:
                            ProcessKindComboBox.SelectedIndex = 4;
                            break;
                        case ProcessState.clear: 
                            ProcessKindComboBox.SelectedIndex = 5;
                            break;
                        case ProcessState.shutDown:
                            ProcessKindComboBox.SelectedIndex = 6;
                            break;
                        default:
                            break;
                    }

                    //防止因为SelectedIndex没有变化而不触发SelectionChanged事件
                    if (lastSelectedIndex == ProcessesListView.SelectedIndex)
                    {
                        ProcessKindComboBox_SelectionChanged(sender, e);
                    }
                    isUserInteraction = true;
                    MainFrame.IsEnabled = true;
                }
            }
        }

        private void ProcessKindComboBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e
        )
        {
            if (isUserInteraction)
            {
                ProcessState processState = selectedProcessData.state;
                selectedProcessData = new ProcessData() { state = processState, doubleData = 5 };
            }
            MainFrame.Content = null;
            Page page = null;
            switch (ProcessKindComboBox.SelectedIndex)
            {
                case 0:
                    page = new ProcessSettingPages.StartSettingPage(selectedProcessData);
                    break;
                case 1:
                    page = new ProcessSettingPages.AudioSettingPage(selectedProcessData);
                    break;
                case 2:
                    page = new ProcessSettingPages.ReadSettingPage(selectedProcessData);
                    break;
                case 3:
                    page = new ProcessSettingPages.CmdSettingPage(selectedProcessData);
                    break;
                case 4:
                    page = new ProcessSettingPages.WaitTimeSettingPage(selectedProcessData);
                    break;
                case 5:
                    page = null;
                    break;
                case 6:
                    page = new ProcessSettingPages.ShutDownSettingPage(selectedProcessData);
                    break;
                default:
                    page = null;
                    break;
            }
            if (page != null)
            {
                MainFrame.Navigate(page);
            }
        }

        private void ProcessesListViewContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (
                ProcessesListView.SelectedItem == null
                || ProcessesListView.SelectedItem.ToString() == "新建流程..."
            )
            {
                ProcessesListViewContextMenu.Visibility = Visibility.Collapsed;
            }
            else if(ProcessesListView.SelectedItem.ToString()=="立即关机"|| ProcessesListView.SelectedItem.ToString() == "一般关机"|| ProcessesListView.SelectedItem.ToString() == "强制关机")
            {
                ProcessesListViewContextMenu.Visibility = Visibility.Visible;
                UpMove.IsEnabled = false;
                DownMove.IsEnabled = false;
            }
            else
            {
                ProcessesListViewContextMenu.Visibility = Visibility.Visible;
                if (ProcessesListView.SelectedIndex == 0)
                {
                    UpMove.IsEnabled = false;
                }
                else
                {
                    UpMove.IsEnabled = true;
                }
                if (ProcessesListView.SelectedIndex == ProcessesListView.Items.Count - 2)
                {
                    DownMove.IsEnabled = false;
                }
                else
                {
                    DownMove.IsEnabled = true;
                }
            }
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (
                ProcessesListView.SelectedItem == null
                || ProcessesListView.SelectedItem.ToString() == "新建流程组..."
            )
                return;

            string itemToDelete = ProcessesListView.SelectedItem.ToString();

            var confirmDialog = new Wpf.Ui.Controls.ContentDialog()
            {
                Title = "确认删除",
                Content = $"确定要删除流程 '{itemToDelete}' 吗？此操作不可恢复。（时间表与快捷键内的流程需要保存后才会变化）",
                PrimaryButtonText = "删除",
                CloseButtonText = "取消",
                DialogHost = Host,
            };

            Wpf.Ui.Controls.ContentDialogResult result = await confirmDialog.ShowAsync();
            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary)
            {
                selectedProcessGroup.processDatas.RemoveAt(ProcessesListView.SelectedIndex);
                int findGroup = GlobalVariables.json.automaticProcess.processGroups.IndexOf(
                    selectedProcessGroup
                );
                if (findGroup != -1)
                {
                    GlobalVariables.json.automaticProcess.processGroups[findGroup] =
                        selectedProcessGroup;
                    GlobalVariables.SaveJson();
                }
                MainFrame.Content = null;
                ProcessKindComboBox.IsEnabled = false;
                ProcessKinds.RemoveAt(ProcessesListView.SelectedIndex);
                ProcessesListView.UnselectAll();
            }
        }

        private void UpMove_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProcessData != null)
            {
                int findGroup = GlobalVariables.json.automaticProcess.processGroups.IndexOf(
                    selectedProcessGroup
                );
                if (findGroup != -1)
                {
                    ProcessData lastProcessData = GlobalVariables
                        .json
                        .automaticProcess
                        .processGroups[findGroup]
                        .processDatas[ProcessesListView.SelectedIndex - 1];
                    string lastProcessDataName = ProcessKinds[ProcessesListView.SelectedIndex - 1];
                    int lastIndex = ProcessesListView.SelectedIndex;
                    GlobalVariables.json.automaticProcess.processGroups[findGroup].processDatas[
                        ProcessesListView.SelectedIndex - 1
                    ] = selectedProcessData;
                    ProcessKinds[ProcessesListView.SelectedIndex - 1] =
                        ProcessesListView.SelectedItem.ToString();
                    GlobalVariables.json.automaticProcess.processGroups[findGroup].processDatas[
                        ProcessesListView.SelectedIndex
                    ] = lastProcessData;
                    ProcessKinds[ProcessesListView.SelectedIndex] = lastProcessDataName;
                    ProcessesListView.SelectedIndex = lastIndex - 1;
                    GlobalVariables.SaveJson();
                }
            }
        }

        private void DownMove_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProcessData != null)
            {
                int findGroup = GlobalVariables.json.automaticProcess.processGroups.IndexOf(
                    selectedProcessGroup
                );
                if (findGroup != -1)
                {
                    ProcessData lastProcessData = GlobalVariables
                        .json
                        .automaticProcess
                        .processGroups[findGroup]
                        .processDatas[ProcessesListView.SelectedIndex + 1];
                    string lastProcessDataName = ProcessKinds[ProcessesListView.SelectedIndex + 1];
                    int lastIndex = ProcessesListView.SelectedIndex;
                    GlobalVariables.json.automaticProcess.processGroups[findGroup].processDatas[
                        ProcessesListView.SelectedIndex + 1
                    ] = selectedProcessData;
                    ProcessKinds[ProcessesListView.SelectedIndex + 1] =
                        ProcessesListView.SelectedItem.ToString();
                    GlobalVariables.json.automaticProcess.processGroups[findGroup].processDatas[
                        ProcessesListView.SelectedIndex
                    ] = lastProcessData;
                    ProcessKinds[ProcessesListView.SelectedIndex] = lastProcessDataName;
                    ProcessesListView.SelectedIndex = lastIndex + 1;
                    GlobalVariables.SaveJson();
                }
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            SaveButton_Click(sender, e);
            Application.Current.Dispatcher.Invoke(() =>
            {
                BrowseProcesses(selectedProcessGroup);
            });
        }

        private void BrowseProcesses(ProcessGroup processGroup)
        {
            ProcessesRunningWindow processesRunningWindow = new ProcessesRunningWindow(
                processGroup
            );
            if(processGroup.show)
                processesRunningWindow.Show();
        }
    }
}
