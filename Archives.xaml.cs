using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NameCube
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
        Json json = new Json();


        public void SaveJson()
        {
            string jsonString = JsonSerializer.Serialize(json);
            File.WriteAllText("config.json", jsonString);
        }
        public Archives()
        {
            InitializeComponent();
            DataContext = this;
            if (!File.Exists("config.json"))
            {
                json = new Json
                {
                    Name = new List<string>(),
                    Speech = true,
                    Dark = false,
                    Volume = 100,
                    Speed = 0,
                    Wait = false,
                };
                json.Name.Add("张三");

                SaveJson();
            }
            else
            {
                string jsonstring = File.ReadAllText("config.json");
                json = JsonSerializer.Deserialize<Json>(jsonstring);
            }
            if (json.Name != null)
            {
                for (int i = 1; i <= json.Name.Count; i++)
                {
                    AllNames.Add(new AllName
                    {
                        Name = json.Name[i - 1]
                    });
                }
            }
            else
            {
                json.Name = new List<string>();
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "从文本文档导入";
            openFileDialog.Filter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                int index = 0;

                AllNames.Clear();
                try
                {
                    foreach (string line in File.ReadLines(openFileDialog.FileName))
                    {
                        AllNames.Add(new AllName
                        {
                            Name = line
                        });
                        json.Name.Add(line);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            SaveJson();
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveJson();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {


            AllNames.Add(new AllName
            {
                Name = AddNameTextBox.Text
            });
            json.Name.Add(AddNameTextBox.Text);
            AddNameTextBox.Text = "";
            SaveJson();

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {


            AllNames.Clear();
            json.Name.Clear();
            SaveJson();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (AllNames.Count != 0)
            {
                Button button = sender as Button;
                if (button != null && button.CommandParameter is AllName allnames)
                {
                    AllNames.Remove(allnames);
                    json.Name.Remove(allnames.Name);
                }
            }
            SaveJson();
        }
        private void AddNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click_1(sender, e);
            }
        }

        private void AddNameTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveJson();
        }
    }
}
