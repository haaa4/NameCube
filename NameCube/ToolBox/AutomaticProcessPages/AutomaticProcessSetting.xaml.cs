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
using Serilog;
using NameCube.GlobalVariables.DataClass;

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
            Log.Information("初始化自动流程设置页面");
            InitializeComponent();
            DataContext = this;
            if (GlobalVariablesData.config.AutomaticProcess.processGroups == null)
            {
                Log.Debug("初始化自动流程组列表为空，创建新列表");
                GlobalVariablesData.config.AutomaticProcess.processGroups = new List<ProcessGroup>();
            }
            Loaded += AutomaticProcessSetting_Loaded;
            Log.Information("自动流程设置页面初始化完成");
        }

        /// <summary>
        /// 刷新流程组列表
        /// </summary>
        /// <param name="selectLastOne">选中最后一个</param>
        private void RefreshList(bool selectLastOne = false)
        {
            Log.Debug("刷新流程组列表，选择最后一个: {SelectLastOne}", selectLastOne);
            canChange = false;
            ProcessGroupsListView.UnselectAll();
            ProcessGroups.Clear();
            foreach (
                ProcessGroup processGroup in GlobalVariablesData.config.AutomaticProcess.processGroups
            )
            {
                ProcessGroups.Add(processGroup.name);
            }
            ProcessGroups.Add("新建流程组...");
            canChange = true;
            if (selectLastOne)
            {
                ProcessGroupsListView.SelectedIndex = ProcessGroups.Count - 2;
                Log.Debug("选择了最后一个流程组，索引: {Index}", ProcessGroups.Count - 2);
            }
            Log.Information("流程组列表刷新完成，共 {Count} 个流程组", ProcessGroups.Count - 1);
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
            try
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
            catch (Exception ex)
            {
                Log.Error(ex, "获取流程名称时发生错误");
                return "获取名称失败";
            }
        }

        private void RefreshProcessKinds(
            ProcessGroup processGroup,
            bool selectedLastOne = false,
            int selected = -1
        )
        {
            Log.Debug("刷新流程列表，流程组: {ProcessGroupName}, 选择最后一个: {SelectedLastOne}, 选择索引: {Selected}",
                processGroup?.name, selectedLastOne, selected);
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
                Log.Debug("选择了最后一个流程，索引: {Index}", ProcessKinds.Count - 2);
            }
            if (selected != -1)
            {
                ProcessesListView.SelectedIndex = selected;
                Log.Debug("选择了指定索引的流程，索引: {Index}", selected);
            }
            Log.Information("流程列表刷新完成，共 {Count} 个流程", ProcessKinds.Count - 1);
        }

        private void AutomaticProcessSetting_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("自动流程设置页面加载完成");
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
                Log.Information("用户选择流程组: {SelectedItem}", selectedItem);

                if (selectedItem == "新建流程组...")
                {
                    Log.Debug("用户选择新建流程组");
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
                            PlaceholderText = "请输入流程组名称(不能以\" * \"开头)",
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
                                GlobalVariablesData.config.AutomaticProcess.processGroups.Add(newGroup);
                                GlobalVariablesData.SaveConfig();
                                Log.Information("成功创建新流程组: {GroupName}, UID: {Uid}",
                                    textBox.Text, newGroup.uid);
                            }
                            else
                            {
                                Log.Warning("流程组命名不符合规定: {GroupName}", textBox.Text);
                                SnackBarFunction.ShowSnackBarInToolBoxWindow("命名不符合规定", Wpf.Ui.Controls.ControlAppearance.Caution);
                            }
                            RefreshList(true);
                        }
                    }
                    else
                    {
                        Log.Debug("用户取消了新建流程组");
                        dialog.Hide();
                        RefreshList(true);
                    }
                }
                else
                {
                    selectedProcessGroup = GlobalVariablesData.config.AutomaticProcess.processGroups[
                        ProcessGroupsListView.SelectedIndex
                    ];
                    Log.Debug("选择流程组详情: {GroupName}, UID: {Uid}, 流程数量: {ProcessCount}",
                        selectedProcessGroup.name, selectedProcessGroup.uid,
                        selectedProcessGroup.processDatas?.Count ?? 0);

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
            Log.Information("用户请求重命名流程组");
            if (
                ProcessGroupsListView.SelectedItem == null
                || ProcessGroupsListView.SelectedItem.ToString() == "新建流程组..."
            )
                return;

            string oldName = ProcessGroupsListView.SelectedItem.ToString();
            var processGroup = GlobalVariablesData.config.AutomaticProcess.processGroups.FirstOrDefault(
                pg => pg.name == oldName
            );

            if (processGroup == null)
            {
                Log.Warning("未找到要重命名的流程组: {OldName}", oldName);
                return;
            }

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
                    string newName = textBox.Text;
                    processGroup.name = newName;
                    GlobalVariablesData.SaveConfig();
                    Log.Information("成功重命名流程组: {OldName} -> {NewName}", oldName, newName);
                    RefreshList();
                }
            }
            else
            {
                Log.Debug("用户取消了重命名操作");
            }
        }

        private async void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户请求删除流程组");
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
                    GlobalVariablesData.config.AutomaticProcess.processGroups.FirstOrDefault(pg =>
                        pg.name == itemToDelete
                    );

                if (processGroup != null)
                {
                    GlobalVariablesData.config.AutomaticProcess.processGroups.Remove(processGroup);
                    Part3.IsEnabled = false;
                    MainFrame.Content = null;
                    ProcessKinds.Clear();
                    SaveButton.IsEnabled = false;
                    BrowseButton.IsEnabled = false;
                    GlobalVariablesData.SaveConfig();
                    Log.Information("成功删除流程组: {GroupName}, UID: {Uid}",
                        itemToDelete, processGroup.uid);
                    RefreshList();
                }
            }
            else
            {
                Log.Debug("用户取消了删除操作");
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ProcessGroupsListView.MaxHeight = e.NewSize.Height / 2 - 60;
            ProcessesListView.MaxHeight = e.NewSize.Height / 2 - 60;
            Log.Debug("页面大小改变，新高度: {NewHeight}, 新宽度: {NewWidth}",
                e.NewSize.Height, e.NewSize.Width);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击保存按钮");
            try
            {
                int findGroup = GlobalVariablesData.config.AutomaticProcess.processGroups.IndexOf(
                    selectedProcessGroup
                );
                if (findGroup != -1)
                {
                    selectedProcessGroup.remindText = RemindTextTextBox.Text;
                    selectedProcessGroup.remindTime = (int)RemindTimeNumberBox.Value;
                    selectedProcessGroup.canCancle = CanCancleCheckBox.IsChecked ?? false;
                    selectedProcessGroup.show = ShowCheckBox.IsChecked ?? true;

                    Log.Debug("更新流程组配置: 提醒文本长度={RemindTextLength}, 提醒时间={RemindTime}, 可取消={CanCancle}, 显示={Show}",
                        RemindTextTextBox.Text.Length, RemindTimeNumberBox.Value,
                        selectedProcessGroup.canCancle, selectedProcessGroup.show);

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
                            Log.Debug("更新启动设置，路径: {Path}", startPage.path);
                        }
                        else if (getPageContent is ProcessSettingPages.AudioSettingPage audioPage)
                        {
                            selectedProcessData.stringData1 = audioPage.url;
                            selectedProcessData.doubleData = audioPage.waitTime;
                            Log.Debug("更新音频设置，URL: {Url}, 等待时间: {WaitTime}",
                                audioPage.url, audioPage.waitTime);
                        }
                        else if (getPageContent is ProcessSettingPages.ReadSettingPage readPage)
                        {
                            selectedProcessData.stringData1 = readPage.text;
                            selectedProcessData.doubleData = readPage.time;
                            selectedProcessData.boolData = readPage.read ?? false;
                            Log.Debug("更新阅读设置，文本长度: {TextLength}, 时间: {Time}, 阅读: {Read}",
                                readPage.text?.Length ?? 0, readPage.time, readPage.read ?? false);
                        }
                        else if (getPageContent is ProcessSettingPages.CmdSettingPage cmdPage)
                        {
                            selectedProcessData.stringData1 = cmdPage.cmd;
                            selectedProcessData.boolData = cmdPage.visibility ?? false;
                            Log.Debug("更新CMD设置，命令: {Cmd}, 可见性: {Visibility}",
                                cmdPage.cmd, cmdPage.visibility ?? false);
                        }
                        else if (getPageContent is ProcessSettingPages.WaitTimeSettingPage waitPage)
                        {
                            selectedProcessData.doubleData = waitPage.waitTime;
                            Log.Debug("更新等待时间设置，等待时间: {WaitTime}", waitPage.waitTime);
                        }
                        else if (getPageContent is ProcessSettingPages.ShutDownSettingPage shutDownSettingPage)
                        {
                            selectedProcessData.doubleData = (int)shutDownSettingPage.shutDownWay;
                            Log.Debug("更新关机设置，关机方式: {ShutDownWay}",
                                (int)shutDownSettingPage.shutDownWay);
                        }
                        selectedProcessGroup.processDatas[ProcessesListView.SelectedIndex] =
                            selectedProcessData;
                        GlobalVariablesData.config.AutomaticProcess.processGroups[findGroup] =
                            selectedProcessGroup;
                    }
                    GlobalVariablesData.SaveConfig();
                    Log.Information("成功保存流程组: {GroupName}", selectedProcessGroup.name);
                    RefreshProcessKinds(selectedProcessGroup, false, ProcessesListView.SelectedIndex);
                }
                else
                {
                    Log.Warning("未找到要保存的流程组");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保存流程组时发生错误");
                throw;
            }
        }

        private void RemindTimeNumberBox_ValueChanged(
            object sender,
            Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args
        )
        {
            if (canChange)
            {
                if (!(RemindTimeNumberBox.Value >= 0))
                {
                    Log.Warning("提醒时间输入无效，重置为默认值5");
                    RemindTimeNumberBox.Value = 5;
                }
            }
        }

        private ProcessState IndexToProcessState(int index)
        {
            try
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
            catch (Exception ex)
            {
                Log.Error(ex, "转换索引到流程状态时发生错误，索引: {Index}", index);
                throw;
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
                    Log.Debug("提醒时间输入框失去焦点，解析值为: {Value}", get);
                }
                else
                {
                    Log.Warning("提醒时间输入无效，重置为默认值5");
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
                Log.Information("用户选择流程: {SelectedItem}", selectedItem);

                if (selectedItem == "新建流程...")
                {
                    if (ProcessKinds.Count > 1)
                    {
                        string lastSelectedItem = ProcessKinds[ProcessKinds.Count - 2].ToString();
                        if (lastSelectedItem == "立即关机" || lastSelectedItem == "一般关机" || lastSelectedItem == "强制关机")
                        {
                            Log.Warning("在关机流程后尝试添加新流程被阻止");
                            SnackBarFunction.ShowSnackBarInToolBoxWindow("自动关机流程后不能加入新流程", Wpf.Ui.Controls.ControlAppearance.Caution);
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
                    GlobalVariablesData.SaveConfig();
                    Log.Information("成功新建流程，状态: {State}, 等待时间: {WaitTime}",
                        ProcessState.wait, 5);
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

                    Log.Debug("选择的流程详情: 状态={State}, 字符串数据1长度={StringData1Length}, 双精度数据={DoubleData}",
                        selectedProcessData.state, selectedProcessData.stringData1?.Length ?? 0,
                        selectedProcessData.doubleData);

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
                            Log.Warning("未知的流程状态: {ProcessState}", selectedProcessData.state);
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
                Log.Debug("用户选择流程类型，索引: {SelectedIndex}", ProcessKindComboBox.SelectedIndex);
                ProcessState processState = selectedProcessData.state;
                selectedProcessData = new ProcessData() { state = processState, doubleData = 5 };
            }
            MainFrame.Content = null;
            Page page = null;
            switch (ProcessKindComboBox.SelectedIndex)
            {
                case 0:
                    page = new ProcessSettingPages.StartSettingPage(selectedProcessData);
                    Log.Debug("创建启动设置页面");
                    break;
                case 1:
                    page = new ProcessSettingPages.AudioSettingPage(selectedProcessData);
                    Log.Debug("创建音频设置页面");
                    break;
                case 2:
                    page = new ProcessSettingPages.ReadSettingPage(selectedProcessData);
                    Log.Debug("创建阅读设置页面");
                    break;
                case 3:
                    page = new ProcessSettingPages.CmdSettingPage(selectedProcessData);
                    Log.Debug("创建CMD设置页面");
                    break;
                case 4:
                    page = new ProcessSettingPages.WaitTimeSettingPage(selectedProcessData);
                    Log.Debug("创建等待时间设置页面");
                    break;
                case 5:
                    page = null;
                    Log.Debug("清理内存页面不需要设置");
                    break;
                case 6:
                    page = new ProcessSettingPages.ShutDownSettingPage(selectedProcessData);
                    Log.Debug("创建关机设置页面");
                    break;
                default:
                    page = null;
                    Log.Warning("未知的流程类型索引: {Index}", ProcessKindComboBox.SelectedIndex);
                    break;
            }
            if (page != null)
            {
                MainFrame.Navigate(page);
                Log.Debug("导航到设置页面完成");
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
            else if (ProcessesListView.SelectedItem.ToString() == "立即关机" || ProcessesListView.SelectedItem.ToString() == "一般关机" || ProcessesListView.SelectedItem.ToString() == "强制关机")
            {
                ProcessesListViewContextMenu.Visibility = Visibility.Visible;
                UpMove.IsEnabled = false;
                DownMove.IsEnabled = false;
                Log.Debug("关机流程的上下文菜单，禁用移动功能");
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
                Log.Debug("流程上下文菜单，向上移动={UpMoveEnabled}, 向下移动={DownMoveEnabled}",
                    UpMove.IsEnabled, DownMove.IsEnabled);
            }
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户请求删除流程");
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
                int findGroup = GlobalVariablesData.config.AutomaticProcess.processGroups.IndexOf(
                    selectedProcessGroup
                );
                if (findGroup != -1)
                {
                    GlobalVariablesData.config.AutomaticProcess.processGroups[findGroup] =
                        selectedProcessGroup;
                    GlobalVariablesData.SaveConfig();
                    Log.Information("成功删除流程: {ProcessName}", itemToDelete);
                }
                MainFrame.Content = null;
                ProcessKindComboBox.IsEnabled = false;
                ProcessKinds.RemoveAt(ProcessesListView.SelectedIndex);
                ProcessesListView.UnselectAll();
            }
            else
            {
                Log.Debug("用户取消了删除流程操作");
            }
        }

        private void UpMove_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户请求向上移动流程");
            if (selectedProcessData != null)
            {
                int findGroup = GlobalVariablesData.config.AutomaticProcess.processGroups.IndexOf(
                    selectedProcessGroup
                );
                if (findGroup != -1)
                {
                    ProcessData lastProcessData = GlobalVariablesData
                        .config
                        .AutomaticProcess
                        .processGroups[findGroup]
                        .processDatas[ProcessesListView.SelectedIndex - 1];
                    string lastProcessDataName = ProcessKinds[ProcessesListView.SelectedIndex - 1];
                    int lastIndex = ProcessesListView.SelectedIndex;
                    GlobalVariablesData.config.AutomaticProcess.processGroups[findGroup].processDatas[
                        ProcessesListView.SelectedIndex - 1
                    ] = selectedProcessData;
                    ProcessKinds[ProcessesListView.SelectedIndex - 1] =
                        ProcessesListView.SelectedItem.ToString();
                    GlobalVariablesData.config.AutomaticProcess.processGroups[findGroup].processDatas[
                        ProcessesListView.SelectedIndex
                    ] = lastProcessData;
                    ProcessKinds[ProcessesListView.SelectedIndex] = lastProcessDataName;
                    ProcessesListView.SelectedIndex = lastIndex - 1;
                    GlobalVariablesData.SaveConfig();
                    Log.Information("成功向上移动流程，从索引 {FromIndex} 移动到 {ToIndex}",
                        lastIndex, lastIndex - 1);
                }
            }
        }

        private void DownMove_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户请求向下移动流程");
            if (selectedProcessData != null)
            {
                int findGroup = GlobalVariablesData.config.AutomaticProcess.processGroups.IndexOf(
                    selectedProcessGroup
                );
                if (findGroup != -1)
                {
                    ProcessData lastProcessData = GlobalVariablesData
                        .config
                        .AutomaticProcess
                        .processGroups[findGroup]
                        .processDatas[ProcessesListView.SelectedIndex + 1];
                    string lastProcessDataName = ProcessKinds[ProcessesListView.SelectedIndex + 1];
                    int lastIndex = ProcessesListView.SelectedIndex;
                    GlobalVariablesData.config.AutomaticProcess.processGroups[findGroup].processDatas[
                        ProcessesListView.SelectedIndex + 1
                    ] = selectedProcessData;
                    ProcessKinds[ProcessesListView.SelectedIndex + 1] =
                        ProcessesListView.SelectedItem.ToString();
                    GlobalVariablesData.config.AutomaticProcess.processGroups[findGroup].processDatas[
                        ProcessesListView.SelectedIndex
                    ] = lastProcessData;
                    ProcessKinds[ProcessesListView.SelectedIndex] = lastProcessDataName;
                    ProcessesListView.SelectedIndex = lastIndex + 1;
                    GlobalVariablesData.SaveConfig();
                    Log.Information("成功向下移动流程，从索引 {FromIndex} 移动到 {ToIndex}",
                        lastIndex, lastIndex + 1);
                }
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击浏览按钮，预览流程组");
            SaveButton_Click(sender, e);
            Application.Current.Dispatcher.Invoke(() =>
            {
                BrowseProcesses(selectedProcessGroup);
            });
        }

        private void BrowseProcesses(ProcessGroup processGroup)
        {
            Log.Information("开始预览流程组: {ProcessGroupName}", processGroup?.name);
            ProcessesRunningWindow processesRunningWindow = new ProcessesRunningWindow(
                processGroup
            );
        }

    }
}