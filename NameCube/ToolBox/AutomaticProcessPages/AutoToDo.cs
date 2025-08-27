using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NameCube.ToolBox.AutomaticProcessPages
{
    internal class AutoToDo
    {
        public async static void StartRunAutomaticProcesses()
        {
            while(true)
            {
                await Task.Delay(1000);
                int hour=DateTime.Now.Hour,minute=DateTime.Now.Minute,second=DateTime.Now.Second;
                int index = TimeToIndex(hour, minute, second);
                if (GlobalVariables.json.automaticProcess.processesSchedule.ContainsKey(index) && GlobalVariables.json.automaticProcess.processesSchedule[index].Count>0)
                {
                    for(int i = 0; i < GlobalVariables.json.automaticProcess.processesSchedule[index].Count;i++)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            RunProcessesGroup(GlobalVariables.json.automaticProcess.processesSchedule[index][i]);
                        });
                    }
                }
            }
        }
        private static int TimeToIndex(int hour, int minute, int second)
        {
            return hour * 3600 + minute * 60 + second;
        }
        private static void RunProcessesGroup(ProcessGroup  processGroup)
        {
            ProcessesRunningWindow processesRunningWindow=new ProcessesRunningWindow(processGroup);
            processesRunningWindow.Show();
            processesRunningWindow.Activate();
        }
    }
}
