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

            if (GlobalVariables.json.automaticProcess.debug)
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