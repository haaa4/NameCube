using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Serilog;
using NameCube.GlobalVariables.DataClass;

namespace NameCube.ToolBox.AutomaticProcessPages
{
    internal class AutoToDo
    {
        public async static void StartRunAutomaticProcesses()
        {
            Log.Information("开始自动流程调度任务");
            int loopCount = 0;

            try
            {
                while (true)
                {
                    await Task.Delay(1000);
                    loopCount++;

                    // 每60次循环记录一次信息（每分钟）
                    if (loopCount % 60 == 0)
                    {
                        Log.Debug("自动流程调度器运行中，已循环 {LoopCount} 次", loopCount);
                    }

                    int hour = DateTime.Now.Hour, minute = DateTime.Now.Minute, second = DateTime.Now.Second;
                    int index = TimeToIndex(hour, minute, second);

                    if (GlobalVariablesData.config.AutomaticProcess.processesSchedule.ContainsKey(index)
                        && GlobalVariablesData.config.AutomaticProcess.processesSchedule[index].Count > 0)
                    {
                        Log.Information("检测到当前时间 {Hour:00}:{Minute:00}:{Second:00} 有 {Count} 个流程组需要执行",
                            hour, minute, second, GlobalVariablesData.config.AutomaticProcess.processesSchedule[index].Count);

                        for (int i = 0; i < GlobalVariablesData.config.AutomaticProcess.processesSchedule[index].Count; i++)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                RunProcessesGroup(GlobalVariablesData.config.AutomaticProcess.processesSchedule[index][i]);
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "自动流程调度任务发生异常，已停止运行");
            }
        }

        private static int TimeToIndex(int hour, int minute, int second)
        {
            return hour * 3600 + minute * 60 + second;
        }

        private static void RunProcessesGroup(ProcessGroup processGroup)
        {
            try
            {
                Log.Information("执行流程组: {ProcessGroupName}, UID: {Uid}, 包含 {ProcessCount} 个流程",
                    processGroup?.name, processGroup?.uid, processGroup?.processDatas?.Count ?? 0);

                ProcessesRunningWindow processesRunningWindow = new ProcessesRunningWindow(processGroup);
                Log.Debug("已创建流程执行窗口");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "执行流程组时发生错误: {ProcessGroupName}", processGroup?.name);
            }
        }
    }
}