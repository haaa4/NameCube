using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.ApplicationModel.Chat;
using Wpf.Ui.Controls;
using Serilog;

namespace NameCube.ToolBox.AutomaticProcessPages
{
    /// <summary>
    /// ProcessesRunningWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessesRunningWindow : FluentWindow
    {
        int allProcessesCount, index = -1;
        bool show;
        ProcessGroup getProcessGroup;
        public ProcessesRunningWindow(ProcessGroup processGroup)
        {
            Log.Information("创建流程运行窗口，流程组: {ProcessGroupName}, UID: {Uid}",
                processGroup?.name, processGroup?.uid);

            try
            {
                InitializeComponent();
                show = processGroup.show;

                Log.Debug("窗口显示设置: {Show}, 可取消: {CanCancle}, 提醒文本: {RemindText}, 提醒时间: {RemindTime}",
                    show, processGroup?.canCancle, processGroup?.remindText, processGroup?.remindTime);

                if (!show)
                {
                    this.Show();
                    this.Topmost = false;
                    this.WindowState = WindowState.Minimized;
                    Log.Debug("窗口设置为最小化显示");
                }
                else
                {
                    this.Show();
                    this.Activate();
                    this.Topmost = true;
                    Log.Debug("窗口设置为顶层显示");
                }

                Title = processGroup.name;
                MainTitle.Title = processGroup.name;
                MainTitle.ShowClose = processGroup.canCancle;
                allProcessesCount = processGroup.processDatas.Count;
                getProcessGroup = processGroup;

                Log.Information("流程组包含 {ProcessCount} 个流程", allProcessesCount);

                LoadPage();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "创建流程运行窗口时发生错误");
                throw;
            }
        }

        public void LoadPage()
        {
            try
            {
                Log.Debug("加载流程页面，当前索引: {Index}, 总流程数: {AllProcessesCount}",
                    index, allProcessesCount);

                if (index == -1)
                {
                    ProcessPages.ReadyPage page = new ProcessPages.ReadyPage(
                        getProcessGroup.name,
                        getProcessGroup.remindTime,
                        getProcessGroup.remindText,
                        getProcessGroup.canCancle,
                        false,
                        show);
                    page.EndThePageAction += CallNextPage;
                    MainFrame.Navigate(page);
                    index++;

                    Log.Information("加载就绪页面完成");
                }
                else if (index < allProcessesCount)
                {
                    ProcessData currentProcess = getProcessGroup.processDatas[index];
                    Log.Debug("加载第 {Index} 个流程，类型: {ProcessState}",
                        index + 1, currentProcess.state);

                    switch (currentProcess.state)
                    {
                        case ProcessState.start:
                            ProcessPages.StartFiles page1 = new ProcessPages.StartFiles(
                                currentProcess.stringData1, false, show);
                            page1.EndThePageAction += CallNextPage;
                            MainFrame.Navigate(page1);
                            Log.Debug("加载启动文件页面，路径: {Path}", currentProcess.stringData1);
                            break;
                        case ProcessState.audio:
                            ProcessPages.AudioPage page2 = new ProcessPages.AudioPage(
                                currentProcess.stringData1,
                                (int)currentProcess.doubleData,
                                false,
                                show);
                            page2.EndThePageAction += CallNextPage;
                            MainFrame.Navigate(page2);
                            Log.Debug("加载音频页面，URL: {Url}, 等待时间: {WaitTime}",
                                currentProcess.stringData1, currentProcess.doubleData);
                            break;
                        case ProcessState.read:
                            ProcessPages.ReadPage page3 = new ProcessPages.ReadPage(
                                currentProcess.stringData1,
                                (int)currentProcess.doubleData,
                                currentProcess.boolData,
                                false,
                                show);
                            page3.EndThePageAction += CallNextPage;
                            MainFrame.Navigate(page3);
                            Log.Debug("加载阅读页面，文本长度: {TextLength}, 时间: {Time}, 朗读: {Read}",
                                currentProcess.stringData1?.Length ?? 0,
                                currentProcess.doubleData,
                                currentProcess.boolData);
                            break;
                        case ProcessState.cmd:
                            ProcessPages.CmdPage page4 = new ProcessPages.CmdPage(
                                currentProcess.stringData1,
                                currentProcess.boolData,
                                false,
                                show);
                            page4.EndThePageAction += CallNextPage;
                            MainFrame.Navigate(page4);
                            Log.Debug("加载CMD页面，命令: {Cmd}, 可见性: {Visibility}",
                                currentProcess.stringData1, currentProcess.boolData);
                            break;
                        case ProcessState.wait:
                            ProcessPages.WaitPage page5 = new ProcessPages.WaitPage(
                                (int)currentProcess.doubleData,
                                false,
                                show);
                            page5.EndThePageAction += CallNextPage;
                            MainFrame.Navigate(page5);
                            Log.Debug("加载等待页面，等待时间: {WaitTime}", currentProcess.doubleData);
                            break;
                        case ProcessState.clear:
                            ProcessPages.ClearPage page6 = new ProcessPages.ClearPage(false, show);
                            page6.EndThePageAction += CallNextPage;
                            MainFrame.Navigate(page6);
                            Log.Debug("加载清理内存页面");
                            break;
                        case ProcessState.shutDown:
                            ProcessPages.PowerOffPage page7 = new ProcessPages.PowerOffPage(
                                (int)currentProcess.doubleData,
                                false,
                                true);
                            page7.EndThePageAction += CallNextPage;
                            MainFrame.Navigate(page7);
                            this.Show();
                            Log.Debug("加载关机页面，关机方式: {ShutDownWay}", currentProcess.doubleData);
                            break;
                        default:
                            Log.Warning("未知的流程状态: {ProcessState}", currentProcess.state);
                            this.Close();
                            break;
                    }

                    index++;
                }
                else
                {
                    Log.Information("所有流程执行完成，关闭窗口");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载流程页面时发生错误，索引: {Index}", index);
                this.Close();
            }
        }

        private void CallNextPage(int get)
        {
            Log.Debug("接收到页面完成信号，代码: {Code} (0=继续, 1=取消)", get);
            if (get == 0)
            {
                LoadPage();
            }
            else if (get == 1)
            {
                Log.Information("用户取消了流程执行");
                this.Close();
            }
        }
    }
}