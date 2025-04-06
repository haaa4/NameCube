using Masuit.Tools;
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

namespace NameCube.ToolBox
{
    /// <summary>
    /// ShutDown.xaml 的交互逻辑
    /// </summary>
    public partial class ShutDown : Page
    {
        bool CanChange;
        public ShutDown()
        {
            InitializeComponent();
            CanChange = false;
            PowerOffSwitch.IsChecked = GlobalVariables.json.StartToDo.PowerOff;
            Hour.Value = GlobalVariables.json.StartToDo.HourPowerOff;
            Min.Value = GlobalVariables.json.StartToDo.MinPowerOff;
            CanChange= true;
        }

        private void PowerOffSwitch_Click(object sender, RoutedEventArgs e)
        {
            if(CanChange)
            {
                GlobalVariables.json.StartToDo.PowerOff = PowerOffSwitch.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void Hour_TextChanged(object sender, TextChangedEventArgs e)
        {
            int get = Hour.Text.ToInt32(-114514);
            if(get==-114514)
            {
                Hour.Value = 20;
            }
            else
            {
                Hour.Value = get;
            }
        }

        private void Min_TextChanged(object sender, TextChangedEventArgs e)
        {
            int get = Min.Text.ToInt32(-114514);
            if (get == -114514)
            {
                Min.Value = 20;
            }
            else
            {
                Min.Value = get;
            }
        }

        private void Hour_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if(CanChange)
            {
                GlobalVariables.json.StartToDo.HourPowerOff = (int)Hour.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void Min_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (CanChange)
            {
                GlobalVariables.json.StartToDo.MinPowerOff = (int)Min.Value;
                GlobalVariables.SaveJson();
            }
        }
    }
}
