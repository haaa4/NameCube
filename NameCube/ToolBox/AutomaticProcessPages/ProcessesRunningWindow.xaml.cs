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

namespace NameCube.ToolBox.AutomaticProcessPages
{
    /// <summary>
    /// ProcessesRunningWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProcessesRunningWindow : FluentWindow
    {
        int allProcessesCount,index=-1;
        ProcessGroup getProcessGroup;
        public ProcessesRunningWindow(ProcessGroup processGroup)
        {
            InitializeComponent();
            Title = processGroup.name;
            MainTitle.Title = processGroup.name;
            MainTitle.ShowClose=processGroup.canCancle;
            allProcessesCount = processGroup.processDatas.Count;
            getProcessGroup = processGroup;
            LoadPage();
        }

        public void LoadPage()
        {
            if(index==-1)
            {
                ProcessPages.ReadyPage page = new ProcessPages.ReadyPage(getProcessGroup.name, getProcessGroup.remindTime, getProcessGroup.remindText, getProcessGroup.canCancle);
                page.EndThePageAction += CallNextPage;
                MainFrame.Navigate(page);
                
                index++;
            }
            else if(index < allProcessesCount)
            {
                switch (getProcessGroup.processDatas[index].state)
                {
                    case ProcessState.start:
                        ProcessPages.StartFiles page = new ProcessPages.StartFiles(getProcessGroup.processDatas[index].stringData1);
                        page.EndThePageAction += CallNextPage;
                        MainFrame.Navigate(page);
                        break;
                    case ProcessState.audio:
                        ProcessPages.AudioPage page2 = new ProcessPages.AudioPage(getProcessGroup.processDatas[index].stringData1, (int)getProcessGroup.processDatas[index].doubleData);
                        page2.EndThePageAction += CallNextPage;
                        MainFrame.Navigate(page2);
                        break;
                    case ProcessState.read:
                        ProcessPages.ReadPage page3 = new ProcessPages.ReadPage(getProcessGroup.processDatas[index].stringData1, (int)getProcessGroup.processDatas[index].doubleData, getProcessGroup.processDatas[index].boolData);
                        page3.EndThePageAction += CallNextPage;
                        MainFrame.Navigate(page3);
                        break;
                    case ProcessState.cmd:
                        ProcessPages.CmdPage page4 = new ProcessPages.CmdPage(getProcessGroup.processDatas[index].stringData1, getProcessGroup.processDatas[index].boolData);
                        page4.EndThePageAction += CallNextPage;
                        MainFrame.Navigate(page4);
                        break;
                    case ProcessState.wait:
                        ProcessPages.WaitPage page5 = new ProcessPages.WaitPage((int)getProcessGroup.processDatas[index].doubleData);
                        page5.EndThePageAction += CallNextPage;
                        MainFrame.Navigate(page5);
                        break;
                    default:
                        this.Close();
                        break;
                }
                
                index++;
            }
            else
            {
                this.Close();
            }
        }

        private void CallNextPage(int get)
        {
            if(get==0)
            {
                LoadPage();
            }
            else if(get==1)
            {
                this.Close();
            }
        }

    }
}
