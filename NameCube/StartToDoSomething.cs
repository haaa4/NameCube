using Masuit.Tools.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using NameCube.ToolBox.AutomaticProcessPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Serilog;  // 添加Serilog命名空间

namespace NameCube
{
    //傻逼名字
    internal class StartToDoSomething
    {
        public async static void GetUpdata()
        {
            try
            {
                Log.Information("开始检查更新");
                if (GlobalVariables.json.StartToDo.AutoUpdata)
                {
                    Log.Debug("自动更新已启用");
                    string GetVersion = "";
                    try
                    {
                        GetVersion = await GithubData.GetLatestReleaseVersionAsync("haaa4", "NameCube");
                        GlobalVariables.json.AllSettings.UpdataTime = DateTime.Now.ToString("s");
                        Log.Information("获取到最新版本: {LatestVersion}，检查时间: {CheckTime}",
                            GetVersion, DateTime.Now.ToString("s"));

                        if (GetVersion != GlobalVariables.Version)
                        {
                            Log.Information("发现新版本: {LatestVersion}，当前版本: {CurrentVersion}，显示通知",
                                GetVersion, GlobalVariables.Version);
                            new ToastContentBuilder()
                                    .AddArgument("action", "viewConversation")
                                    .AddArgument("conversationId", 9813)
                                    .AddText("点鸣魔方")
                                    .AddText("检测到最新版本:" + GetVersion + "。请前往设置->更新处查看")
                                    .Show();
                        }
                        else
                        {
                            Log.Debug("当前已是最新版本: {CurrentVersion}", GlobalVariables.Version);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "检查更新时发生错误");
                    }
                }
                else
                {
                    Log.Debug("自动更新已禁用，跳过检查");
                }
                Log.Information("更新检查完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "执行GetUpdata时发生错误");
            }
        }

        public static void RunAutomaticProcesses()
        {
            try
            {
                Log.Information("开始运行自动进程");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AutoToDo.StartRunAutomaticProcesses();
                });
                Log.Information("自动进程启动完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "运行自动进程时发生错误");
            }
        }
    }
}