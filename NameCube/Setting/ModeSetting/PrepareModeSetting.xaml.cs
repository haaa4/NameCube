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
    /// PrepareModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class PrepareModeSetting : Page
    {
        private static readonly ILogger _logger = Log.ForContext<PrepareModeSetting>();

        public PrepareModeSetting()
        {
            InitializeComponent();
            _logger.Debug("预备模式设置页面初始化开始");

            CanChange = false;
            LockedCheck.IsChecked = GlobalVariables.json.PrepareModeSetting.Locked;
            Speed.Value = GlobalVariables.json.PrepareModeSetting.Speed - 10;
            CanChange = true;

            _logger.Information("预备模式设置加载完成，锁定状态: {Locked}, 速度: {Speed}",
                LockedCheck.IsChecked,
                Speed.Value);
        }

        bool CanChange;

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.PrepareModeSetting.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
                _logger.Information("预备模式锁定状态修改为: {Locked}", LockedCheck.IsChecked.Value);
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.PrepareModeSetting.Speed = (int)Speed.Value + 10;
                GlobalVariables.SaveJson();
                _logger.Debug("预备模式速度修改为: {Speed}", (int)Speed.Value + 10);
            }
        }
    }
}