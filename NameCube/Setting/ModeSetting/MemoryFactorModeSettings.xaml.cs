using Masuit.Tools;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.Setting.ModeSetting
{
    /// <summary>
    /// MemoryFactorModeSettings.xaml 的交互逻辑
    /// </summary>
    public partial class MemoryFactorModeSettings : Page
    {
        bool CanChange;
        public MemoryFactorModeSettings()
        {
            InitializeComponent();
            CanChange = false;
            LockedCheck.IsChecked = GlobalVariables.json.MemoryFactorModeSettings.Locked;
            Speed.Value = GlobalVariables.json.MemoryFactorModeSettings.Speed - 10;
            if(GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening==null)
            {
                GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening = new List<int> { 4, 2, 3, 4, 2, 2, 3, 1, 1 ,2};
            }
            Random1.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[0];
            Random2.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[1];
            Random3.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[2];
            Random4.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[3];
            Random5.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[4];
            Random6.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[5];
            Random7.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[6];
            Random8.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[7];
            Random9.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[8];
            Random10.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[9];
            CanChange = true;
        }

        private void LockedCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.MemoryFactorModeSettings.Locked = LockedCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void Speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariables.json.MemoryFactorModeSettings.Speed = (int)Speed.Value + 10;
                GlobalVariables.SaveJson();
            }
        }

        private void NumberBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            int number;
            e.Handled = !int.TryParse(e.Text.Insert(e.Text.Length, e.Text), out number);
        }

        private void Random_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if(CanChange)
            {
                GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[0] = ((int?)Random1.Value)??0;
                GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[1] = ((int?)Random2.Value)??0;
                GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[2] = ((int?)Random3.Value)??0;
                GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[3] = ((int?)Random4.Value)??0;
                GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[4] = ((int?)Random5.Value)??0;
                GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[5] = ((int?)Random6.Value)??0;
                GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[6] = ((int?)Random7.Value)??0;
                GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[7] = ((int?)Random8.Value)??0;
                GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[8] = ((int?)Random9.Value) ?? 0;
                GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[9] = ((int?)Random10.Value) ?? 0;
                GlobalVariables.SaveJson();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CanChange = false;
            GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening = new List<int> { 4, 2, 3, 4, 2, 2, 3, 1, 1 ,2};
            Random1.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[0];
            Random2.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[1];
            Random3.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[2];
            Random4.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[3];
            Random5.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[4];
            Random6.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[5];
            Random7.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[6];
            Random8.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[7];
            Random9.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[8];
            Random10.Value = GlobalVariables.json.MemoryFactorModeSettings.probabilityOfHappening[9];
            CanChange = true;
            GlobalVariables.SaveJson();
        }
    }
    
}
