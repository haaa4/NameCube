using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.Mode
{
    /// <summary>
    /// BatchMode.xaml 的交互逻辑
    /// </summary>
    public partial class BatchMode : Page
    {
        public ObservableCollection<AllName> AllNames { get; set; } = new ObservableCollection<AllName>();
        public class AllName
        {
            public string Name { get; set; }
        }

        public BatchMode()
        {
            InitializeComponent();
            DataContext = this;
            Numberbox.Value = GlobalVariables.json.BatchModeSettings.Number;
            NameSwitch.IsChecked = GlobalVariables.json.BatchModeSettings.NumberMode;
            Numberbox.IsEnabled = GlobalVariables.json.BatchModeSettings.NumberMode;
            Indexbox.Value = GlobalVariables.json.BatchModeSettings.Index;
            ReCheckBox.IsChecked = GlobalVariables.json.BatchModeSettings.Repetition;
            if(!GlobalVariables.json.BatchModeSettings.NumberMode)
            {
                IndexText.Visibility = Visibility.Collapsed;
                Numberbox.Visibility = Visibility.Collapsed;
            }
        }

        private void NameSwitch_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.BatchModeSettings.NumberMode = NameSwitch.IsChecked.Value;
            Numberbox.IsEnabled = NameSwitch.IsChecked.Value;
            if (!GlobalVariables.json.BatchModeSettings.NumberMode)
            {
                IndexText.Visibility = Visibility.Collapsed;
                Numberbox.Visibility = Visibility.Collapsed;
            }
            else
            {
                IndexText.Visibility = Visibility.Visible;
                Numberbox.Visibility = Visibility.Visible;
            }
            GlobalVariables.SaveJson();
        }

        private void Numberbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            GlobalVariables.json.BatchModeSettings.Number = Numberbox.Text.ToInt32(-114514);
            if (GlobalVariables.json.BatchModeSettings.Number == -114514)
            {
                GlobalVariables.json.BatchModeSettings.Number = 53;
                Numberbox.Value = 53;
            }
            GlobalVariables.SaveJson();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            Numberbox.IsEnabled = false;
            NameSwitch.IsEnabled = false;
            Indexbox.IsEnabled = false;
            ReCheckBox.IsEnabled = false;
            List<int> numbers = new List<int>();
            AllNames.Clear();
            if (GlobalVariables.json.AllSettings.Name.Count <= 2)
            {
                MessageBox.Show("学生名单怎么空空的？\n翻译：学生人数少于3人");
                return;
            }
            if (GlobalVariables.json.BatchModeSettings.NumberMode)
            {
                if (GlobalVariables.json.BatchModeSettings.Index > GlobalVariables.json.BatchModeSettings.Number && !GlobalVariables.json
                    .BatchModeSettings.Repetition)
                {
                    MessageBox.Show("无 中 生 有\n翻译：抽取数量大于实际可抽取数量");
                    StartButton.IsEnabled = true;
                    Numberbox.IsEnabled = true;
                    NameSwitch.IsEnabled = true;
                    Indexbox.IsEnabled = true;
                    ReCheckBox.IsEnabled = true;
                    return;
                }
                for (int i = 1; i <= GlobalVariables.json.BatchModeSettings.Number; i++)
                {
                    numbers.Add(i);
                }
            }
            else
            {
                if (GlobalVariables.json.BatchModeSettings.Index > GlobalVariables.json.AllSettings.Name.Count && !GlobalVariables.json
                    .BatchModeSettings.Repetition)
                {
                    MessageBox.Show("无 中 生 有\n翻译：抽取数量大于实际可抽取数量");
                    StartButton.IsEnabled = true;
                    Numberbox.IsEnabled = true;
                    NameSwitch.IsEnabled = true;
                    Indexbox.IsEnabled = true;
                    ReCheckBox.IsEnabled = true;
                    return;
                }
                for (int i = 1; i <= GlobalVariables.json.AllSettings.Name.Count; i++)
                {
                    numbers.Add(i);
                }
            }
            Random random = new Random();
            int now;
            for (int i = 1; i <= GlobalVariables.json.BatchModeSettings.Index; i++)
            {
                now = random.StrictNext(numbers.Count);
                if (GlobalVariables.json.BatchModeSettings.NumberMode)
                {
                    AllNames.Add(new AllName { Name = numbers[now].ToString() });
                }
                else
                {
                    AllNames.Add(new AllName { Name = GlobalVariables.json.AllSettings.Name[numbers[now] - 1] });
                }
                if (!GlobalVariables.json.BatchModeSettings.Repetition)
                {
                    numbers.RemoveAt(now);
                }
            }
            StartButton.IsEnabled = true;
            Numberbox.IsEnabled = true;
            NameSwitch.IsEnabled = true;
            Indexbox.IsEnabled = true;
            ReCheckBox.IsEnabled = true;
        }

        private void Indexbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            GlobalVariables.json.BatchModeSettings.Index = Indexbox.Text.ToInt32(-114514);
            if (GlobalVariables.json.BatchModeSettings.Index == -114514)
            {
                GlobalVariables.json.BatchModeSettings.Index = 10;
                Indexbox.Value = 10;
            }
            GlobalVariables.SaveJson();
        }

        private void ReCheckBox_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.BatchModeSettings.Repetition = ReCheckBox.IsChecked.Value;
            GlobalVariables.SaveJson();
        }

        private void Numberbox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            GlobalVariables.json.BatchModeSettings.Number = (int)Numberbox.Value;
            GlobalVariables.SaveJson();
        }

        private void Indexbox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            GlobalVariables.json.BatchModeSettings.Index = (int)Indexbox.Value;
            GlobalVariables.SaveJson();
        }
    }
}
