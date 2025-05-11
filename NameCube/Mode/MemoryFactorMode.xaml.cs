using Masuit.Tools;
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
using System.Windows.Media;
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
            if (GlobalVariables.json.MemoryFactorModeSettings.Speed == 0)
            {
                GlobalVariables.json.MemoryFactorModeSettings.Speed = 20;
            }
            StartLoad();
            NowNumberText.Foreground = GlobalVariables.json.AllSettings.color;
            FinishNumberText.Foreground=GlobalVariables.json.AllSettings.color;
            NowNumberText.FontFamily = GlobalVariables.json.AllSettings.Font;
            FinishNumberText.FontFamily = GlobalVariables.json.AllSettings.Font;
        }
        private async void StartLoad()
        {
            await Task.Run(() =>
            {
                string FilePath = System.IO.Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode");
                if (!File.Exists(Path.Combine(FilePath, "Memory.json")))
                {
                    if (GlobalVariables.json.AllSettings.Name.Count <= 1)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            StartButton.IsEnabled = false;
                            ResetButton.IsEnabled = false;
                        }));
                        MessageBoxFunction.ShowMessageBoxWarning("我们，拒 绝 签 字\n翻译：学生名单少于两位，无法初始化");
                        return Task.CompletedTask;
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
                int max = 0, MaxIndex = -1;
                for (int i = 0; i < thisModeJson.Count; i++)
                {
                    for (int ii = 1; ii <= thisModeJson[i].Factor; ii++)
                    {
                        Trainings.Add(thisModeJson[i].Name);
                    }
                    if (thisModeJson[i].Factor > max)
                    {
                        max = thisModeJson[i].Factor;
                        MaxIndex = i;
                    }
                }
                if(MaxIndex!=-1&&GlobalVariables.json.MemoryFactorModeSettings.MaxName==null)
                {
                    GlobalVariables.json.MemoryFactorModeSettings.MaxName = thisModeJson[MaxIndex].Name;
                    GlobalVariables.json.MemoryFactorModeSettings.MaxTimes = 1;
                }
                this.Dispatcher.Invoke(() =>
                {
                    Count.Text=Trainings.Count.ToString();
                    //if (MaxIndex != -1)
                    //{
                    //    MaxName.Text = "当前概率最高:" + thisModeJson[MaxIndex].Name + "---" + thisModeJson[MaxIndex].Factor.ToString() + "(" + (Math.Round((double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100, 2)).ToString() + "%)";
                    //}
                    //if (GlobalVariables.json.MemoryFactorModeSettings.MaxTimes >= 5 && GlobalVariables.json.MemoryFactorModeSettings.MaxTimes < 10)
                    //{
                    //    MaxName.Text = MaxName.Text + "(剩" + (10 - GlobalVariables.json.MemoryFactorModeSettings.MaxTimes).ToString() + "次触发保底)";
                    //    MaxName.Foreground = Brushes.Black;
                    //}
                    //else if (GlobalVariables.json.MemoryFactorModeSettings.MaxTimes >= 10 && GlobalVariables.json.MemoryFactorModeSettings.MaxTimes <= 12)
                    //{
                    //    MaxName.Text = "当前概率最高:" + thisModeJson[MaxIndex].Name + "---" + thisModeJson[MaxIndex].Factor.ToString() + "(" + ((GlobalVariables.json.MemoryFactorModeSettings.MaxTimes - 9) * 25) + "%)";
                    //    MaxName.Foreground = GlobalVariables.json.AllSettings.color;
                    //}
                    //else if (GlobalVariables.json.MemoryFactorModeSettings.MaxTimes >= 13)
                    //{
                    //    MaxName.Text = "当前概率最高:" + thisModeJson[MaxIndex].Name + "---" + thisModeJson[MaxIndex].Factor.ToString() + "(100%)";
                    //    MaxName.Foreground = Brushes.Red;
                    //}
                    MaxIndexName.Text= thisModeJson[MaxIndex].Name;
                    MaxIndexText.Text = thisModeJson[MaxIndex].Factor.ToString();
                    if(GlobalVariables.json.MemoryFactorModeSettings.MaxTimes<10)
                    {
                        Mininum.Visibility = Visibility.Visible;
                        MinimumText.Visibility = Visibility.Visible;
                        MininumState.Visibility = Visibility.Collapsed;
                        MaxIndexRealProbability.Text = (Math.Round((double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100, 2)).ToString() + "%";
                        MinimumText.Text=(10 - GlobalVariables.json.MemoryFactorModeSettings.MaxTimes).ToString() + "次";
                    }
                    else if(GlobalVariables.json.MemoryFactorModeSettings.MaxTimes >= 10 && GlobalVariables.json.MemoryFactorModeSettings.MaxTimes <= 12)
                    {
                        Mininum.Visibility = Visibility.Collapsed;
                        MinimumText.Visibility = Visibility.Collapsed;
                        MininumState.Visibility = Visibility.Visible;
                        MaxIndexRealProbability.Text = ((GlobalVariables.json.MemoryFactorModeSettings.MaxTimes - 9) * 25) + "%";
                        MaxIndexRealProbability.Foreground = GlobalVariables.json.AllSettings.color;
                    }
                    else
                    {
                        Mininum.Visibility = Visibility.Collapsed;
                        MinimumText.Visibility = Visibility.Collapsed;
                        MininumState.Visibility = Visibility.Visible;
                        MaxIndexRealProbability.Text = "100%";
                        MaxIndexRealProbability.Foreground = Brushes.Red;
                    }
                });
                
                return Task.CompletedTask;

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
                MessageBoxFunction.ShowMessageBoxError($"保存配置失败: {ex.Message}");
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
                Random random = new Random();
                IsCanStop = true;
                bool mininum = false;
                string NowNumberTextValue = NowNumberText.Text;
                if(GlobalVariables.json.MemoryFactorModeSettings.MaxTimes>=10)
                {
                    int get=random.StrictNext(100);
                    if (get <= (GlobalVariables.json.MemoryFactorModeSettings.MaxTimes - 9) * 25)
                    {
                        NowNumberTextValue = GlobalVariables.json.MemoryFactorModeSettings.MaxName;
                        mininum = true;
                    }

                }
                FinishNumberText.Text = NowNumberTextValue;
                NowNumberText.Visibility = Visibility.Hidden;
                FinishNumberText.Visibility = Visibility.Visible;
                if (GlobalVariables.json.MemoryFactorModeSettings.Speech)
                {
                    _speechSynthesizer.SpeakAsync(NowNumberTextValue);
                }
                int delete = 0, GetRandom,max=0,MaxIndex=-1,lastfactor=0;
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
                        lastfactor = thisModeJson[delete].Factor;
                        thisModeJson[delete].Factor = 0;
                    }
                    else if (thisModeJson[i].Factor>max)
                    {
                        max = thisModeJson[i].Factor;
                        MaxIndex = i;
                    }
                }

                Trainings.RemoveAll(s => s == NowNumberTextValue);
                GetRandom = random.StrictNext(thisModeJson.Count);
                int Past = thisModeJson[GetRandom].Factor;
                for(int i = 1; i <= thisModeJson[GetRandom].Factor;i++)
                {
                    Trainings.Add(thisModeJson[GetRandom].Name);
                }
                thisModeJson[GetRandom].Factor *= 2;
                //multiply.Text= "概率UP:" + thisModeJson[GetRandom].Name + "---" + Past.ToString() + "→" + thisModeJson[GetRandom].Factor.ToString() + "(" + (Math.Round((double)thisModeJson[GetRandom].Factor / Trainings.Count * 100, 2)).ToString() + "%)";
                UpName.Text= thisModeJson[GetRandom].Name;
                UpFactor.Text = Past.ToString() + "→" + thisModeJson[GetRandom].Factor.ToString();
                UpRealProbability.Text= (Math.Round((double)thisModeJson[GetRandom].Factor / Trainings.Count * 100, 2)).ToString() + "%";
                if (thisModeJson[GetRandom].Factor>max)
                {
                    MaxIndex = GetRandom;
                }
                if (MaxIndex != -1)
                {
                    Mininum.Visibility = Visibility.Visible;
                    MinimumText.Visibility = Visibility.Visible;
                    MininumState.Visibility = Visibility.Collapsed;
                    MaxIndexRealProbability.Text = (Math.Round((double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100, 2)).ToString() + "%";
                    MinimumText.Text = (10 - GlobalVariables.json.MemoryFactorModeSettings.MaxTimes).ToString() + "次";
                    MaxIndexName.Text = thisModeJson[MaxIndex].Name;
                    MaxIndexText.Text = thisModeJson[MaxIndex].Factor.ToString();
                }
                if (thisModeJson[MaxIndex].Name==GlobalVariables.json.MemoryFactorModeSettings.MaxName)
                {
                    GlobalVariables.json.MemoryFactorModeSettings.MaxTimes++;
                    MaxIndexName.Text = thisModeJson[MaxIndex].Name;
                    MaxIndexText.Text = thisModeJson[MaxIndex].Factor.ToString();
                    if (GlobalVariables.json.MemoryFactorModeSettings.MaxTimes < 10)
                    {
                        Mininum.Visibility = Visibility.Visible;
                        MinimumText.Visibility = Visibility.Visible;
                        MininumState.Visibility = Visibility.Collapsed;
                        MaxIndexRealProbability.Text = (Math.Round((double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100, 2)).ToString() + "%";
                        MinimumText.Text = (10 - GlobalVariables.json.MemoryFactorModeSettings.MaxTimes).ToString() + "次";
                    }
                    else if (GlobalVariables.json.MemoryFactorModeSettings.MaxTimes >= 10 && GlobalVariables.json.MemoryFactorModeSettings.MaxTimes <= 12)
                    {
                        Mininum.Visibility = Visibility.Collapsed;
                        MinimumText.Visibility = Visibility.Collapsed;
                        MininumState.Visibility = Visibility.Visible;
                        MaxIndexRealProbability.Text = ((GlobalVariables.json.MemoryFactorModeSettings.MaxTimes - 9) * 25) + "%";
                        MaxIndexRealProbability.Foreground = GlobalVariables.json.AllSettings.color;
                    }
                    else
                    {
                        Mininum.Visibility = Visibility.Collapsed;
                        MinimumText.Visibility = Visibility.Collapsed;
                        MininumState.Visibility = Visibility.Visible;
                        MaxIndexRealProbability.Text = "100%";
                        MaxIndexRealProbability.Foreground = Brushes.Red;
                    }

                }
                else
                {
                    GlobalVariables.json.MemoryFactorModeSettings.MaxName = thisModeJson[MaxIndex].Name;
                    GlobalVariables.json.MemoryFactorModeSettings.MaxTimes = 1;
                    Mininum.Visibility = Visibility.Visible;
                    MinimumText.Visibility = Visibility.Visible;
                    MininumState.Visibility = Visibility.Collapsed;
                    MaxIndexRealProbability.Text = (Math.Round((double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100, 2)).ToString() + "%";
                    MinimumText.Text = (10 - GlobalVariables.json.MemoryFactorModeSettings.MaxTimes).ToString() + "次";
                    MaxIndexRealProbability.Foreground = Brushes.Black;
                }
                if(mininum)
                {
                    LastFactorText.Text = "(保底)";
                }
                else
                {
                    LastFactorText.Text = "(非保底:" + lastfactor.ToString() + ")";
                }
                SaveThisJson();
                GlobalVariables.SaveJson();
                StartButton.IsEnabled = true;
                IsCanStop = false;
                NowNumberText.Text = NowNumberTextValue;
                Count.Text = Trainings.Count.ToString();
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.MemoryFactorModeSettings.Speech = SpeechButton.IsChecked.Value;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
                try
                {
                    Directory.CreateDirectory(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups"));
                    File.Copy(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Memory.json"), Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups",
                        DateTime.Now.ToString("yyyy_MM_d_HH_MM_ss")));
                    File.Delete(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Memory.json"));
                    GlobalVariables.json.MemoryFactorModeSettings.MaxName = null;
                    GlobalVariables.json.MemoryFactorModeSettings.MaxTimes = 1;
                    StartButton.IsEnabled = false;
                    ResetButton.IsEnabled = false;
                    MessageBoxFunction.ShowMessageBoxInfo("重置成功，请重新打开界面");
                }
                catch (Exception ex)
                {
                    MessageBoxFunction.ShowMessageBoxError("出错" + ex.Message);
                    return;
                }
        }



    }
}
