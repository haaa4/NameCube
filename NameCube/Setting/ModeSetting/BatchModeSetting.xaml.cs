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

namespace NameCube.Setting.ModeSetting
{
    /// <summary>
    /// BatchModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class BatchModeSetting : Page
    {
        private static readonly ILogger _logger = Log.ForContext<BatchModeSetting>();

        bool CanChange;
        public BatchModeSetting()
        {
            InitializeComponent();
            _logger.Debug("批量模式设置页面初始化开始");

            CanChange = false;
            LockedCheck.IsChecked = GlobalVariables.json.BatchModeSettings.Locked;
            CanChange = true;

            _logger.Information("批量模式设置加载完成，锁定状态: {Locked}", LockedCheck.IsChecked);
        }

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.BatchModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
                _logger.Information("批量模式锁定状态修改为: {Locked}", LockedCheck.IsChecked.Value);
            }
        }
    }
}