using System;
using System.Collections.Generic;
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
using Serilog;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessSettingPages
{
    /// <summary>
    /// StartSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class StartSettingPage : Page
    {
        public string path;

        public StartSettingPage(ProcessData processData)
        {
            Log.Information("初始化启动文件设置页面");
            try
            {
                InitializeComponent();
                DataContext = this;
                path = processData.stringData1;
                PathTextBox.Text = path;

                Log.Debug("启动文件路径初始化: {Path}", path ?? "空");
                Log.Information("启动文件设置页面初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化启动文件设置页面时发生错误");
                throw;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击浏览文件按钮");
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "所有文件|*.*",
                Title = "选择启动文件",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string oldPath = path;
                path = openFileDialog.FileName;
                PathTextBox.Text = path;

                Log.Information("用户选择了文件: {FilePath}，原路径: {OldPath}",
                    path, oldPath ?? "空");
            }
            else
            {
                Log.Debug("用户取消了文件选择");
            }
        }

        private void PathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string oldPath = path;
            path = PathTextBox.Text;

            if (oldPath != path)
            {
                Log.Debug("启动文件路径从 {OldPath} 修改为 {NewPath}",
                    oldPath ?? "空", path ?? "空");
            }
        }

        private void PathTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            // 这个方法似乎是重复的，保留原有逻辑但添加日志
            Log.Debug("PathTextBox文本变更事件2被触发");
            PathTextBox_TextChanged(sender, e);
        }
    }
}