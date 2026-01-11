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
            LockedCheck.IsChecked = GlobalVariables.json.NumberModeSettings.Locked;
            Speed.Value = GlobalVariables.json.NumberModeSettings.Speed - 10;
            CanChange = true;

            _logger.Information("数字模式设置加载完成，锁定状态: {Locked}, 速度: {Speed}",
                LockedCheck.IsChecked,
                Speed.Value);
        }

        bool CanChange;

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.NumberModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
                _logger.Information("数字模式锁定状态修改为: {Locked}", LockedCheck.IsChecked.Value);
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.NumberModeSettings.Speed = (int)Speed.Value + 10;
                GlobalVariables.SaveJson();
                _logger.Debug("数字模式速度修改为: {Speed}", (int)Speed.Value + 10);
            }
        }
    }
}