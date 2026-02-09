using Masuit.Tools;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Serilog;

namespace NameCube.Setting.ModeSetting
{
    /// <summary>
    /// MemoryFactorModeSettings.xaml 的交互逻辑
    /// </summary>
    public partial class MemoryFactorModeSettings : Page
    {
        private static readonly ILogger _logger = Log.ForContext<MemoryFactorModeSettings>();

        bool CanChange;
        public MemoryFactorModeSettings()
        {
            InitializeComponent();
            _logger.Debug("记忆因子模式设置页面初始化开始");

            CanChange = false;
            LockedCheck.IsChecked = GlobalVariablesData.config.MemoryFactorModeSettings.Locked;
            Speed.Value = GlobalVariablesData.config.MemoryFactorModeSettings.Speed - 10;

            if (GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening == null)
            {
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening = new List<int> { 4, 2, 3, 4, 2, 2, 3, 1, 1, 2 };
                _logger.Debug("记忆因子模式概率数组为空，使用默认值");
            }

            Random1.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[0];
            Random2.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[1];
            Random3.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[2];
            Random4.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[3];
            Random5.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[4];
            Random6.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[5];
            Random7.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[6];
            Random8.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[7];
            Random9.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[8];
            Random10.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[9];
            CanChange = true;

            _logger.Information("记忆因子模式设置加载完成，锁定状态: {Locked}, 速度: {Speed}",
                LockedCheck.IsChecked,
                Speed.Value);
        }

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.MemoryFactorModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                _logger.Information("记忆因子模式锁定状态修改为: {Locked}", LockedCheck.IsChecked.Value);
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.MemoryFactorModeSettings.Speed = (int)Speed.Value + 10;
                GlobalVariablesData.SaveConfig();
                _logger.Debug("记忆因子模式速度修改为: {Speed}", (int)Speed.Value + 10);
            }
        }

        private void NumberBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            int number;
            e.Handled = !int.TryParse(e.Text.Insert(e.Text.Length, e.Text), out number);
        }

        private void Random_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[0] = ((int?)Random1.Value) ?? 0;
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[1] = ((int?)Random2.Value) ?? 0;
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[2] = ((int?)Random3.Value) ?? 0;
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[3] = ((int?)Random4.Value) ?? 0;
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[4] = ((int?)Random5.Value) ?? 0;
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[5] = ((int?)Random6.Value) ?? 0;
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[6] = ((int?)Random7.Value) ?? 0;
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[7] = ((int?)Random8.Value) ?? 0;
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[8] = ((int?)Random9.Value) ?? 0;
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[9] = ((int?)Random10.Value) ?? 0;
                GlobalVariablesData.SaveConfig();

                _logger.Debug("记忆因子模式概率设置修改为: {ProbabilityArray}",
                    string.Join(",", GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("重置记忆因子模式概率设置");
            CanChange = false;
            GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening = new List<int> { 4, 2, 3, 4, 2, 2, 3, 1, 1, 2 };
            Random1.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[0];
            Random2.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[1];
            Random3.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[2];
            Random4.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[3];
            Random5.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[4];
            Random6.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[5];
            Random7.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[6];
            Random8.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[7];
            Random9.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[8];
            Random10.Value = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[9];
            CanChange = true;
            GlobalVariablesData.SaveConfig();
        }
    }
}