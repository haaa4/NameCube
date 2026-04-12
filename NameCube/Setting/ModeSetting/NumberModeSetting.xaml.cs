using Serilog;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.Setting.ModeSetting
{
    /// <summary>
    /// NumberModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class NumberModeSetting : Page
    {
        private static readonly ILogger _logger = Log.ForContext<NumberModeSetting>();

        public NumberModeSetting()
        {
            InitializeComponent();
            _logger.Debug("数字模式设置页面初始化开始");

            CanChange = false;
            LockedCheck.IsChecked = GlobalVariablesData.config.NumberModeSettings.Locked;
            Speed.Value = GlobalVariablesData.config.NumberModeSettings.Speed - 10;
            CanChange = true;

            _logger.Information("数字模式设置加载完成，锁定状态: {Locked}, 速度: {Speed}",
                LockedCheck.IsChecked,
                Speed.Value);
        }

        private bool CanChange;

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.NumberModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                _logger.Information("数字模式锁定状态修改为: {Locked}", LockedCheck.IsChecked.Value);
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.NumberModeSettings.Speed = (int)Speed.Value + 10;
                GlobalVariablesData.SaveConfig();
                _logger.Debug("数字模式速度修改为: {Speed}", (int)Speed.Value + 10);
            }
        }
    }
}