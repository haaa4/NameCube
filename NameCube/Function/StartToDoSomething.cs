using Masuit.Tools.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using NameCube.Function;
using NameCube.ToolBox.AutomaticProcessPages;
using Serilog;  // 添加Serilog命名空间
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

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
                if (GlobalVariablesData.config.StartToDo.AutoUpdata)
                {
                    Log.Debug("自动更新已启用");
                    string GetVersion = "";
                    try
                    {
                            if (GlobalVariablesData.config.AllSettings.token == "" || GlobalVariablesData.config.AllSettings.token == null)
                            {
                                Log.Debug("使用匿名方式检查更新");
                                if (GlobalVariablesData.config.AllSettings.DownloadWay == 0)
                                {
                                    GetVersion = await GithubData.GetLatestReleaseVersionAsync("haaa4", "NameCube");
                                }
                                else
                                {
                                    GetVersion = await GiteeData.GerVersion();
                                }
                            }
                            else
                            {
                                Log.Debug("使用Token方式检查更新");
                                if (GlobalVariablesData.config.AllSettings.DownloadWay == 0)
                                {
                                    GetVersion = await GithubData.GetLatestReleaseVersionAsync("haaa4", "NameCube", GlobalVariablesData.config.AllSettings.token);
                                }
                                else
                                {
                                    GetVersion = await GiteeData.GerVersion();
                                }

                                GlobalVariablesData.config.AllSettings.UpdataTime = DateTime.Now.ToString("f");

                            }
                            Log.Information("获取到最新版本: {LatestVersion}，检查时间: {CheckTime}",
                            GetVersion, DateTime.Now.ToString("s"));

                        if (ExtractVersionCode(GetVersion) > GlobalVariablesData.VERSIONCODE)
                        {
                            Log.Information("发现新版本: {LatestVersion}，当前版本: {CurrentVersion}，显示通知",
                                GetVersion, GlobalVariablesData.VERSION);
                            new ToastContentBuilder()
                                    .AddArgument("action", "viewConversation")
                                    .AddArgument("conversationId", 9813)
                                    .AddText("点鸣魔方")
                                    .AddText("检测到最新版本:" + GetVersion + "。请前往设置->更新处查看")
                                    .Show();
                            GlobalVariablesData.config.AllSettings.newVersion = GetVersion;
                        }
                        else
                        {
                            Log.Debug("当前已是最新版本: {CurrentVersion}", GlobalVariablesData.VERSION);
                        }
                        GlobalVariablesData.config.AllSettings.UpdataTime = DateTime.Now.ToString("f");
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
        public static int ExtractVersionCode(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("输入字符串不能为空。");

            // 正则表达式解释：
            // \( 匹配左括号
            // # 匹配井号
            // [^#]* 匹配任意非#字符（如p或r）
            // (\d+) 捕获一组数字
            // # 匹配井号
            // \) 匹配右括号
            string pattern = @"\(#[^#]*(\d+)#\)";
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }

            throw new ArgumentException("输入字符串中未找到有效的版本代码格式。");
        }
    }
}