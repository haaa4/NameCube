﻿using Masuit.Tools;
using Masuit.Tools.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace NameCube.Mode
{
    /// <summary>
    /// MemoryFactorMode.xaml 的交互逻辑
    /// </summary>
    public partial class MemoryFactorMode : Page
    {
        public class ThisModeJson : INotifyPropertyChanged
        {
            private string _name;

            public string Name
            {
                get => _name;
                set
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }

            private int _factor;

            public int Factor
            {
                get => _factor;
                set
                {
                    if (_factor != value)
                    {
                        _factor = value;
                        OnPropertyChanged(nameof(Factor));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        /// <summary>
        /// 本模式的局部设置
        /// </summary>
        public ObservableCollection<ThisModeJson> thisModeJson { get; set; } = new ObservableCollection<ThisModeJson>();
        public System.Timers.Timer timer = new System.Timers.Timer();
        public SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        public bool IsCanStop = false;
        public List<string> Trainings { get; set; } = new List<string>();
        public MemoryFactorMode()
        {
            InitializeComponent();
            DataContext = this;
            if (!GlobalVariables.json.AllSettings.SystemSpeech)
            {
                _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult); // 选择女声
                _speechSynthesizer.Rate = GlobalVariables.json.AllSettings.Speed; // 语速 (-10 ~ 10)
                _speechSynthesizer.Volume = GlobalVariables.json.AllSettings.Volume;
            }
            timer.Elapsed += Timer_Elapsed;
            SpeechButton.IsChecked = GlobalVariables.json.MemoryFactorModeSettings.Speech;
            SpeechButton.IsEnabled = !GlobalVariables.json.MemoryFactorModeSettings.Locked;
            ResetButton.IsEnabled = !GlobalVariables.json.MemoryFactorModeSettings.Locked;
            string FilePath = System.IO.Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode");
            Directory.CreateDirectory(FilePath);
            StartLoad();

        }
        private async void StartLoad()
        {
            await Task.Run(async () =>
            {

                string FilePath = System.IO.Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode");
                if (!File.Exists(Path.Combine(FilePath, "Memory.json")))
                {
                    LogManager.Info("找不到文件");
                    if (GlobalVariables.json.AllSettings.Name.Count <= 1)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            StartButton.IsEnabled = false;
                            ResetButton.IsEnabled = false;
                        }));
                        System.Windows.Forms.MessageBox.Show("我们，拒 绝 签 字\n翻译：学生名单少于两位，无法初始化");
                        return;
                    }

                    // 通过 Add 方法填充现有集合
                    thisModeJson.Clear();
                    foreach (var name in GlobalVariables.json.AllSettings.Name)
                    {
                        thisModeJson.Add(new ThisModeJson
                        {
                            Name = name,
                            Factor = 1
                        });
                    }
                    SaveThisJson();
                }
                else
                {
                    // 反序列化后合并到现有集合
                    string jsonString = File.ReadAllText(Path.Combine(FilePath, "Memory.json"));
                    var loadedData = JsonConvert.DeserializeObject<ObservableCollection<ThisModeJson>>(jsonString);
                    this.Dispatcher.Invoke(() =>
                    {
                        thisModeJson.Clear();
                        foreach (var item in loadedData)
                        {
                            thisModeJson.Add(item);
                        }
                    });

                }
                for (int i = 0; i < thisModeJson.Count; i++)
                {
                    for (int ii = 1; ii <= thisModeJson[i].Factor; ii++)
                    {
                        Trainings.Add(thisModeJson[i].Name);
                    }
                }
                this.Dispatcher.Invoke(() =>
                {
                    Count.Text = "总概率因子数量:" + Trainings.Count.ToString();
                });

            });

        }
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Random rnd = new Random();
            if (IsCanStop)
            {
                return;
            }
            this.Dispatcher.Invoke(new Action(() =>
            {
                NowNumberText.Text = Trainings[rnd.StrictNext(Trainings.Count)];
            }));
            if (IsCanStop)
            {
                IsCanStop = false;
            }

        }

        public void SaveThisJson()
        {
            LogManager.Info("保存基因因子设置");

            string configPath = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Memory.json");

            try
            {
                // 确保目录存在
                Directory.CreateDirectory(GlobalVariables.configDir);

                string jsonString = JsonConvert.SerializeObject(thisModeJson);
                File.WriteAllText(configPath, jsonString);
            }
            catch (Exception ex)
            {
                // 记录错误或提示用户
                System.Windows.Forms.MessageBox.Show($"保存配置失败: {ex.Message}");
                LogManager.Error(ex);
            }
        }



        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            _speechSynthesizer.SpeakAsyncCancelAll();
            if (StartButton.Content.ToString() == "开始")
            {
                FinishNumberText.Visibility = Visibility.Hidden;
                NowNumberText.Visibility = Visibility.Visible;
                StartButton.Content = "结束";
                timer.Interval = GlobalVariables.json.MemoryFactorModeSettings.Speed;
                timer.Start();
                StartButton.IsEnabled = true;
            }
            else
            {
                StartButton.Content = "开始";
                timer.Stop();
                IsCanStop = true;
                string NowNumberTextValue = NowNumberText.Text;
                FinishNumberText.Text = NowNumberTextValue;
                NowNumberText.Visibility = Visibility.Hidden;
                FinishNumberText.Visibility = Visibility.Visible;
                if (GlobalVariables.json.MemoryFactorModeSettings.Speech)
                {
                    _speechSynthesizer.SpeakAsync(NowNumberTextValue);
                }
                int delete = 0, GetRandom;
                Random random = new Random();
                for (int i = 0; i < thisModeJson.Count; i++)
                {
                    GetRandom = random.StrictNext(4);
                    thisModeJson[i].Factor += GetRandom;
                    for (int j = 1; j <= GetRandom; j++)
                    {
                        Trainings.Add(thisModeJson[i].Name);
                    }
                    if (thisModeJson[i].Name == NowNumberTextValue)
                    {
                        delete = i;
                    }
                }
                thisModeJson[delete].Factor = 0;
                Trainings.RemoveAll(s => s == NowNumberTextValue);
                SaveThisJson();
                StartButton.IsEnabled = true;
                IsCanStop = false;
                NowNumberText.Text = NowNumberTextValue;
                Count.Text = "总概率因子数量:" + Trainings.Count.ToString();
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.MemoryFactorModeSettings.Speech = SpeechButton.IsChecked.Value;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult result = MessageBox.Show("确定重置概率因子吗？\n（重置概率因子后配置文件不会删除，而是会创建一个备份", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups"));
                    File.Copy(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Memory.json"), Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups",
                        DateTime.Now.ToString("yyyy_MM_d_HH_MM_ss")));
                    File.Delete(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Memory.json"));
                    StartButton.IsEnabled = false;
                    ResetButton.IsEnabled = false;
                    MessageBox.Show("重置成功，请重新打开界面", "完成");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("出错" + ex.Message, "错误");
                    LogManager.Error(ex);
                    return;
                }
            }
        }


    }
}
