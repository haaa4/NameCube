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
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                }
                if (GlobalVariables.json.AllSettings.Name.Count >= 120)
                {
                    WarningInfoBar.IsOpen = true;
                }
            }
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (AddNameTextBox.Text == "" || AddNameTextBox.Text == null)
            {
                MessageBoxFunction.ShowMessageBoxWarning("大家好，我叫无名氏\n翻译：添加内容为空！");
                return;
            }
            AllNames.Add(new AllName
            {
                Name = AddNameTextBox.Text
            });
            GlobalVariables.json.AllSettings.Name.Add(AddNameTextBox.Text);
            AddNameTextBox.Text = "";
            if (GlobalVariables.json.AllSettings.Name.Count >= 120)
            {
                WarningInfoBar.IsOpen = true;
            }

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {


            AllNames.Clear();
            GlobalVariables.json.AllSettings.Name.Clear();

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

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            AllNames.Clear();
            GlobalVariables.json.AllSettings.Name.Clear();
            //这是在干嘛(#`O′)
            List<string> Names = new List<string>
            {
                "王梓轩",
                "李若汐",
                "张峻豪",
                "陈思妍",
                "刘翊航",
                "杨雨桐",
                "黄皓宇",
                "周瑾萱",
                "吴泽楷",
                "徐婉晴",
                "孙慕辰",
                "朱语桐",
                "何昱珩",
                "郭婧瑶",
                "林子墨",
                "许若琳",
                "郑皓然",
                "冯悦彤",
                "邓嘉树",
                "蒋舒然",
                "蔡景行",
                "沈奕辰",
                "韩雨萌",
                "谢明哲",
                "曹沐妍",
                "丁昊然",
                "魏瑾瑜",
                "苏允哲",
                "杜心怡",
                "卢泽言",
                "叶思远",
                "潘晨熙",
                "姚星野",
                "董一诺",
                "袁梦琪",
                "夏子谦",
                "薛梓萌",
                "钟雨墨",
                "谭睿泽",
                "邵清妍",
            };
            foreach (string name in Names)
            {
                AllNames.Add(new AllName { Name = name });
                GlobalVariables.json.AllSettings.Name.Add(name);
            }
            if (GlobalVariables.json.AllSettings.Name.Count >= 120)
            {
                WarningInfoBar.IsOpen = true;
            }
        }
    }
}
