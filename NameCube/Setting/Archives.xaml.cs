using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NameCube.Setting
{
    /// <summary>
    /// Archives.xaml 的交互逻辑
    /// </summary>
    public partial class Archives : Page
    {
        public class AllName
        {
            public string Name { get; set; }
        }

        public ObservableCollection<AllName> AllNames { get; set; } = new ObservableCollection<AllName>();



        public Archives()
        {
            InitializeComponent();
            DataContext = this;
            if (GlobalVariables.json.AllSettings.Name != null)
            {
                for (int i = 1; i <= GlobalVariables.json.AllSettings.Name.Count; i++)
                {
                    AllNames.Add(new AllName
                    {
                        Name = GlobalVariables.json.AllSettings.Name[i - 1]
                    });
                }
            }
            else
            {
                GlobalVariables.json.AllSettings.Name = new List<string>();
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "从文本文档导入";
            openFileDialog.Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {

                AllNames.Clear();
                try
                {
                    foreach (string line in File.ReadLines(openFileDialog.FileName))
                    {
                        AllNames.Add(new AllName
                        {
                            Name = line
                        });
                        GlobalVariables.json.AllSettings.Name.Add(line);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            GlobalVariables.SaveJson();
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GlobalVariables.SaveJson();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (AddNameTextBox.Text == "" || AddNameTextBox.Text == null)
            {
                MessageBox.Show("大家好，我叫无名氏\n翻译：添加内容为空！");
                return;
            }
            AllNames.Add(new AllName
            {
                Name = AddNameTextBox.Text
            });
            GlobalVariables.json.AllSettings.Name.Add(AddNameTextBox.Text);
            AddNameTextBox.Text = "";
            GlobalVariables.SaveJson();

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {


            AllNames.Clear();
            GlobalVariables.json.AllSettings.Name.Clear();
            GlobalVariables.SaveJson();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (AllNames.Count != 0)
            {
                Button button = sender as Button;
                if (button != null && button.CommandParameter is AllName allnames)
                {
                    AllNames.Remove(allnames);
                    GlobalVariables.json.AllSettings.Name.Remove(allnames.Name);
                }
            }
            GlobalVariables.SaveJson();
        }
        private void AddNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Button_Click_1(sender, e);
            }
        }

        private void AddNameTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            GlobalVariables.SaveJson();
        }
    }
}
