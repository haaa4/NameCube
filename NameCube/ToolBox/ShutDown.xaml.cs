using Serilog;
using System.Windows.Controls;

namespace NameCube.ToolBox
{
    /// <summary>
    /// ShutDown.xaml 的交互逻辑
    /// </summary>
    public partial class ShutDown : Page
    {
        private static readonly ILogger _logger = Log.ForContext<ShutDown>();

        public ShutDown()
        {
            InitializeComponent();
            _logger.Debug("关机工具箱页面初始化");
        }
    }
}