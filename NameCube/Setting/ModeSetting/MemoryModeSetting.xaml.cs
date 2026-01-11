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
    /// MemoryModeSetting.xaml 的交互逻辑
    /// </summary>
    public partial class MemoryModeSetting : Page
    {
        private static readonly ILogger _logger = Log.ForContext<MemoryModeSetting>();

        public MemoryModeSetting()
        {
            InitializeComponent();
            _logger.Debug("记忆模式设置页面初始化开始");

            CanChange = false;
            LockedCheck.IsChecked = GlobalVariables.json.MemoryModeSettings.Locked;
            Speed.Value = GlobalVariables.json.MemoryModeSettings.Speed - 10;
            AddSwitch.IsChecked = GlobalVariables.json.MemoryModeSettings.AutoAddFile;
            CanChange = true;

            _logger.Information("记忆模式设置加载完成，锁定状态: {Locked}, 速度: {Speed}, 自动添加文件: {AutoAddFile}",
                LockedCheck.IsChecked,
                Speed.Value,
                AddSwitch.IsChecked);
        }

        bool CanChange;

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.MemoryModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
                _logger.Information("记忆模式锁定状态修改为: {Locked}", LockedCheck.IsChecked.Value);
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.MemoryModeSettings.Speed = (int)Speed.Value + 10;
                GlobalVariables.SaveJson();
                _logger.Debug("记忆模式速度修改为: {Speed}", (int)Speed.Value + 10);
            }
        }

        private void AddSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.MemoryModeSettings.AutoAddFile = AddSwitch.IsChecked.Value;
                GlobalVariables.SaveJson();
                _logger.Information("记忆模式自动添加文件修改为: {AutoAddFile}", AddSwitch.IsChecked.Value);
            }
        }
    }
}