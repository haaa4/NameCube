using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
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