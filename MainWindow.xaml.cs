using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Diagnostics.Eventing.Reader;
using System.Collections.Specialized;
using System.Timers;
using System.Media;
using System.Speech.Synthesis;

namespace NameCube
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        public class AllName
        {
            public string Index { get; set; }
            public string Name { get; set; }
        }
        public class Json
        {
            public List<string> Name { get; set; }
            public bool Speech {  get; set; }
            public bool Dark {  get; set; }
            public int Volume { get; set; }
            public int Speed { get; set; }
        }

        public ObservableCollection<AllName> AllNames { get; set; } = new ObservableCollection<AllName>();
        Json json = new Json();
        public System.Timers.Timer timer;
        int NowIndex = 0;
        bool IsReadyToStop;
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();


        public void SaveJson()
        {
            string jsonString = JsonSerializer.Serialize(json);
            File.WriteAllText("config.json", jsonString);
        }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            if (!File.Exists("config.json"))
            {
                json = new Json
                {
                    Name = new List<string>(),
                    Speech = true,
                    Dark =false,
                    Volume=100,
                    Speed=0,
                };
                json.Name.Add("张三");

                SaveJson();
            }
            else
            {
                string jsonstring = File.ReadAllText("config.json");
                json = JsonSerializer.Deserialize<Json>(jsonstring);
            }
            for (int i = 1; i <= json.Name.Count; i++)
            {
                AllNames.Add(new AllName
                {
                    Index = i.ToString(),
                    Name = json.Name[i - 1]
                });
            }
            SpeechCheck.IsChecked = json.Speech;
            timer = new System.Timers.Timer(50);
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            VolumeSlider.Value = json.Volume;
            SpeedSlider.Value= json.Speed+10;
            _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult); // 选择女声
            _speechSynthesizer.Rate = json.Speed; // 语速 (-10 ~ 10)
            _speechSynthesizer.Volume = json.Volume;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsReadyToStop)
            {
                timer.Interval *= 1.5;
            }
            if (timer.Interval >= 1500)
            {
                Random random = new Random();
                if (random.Next(0, 2) == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        NowNumberText.Text = AllNames[NowIndex].Name;
                    });
                    NowIndex++;
                    if (NowIndex >= AllNames.Count)
                    {
                        NowIndex = 0;

                    }
                }
                Dispatcher.Invoke(() =>
                {
                    StartButton.IsEnabled = true;
                    StartButton.Content = "开始";
                    if (SpeechCheck.IsChecked.Value)
                    {
                       _speechSynthesizer.SpeakAsync(NowNumberText.Text);
                    }
                });
                timer.Stop();
                return;
            }
            Dispatcher.Invoke(() =>
            {
                NowNumberText.Text = AllNames[NowIndex].Name;
            });
            NowIndex++;
            if (NowIndex >= AllNames.Count)
            {
                NowIndex = 0;
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
                            Index = (++index).ToString(),
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
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveJson();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (StartButton.Content.ToString() == "开始")
            {
                AllNames.Add(new AllName
                {
                    Index = (AllNames.Count + 1).ToString(),
                    Name = AddNameTextBox.Text
                });
                json.Name.Add(AddNameTextBox.Text);
                AddNameTextBox.Text = "";
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (StartButton.Content.ToString() == "开始")
            {
                AllNames.Clear();
                json.Name.Clear();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (AllNames.Count != 0 && StartButton.Content.ToString() == "开始")
            {
                Button button = sender as Button;
                if (button != null && button.CommandParameter is AllName allnames)
                {
                    AllNames.Remove(allnames);
                    json.Name.Remove(allnames.Name);
                    for (int i = 1; i <= AllNames.Count; i++)
                    {
                        AllNames[i - 1].Index = i.ToString();
                    }
                }
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (AllNames.Count == 0)
            {
                MessageBox.Show("请先添加学生名单！");
                return;
            }
            if (StartButton.Content.ToString() == "开始")
            {
                NowIndex = 0;
                StartButton.Content = "结束";
                timer.Interval = 20;
                timer.Start();
                IsReadyToStop = false;
            }
            else
            {
                StartButton.Content = "暂停中";
                IsReadyToStop = true;
                StartButton.IsEnabled = false;
            }
        }

        private void FluentWindow_Closed(object sender, EventArgs e)
        {
            json.Speech = SpeechCheck.IsChecked.Value;
            SaveJson();
        }

        private void AddNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click_1(sender, e);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            json.Dark = DarkLight.IsChecked.Value;
            if (DarkLight.IsChecked.Value)
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                    Wpf.Ui.Appearance.ApplicationTheme.Dark, // Theme type
                     Wpf.Ui.Controls.WindowBackdropType.Auto,  // Background type
                     true                                      // Whether to change accents automatically
                   );
            }
            else
            {
                Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
                    Wpf.Ui.Appearance.ApplicationTheme.Light, // Theme type
                     Wpf.Ui.Controls.WindowBackdropType.Mica,  // Background type
                     true                                      // Whether to change accents automatically
                   );
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            json.Volume=(int)VolumeSlider.Value;
            _speechSynthesizer.Volume = json.Volume;
            VolumeText.Text = "音量(" + json.Volume.ToString() + "%)";
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            json.Speed = (int)SpeedSlider.Value - 10;
            _speechSynthesizer.Rate= json.Speed;
            SpeedText.Text = "速度(" + json.Speed.ToString() + ")";
        }
    }
}
