using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Serilog;

namespace NameCube.ToolBox.AutomaticProcessPages
{
    /// <summary>
    /// Debug.xaml 的交互逻辑
    /// </summary>
    public partial class Debug : Page
    {
        public Debug()
        {
            Log.Information("初始化调试页面");
            InitializeComponent();
            Log.Information("调试页面初始化完成");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击退出调试模式按钮");
            GlobalVariablesData.config.AutomaticProcess.debug = false;
            GlobalVariablesData.SaveConfig();
            Log.Information("已关闭调试模式");
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击确定按钮，创建调试窗口");
            DebugWindows debugWindows = null;
            Page targetPage = null;

            Log.Debug("调试参数 - 选择类型索引: {SelectedIndex}, 数据1: {Data1}, 数值数据: {NumericData}, 数据2: {Data2}, 复选框: {CheckedData}",
                SelectedData.SelectedIndex, Data1.Text, NumericData.Value, Data2.Text, CheckedData.IsChecked);

            switch (SelectedData.SelectedIndex)
            {
                case 0:
                    targetPage = new ProcessPages.ReadyPage(Data1.Text, (int)NumericData.Value, Data2.Text, CheckedData.IsChecked.Value, true);
                    Log.Debug("创建就绪页面，标题: {Title}, 等待时间: {WaitTime}, 消息: {Message}, 可取消: {CanCancel}",
                        Data1.Text, NumericData.Value, Data2.Text, CheckedData.IsChecked.Value);
                    break;
                case 1:
                    targetPage = new ProcessPages.StartFiles(Data1.Text, true);
                    Log.Debug("创建启动文件页面，路径: {Path}", Data1.Text);
                    break;
                case 2:
                    targetPage = new ProcessPages.AudioPage(Data1.Text, (int)NumericData.Value, true);
                    Log.Debug("创建音频页面，URL: {Url}, 等待时间: {WaitTime}", Data1.Text, NumericData.Value);
                    break;
                case 3:
                    targetPage = new ProcessPages.ReadPage(Data1.Text, (int)NumericData.Value, CheckedData.IsChecked.Value, true);
                    Log.Debug("创建阅读页面，文本长度: {TextLength}, 时间: {Time}, 朗读: {Read}",
                        Data1.Text?.Length ?? 0, NumericData.Value, CheckedData.IsChecked.Value);
                    break;
                case 4:
                    targetPage = new ProcessPages.CmdPage(Data1.Text, CheckedData.IsChecked.Value, true);
                    Log.Debug("创建CMD页面，命令: {Cmd}, 可见性: {Visibility}", Data1.Text, CheckedData.IsChecked.Value);
                    break;
                case 5:
                    targetPage = new ProcessPages.WaitPage((int)NumericData.Value, true);
                    Log.Debug("创建等待页面，等待时间: {WaitTime}", NumericData.Value);
                    break;
                case 6:
                    targetPage = new ProcessPages.ClearPage(true);
                    Log.Debug("创建清理内存页面");
                    break;
                case 7:
                    targetPage = new ProcessPages.PowerOffPage((int)NumericData.Value, true);
                    Log.Debug("创建关机页面，关机方式: {ShutDownWay}", NumericData.Value);
                    break;
                default:
                    targetPage = null;
                    Log.Warning("未知的调试类型索引: {Index}", SelectedData.SelectedIndex);
                    break;
            }

            if (targetPage != null)
            {
                try
                {
                    debugWindows = new DebugWindows(targetPage);
                    debugWindows.Show();
                    Log.Information("调试窗口创建并显示成功");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "创建调试窗口时发生错误");
                }
            }
            else
            {
                Log.Warning("目标页面创建失败");
            }
        }
    }
}