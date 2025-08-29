using Masuit.Tools.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using NameCube.ToolBox.AutomaticProcessPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NameCube
{
    //傻逼名字
    internal class StartToDoSomething
    {
        public async static void GetUpdata()
        {
           if (GlobalVariables.json.StartToDo.AutoUpdata)
            {
                string GetVersion = "";
                try
                {
                    GetVersion = await GithubData.GetLatestReleaseVersionAsync("haaa4", "NameCube");
                    GlobalVariables.json.AllSettings.UpdataTime = DateTime.Now.ToString("s");
                    if (GetVersion != GlobalVariables.Version)
                    {
                        new ToastContentBuilder()
                                .AddArgument("action", "viewConversation")
                                .AddArgument("conversationId", 9813)
                                .AddText("点鸣魔方")
                                .AddText("检测到最新版本:" + GetVersion + "。请前往设置->更新处查看")
                                .Show();
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(ex);
                }

            }
            
        }
        public static void RunAutomaticProcesses()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AutoToDo.StartRunAutomaticProcesses();
            });
        }
    }
}
