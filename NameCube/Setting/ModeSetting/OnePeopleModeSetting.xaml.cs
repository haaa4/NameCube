using System.Windows;
using System.Windows.Controls;
using Serilog;

namespace NameCube.Setting.ModeSetting
{
    /// <summary>
    /// OnePeopleModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class OnePeopleModeSetting : Page
    {
        private static readonly ILogger _logger = Log.ForContext<OnePeopleModeSetting>();

        bool CanChange;
        public OnePeopleModeSetting()
        {
            InitializeComponent();
            _logger.Debug("单人模式设置页面初始化开始");

            CanChange = false;
            LockedCheck.IsChecked = GlobalVariablesData.config.OnePeopleModeSettings.Locked;
            Speed.Value = GlobalVariablesData.config.OnePeopleModeSettings.Speed - 10;
            CanChange = true;

            _logger.Information("单人模式设置加载完成，锁定状态: {Locked}, 速度: {Speed}",
                LockedCheck.IsChecked,
                Speed.Value);
        }

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.OnePeopleModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                _logger.Information("单人模式锁定状态修改为: {Locked}", LockedCheck.IsChecked.Value);
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.OnePeopleModeSettings.Speed = (int)Speed.Value + 10;
                GlobalVariablesData.SaveConfig();
                _logger.Debug("单人模式速度修改为: {Speed}", (int)Speed.Value + 10);
            }
        }
    }
}