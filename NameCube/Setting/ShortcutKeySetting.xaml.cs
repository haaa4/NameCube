using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace NameCube.Setting
{
    /// <summary>
    /// ShortcutKeySetting.xaml 的交互逻辑
    /// </summary>
    public partial class ShortcutKeySetting : Page
    {
        public ShortcutKeySetting()
        {
            InitializeComponent();
            canChange = false;
            Operation.SelectedIndex = GlobalVariables.json.ShortCutKey.Way;
            if(GlobalVariables.json.ShortCutKey.keys==null||GlobalVariables.json.ShortCutKey.keys.Count==0)
            {
                GlobalVariables.json.ShortCutKey.keys = new List<Key>();
            }
            else
            {
                foreach (Key key2 in GlobalVariables.json.ShortCutKey.keys)
                {
                    KeyText.Text = KeyText.Text + key2.ToString() + " ";
                }
            }
            canChange= true;
        }
        bool IsChoosing=false,canChange=false;
        private void KeyChooseButton_Click(object sender, RoutedEventArgs e)
        {
            if(IsChoosing)
            {
                KeyChooseButton.Content = "编辑";
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.CanUseShortCutKey = true;
                IsChoosing = false;
            }
            else
            {
                GlobalVariables.json.ShortCutKey.keys= new List<Key>();
                KeyText.Text = "";
                KeyChooseButton.Content = "完成";
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.CanUseShortCutKey = false;
                IsChoosing = true;
            }

        }

        private void Operation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(canChange)
            {
                GlobalVariables.json.ShortCutKey.Way = Operation.SelectedIndex;
                GlobalVariables.SaveJson();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.CanUseShortCutKey = true;
        }

        private void KeyChooseButton_KeyDown(object sender, KeyEventArgs e)
        {
            if (IsChoosing&&GlobalVariables.json.ShortCutKey.keys.Count<=4) 
            {
                Key key = e.Key;
                foreach (Key key1 in GlobalVariables.json.ShortCutKey.keys) 
                {
                    if (key1 == key) 
                    {
                        return;
                    }
                }
                GlobalVariables.json.ShortCutKey.keys.Add(key);
                KeyText.Text = "";
                foreach (Key key2 in GlobalVariables.json.ShortCutKey.keys)
                {
                    KeyText.Text=KeyText.Text+key2.ToString()+" ";
                }
                KeyText.Text.Remove(KeyText.Text.Length-1);
                GlobalVariables.SaveJson();
            }
        }
    }
}
