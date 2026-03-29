using Serilog; // 添加Serilog引用
using System.Windows.Controls;

namespace NameCube.Setting
{
    /// <summary>
    /// ModeSettings.xaml 的交互逻辑
    /// </summary>
    public partial class ModeSettings : Page
    {
        private static readonly ILogger _logger = Log.ForContext<ModeSettings>(); // 添加Serilog日志实例

        public ModeSettings()
        {
            InitializeComponent();
            _logger.Debug("ModeSettings 初始化完成");
        }
    }
}