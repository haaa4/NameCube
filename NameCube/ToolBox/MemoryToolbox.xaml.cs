using Masuit.Tools.Win32;
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
using Wpf.Ui.Controls;
using Serilog;

namespace NameCube.ToolBox
{
    /// <summary>
    /// MemoryToolbox.xaml 的交互逻辑
    /// </summary>
    public partial class MemoryToolbox : Page
    {
        private static readonly ILogger _logger = Log.ForContext<MemoryToolbox>();

        public MemoryToolbox()
        {
            InitializeComponent();
            _logger.Debug("内存工具箱页面初始化");

            MemoryCheck.IsChecked = GlobalVariablesData.config.StartToDo.AlwaysCleanMemory;
            _logger.Information("内存清理设置加载完成，总是清理内存: {AlwaysCleanMemory}", MemoryCheck.IsChecked);
        }

        private void MemoryCheck_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("内存清理设置修改为: {AlwaysCleanMemory}", MemoryCheck.IsChecked.Value);

            GlobalVariablesData.config.StartToDo.AlwaysCleanMemory = MemoryCheck.IsChecked.Value;
            GlobalVariablesData.SaveConfig();

            var snackbar = new Snackbar(SnackbarPresenterHost)
            {
                Content = "设置已保存,请重启软件", // 设置提示内容
                Title = "提示",
                Appearance = ControlAppearance.Info,
                Timeout = TimeSpan.FromSeconds(10) // 显示时长
            };
            snackbar.Show();

            _logger.Information("内存清理设置已保存");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("开始手动内存清理");

            try
            {
                Masuit.Tools.Win32.Windows.ClearMemory();
                var snackbar = new Snackbar(SnackbarPresenterHost2)
                {
                    Content = "内存清理完成", // 设置提示内容
                    Title = "提示",
                    Appearance = ControlAppearance.Success,
                    Timeout = TimeSpan.FromSeconds(3) // 显示时长
                };
                snackbar.Show();

                _logger.Information("内存清理完成");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "内存清理过程中发生异常");
            }
        }
    }
}