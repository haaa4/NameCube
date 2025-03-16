using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Speech.Synthesis;
using System.Text.Json;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace NameCube
{
    /// <summary>
    /// OnePeopleMode.xaml 的交互逻辑
    /// </summary>
    public partial class OnePeopleMode : Page
    {
        public class AllName
        {
            public string Index { get; set; }
            public string Name { get; set; }
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
        public OnePeopleMode()
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
                        Index = i.ToString(),
                        Name = json.Name[i - 1]
                    });
                }
            }
            SpeechCheck.IsChecked = json.Speech;
            WaitCheck.IsChecked = json.Wait;
            timer = new System.Timers.Timer(50);
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
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
            try
            {
                Dispatcher.Invoke(() =>
                {
                    NowNumberText.Text = AllNames[NowIndex].Name;
                });
            }
            catch (Exception ex)
            {
            }
            NowIndex++;
            if (NowIndex >= AllNames.Count)
            {
                NowIndex = 0;
            }

        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ReadJson();
            if (AllNames.Count == 0)
            {
                MessageBox.Show("请先添加学生名单！");
                return;
            }
            if (StartButton.Content.ToString() == "开始")
            {
                StartButton.Content = "结束";
                timer.Interval = 20;
                timer.Start();
                IsReadyToStop = false;
            }
            else
            {
                if (!json.Wait)
                {
                    StartButton.Content = "暂停中";
                    IsReadyToStop = true;
                    StartButton.IsEnabled = false;
                }
                else
                {
                    timer.Stop();
                    StartButton.Content = "开始";
                    _speechSynthesizer.SpeakAsync(NowNumberText.Text);
                }
            }
        }
        private void ReadJson()
        {
            string JsonString = File.ReadAllText("config.json");
            json = JsonSerializer.Deserialize<Json>(JsonString);
            json.Speech = (bool)SpeechCheck.IsChecked;
        }
        private void ReadAndSaveJson()
        {
            ReadJson();
            SaveJson();
        }



        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveJson();
        }

        private void SpeechCheck_Click(object sender, RoutedEventArgs e)
        {
            json.Speech = (bool)SpeechCheck.IsChecked;
            SaveJson();
        }

        private void WaitCheck_Click(object sender, RoutedEventArgs e)
        {
            json.Wait = (bool)WaitCheck.IsChecked;
            SaveJson();
        }
    }
}
