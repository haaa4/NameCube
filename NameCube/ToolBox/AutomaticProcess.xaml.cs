using Serilog;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.ToolBox
{
    /// <summary>
    /// AutomaticProcess.xaml 的交互逻辑
    /// </summary>
    public partial class AutomaticProcess : Page
    {
        private static readonly ILogger _logger = Log.ForContext<AutomaticProcess>();

        public AutomaticProcess()
        {
            InitializeComponent();
            _logger.Debug("自动处理页面初始化");

            if (GlobalVariablesData.config.AutomaticProcess.debug)
            {
                DebugItem.Visibility = Visibility.Visible;
                _logger.Warning("自动处理调试模式已启用");
            }
            else
            {
                _logger.Debug("自动处理调试模式未启用");
            }
        }
    }
}