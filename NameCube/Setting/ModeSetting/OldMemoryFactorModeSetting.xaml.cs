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
    /// OldMemoryFactorModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class OldMemoryFactorModeSetting : Page
    {
        private static readonly ILogger _logger = Log.ForContext<OldMemoryFactorModeSetting>();

        bool CanChange;
        public OldMemoryFactorModeSetting()
        {
            InitializeComponent();
            _logger.Debug("旧记忆因子模式设置页面初始化开始");

            CanChange = false;
            LockedCheck.IsChecked = GlobalVariables.json.OldMemoryFactorModeSettings.Locked;
            Speed.Value = (double)(GlobalVariables.json.OldMemoryFactorModeSettings.Speed - 10);
            EnableCheck.IsChecked = GlobalVariables.json.OldMemoryFactorModeSettings.IsEnable;
            CanChange = true;

            _logger.Information("旧记忆因子模式设置加载完成，锁定状态: {Locked}, 速度: {Speed}, 启用状态: {Enabled}",
                LockedCheck.IsChecked,
                Speed.Value,
                EnableCheck.IsChecked);
        }

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.OldMemoryFactorModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
                _logger.Information("旧记忆因子模式锁定状态修改为: {Locked}", LockedCheck.IsChecked.Value);
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.OldMemoryFactorModeSettings.Speed = (int)Speed.Value + 10;
                GlobalVariables.SaveJson();
                _logger.Debug("旧记忆因子模式速度修改为: {Speed}", (int)Speed.Value + 10);
            }
        }

        private void EnableCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.OldMemoryFactorModeSettings.IsEnable = EnableCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
                _logger.Information("旧记忆因子模式启用状态修改为: {Enabled}", EnableCheck.IsChecked.Value);
            }
        }
    }
}