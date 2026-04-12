using Serilog;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.Setting.ModeSetting
{
    /// <summary>
    /// BatchModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class BatchModeSetting : Page
    {
        private static readonly ILogger _logger = Log.ForContext<BatchModeSetting>();

        private bool CanChange;

        public BatchModeSetting()
        {
            InitializeComponent();
            _logger.Debug("批量模式设置页面初始化开始");

            CanChange = false;
            LockedCheck.IsChecked = GlobalVariablesData.config.BatchModeSettings.Locked;
            CanChange = true;

            _logger.Information("批量模式设置加载完成，锁定状态: {Locked}", LockedCheck.IsChecked);
        }

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.BatchModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                _logger.Information("批量模式锁定状态修改为: {Locked}", LockedCheck.IsChecked.Value);
            }
        }
    }
}