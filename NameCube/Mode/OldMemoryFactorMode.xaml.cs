using Masuit.Tools;
using Masuit.Tools.Logging;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
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
using Path = System.IO.Path;

namespace NameCube.Mode
{
    /// <summary>
    /// OldMemoryFactorMode.xaml 的交互逻辑
    /// </summary>
    public partial class OldMemoryFactorMode : Page
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
        public OldMemoryFactorMode()
        {
            InitializeComponent();
            DataContext = this;

            if (!GlobalVariables.json.AllSettings.SystemSpeech)
            {
                _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                _speechSynthesizer.Rate = GlobalVariables.json.AllSettings.Speed;
                _speechSynthesizer.Volume = GlobalVariables.json.AllSettings.Volume;
            }

            timer.Elapsed += Timer_Elapsed;
            SpeechButton.IsChecked = GlobalVariables.json.OldMemoryFactorModeSettings.Speech;
            SpeechButton.IsEnabled = !GlobalVariables.json.OldMemoryFactorModeSettings.Locked ?? true;
            ResetButton.IsEnabled = !GlobalVariables.json.OldMemoryFactorModeSettings.Locked ?? true;

            string FilePath = System.IO.Path.Combine(GlobalVariables.configDir, "Mode_data", "OldMemoryFactoryMode");
            Directory.CreateDirectory(FilePath);

            if (GlobalVariables.json.OldMemoryFactorModeSettings.Speed == 0)
            {
                Log.Debug("重置速度为默认值20");
                GlobalVariables.json.OldMemoryFactorModeSettings.Speed = 20;
            }

            StartLoad();
            NowNumberText.Foreground = GlobalVariables.json.AllSettings.color;
            FinishNumberText.Foreground = GlobalVariables.json.AllSettings.color;
            NowNumberText.FontFamily = GlobalVariables.json.AllSettings.Font;
            FinishNumberText.FontFamily = GlobalVariables.json.AllSettings.Font;

            if (GlobalVariables.json.OldMemoryFactorModeSettings.LastName != null)
            {
                NowNumberText.Text = GlobalVariables.json.OldMemoryFactorModeSettings.LastName;
                Log.Debug("设置上次抽取结果: {LastName}", GlobalVariables.json.OldMemoryFactorModeSettings.LastName);
            }

            Log.Debug("OldMemoryFactorMode页面初始化完成");
        }

        private async void StartLoad()
        {
            try
            {
                Log.Debug("开始加载旧记忆因子数据");
                await Task.Run(() =>
                {
                    string FilePath = System.IO.Path.Combine(GlobalVariables.configDir, "Mode_data", "OldMemoryFactoryMode");
                    if (!File.Exists(Path.Combine(FilePath, "Memory.json")))
                    {
                        if (GlobalVariables.json.AllSettings.Name.Count <= 1)
                        {
                            Log.Warning("学生名单数量不足: {Count} <= 1", GlobalVariables.json.AllSettings.Name.Count);
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                StartButton.IsEnabled = false;
                                ResetButton.IsEnabled = false;
                            }));
                            MessageBoxFunction.ShowMessageBoxWarning("我们，拒 绝 签 字\n翻译：学生名单少于两位，无法初始化");
                            return Task.CompletedTask;
                        }

                        Log.Information("初始化新的记忆因子数据，学生数量: {Count}", GlobalVariables.json.AllSettings.Name.Count);

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
                        Log.Debug("加载现有记忆因子数据");
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

                    if (MaxIndex != -1 && GlobalVariables.json.OldMemoryFactorModeSettings.MaxName == null)
                    {
                        GlobalVariables.json.OldMemoryFactorModeSettings.MaxName = thisModeJson[MaxIndex].Name;
                        GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes = 1;
                        Log.Debug("设置最大因子学生: {Name}, 因子: {Factor}",
                            thisModeJson[MaxIndex].Name,
                            thisModeJson[MaxIndex].Factor);
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        Count.Text = Trainings.Count.ToString();
                        MaxIndexName.Text = thisModeJson[MaxIndex].Name;
                        MaxIndexText.Text = thisModeJson[MaxIndex].Factor.ToString();

                        if (GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes < 10)
                        {
                            Mininum.Visibility = Visibility.Visible;
                            MinimumText.Visibility = Visibility.Visible;
                            MininumState.Visibility = Visibility.Collapsed;
                            MaxIndexRealProbability.Text = (Math.Round((double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100, 2)).ToString() + "%";
                            MinimumText.Text = (10 - GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes).ToString() + "次";
                            Log.Debug("保底状态: 等待保底，剩余 {Remaining} 次", 10 - GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes);
                        }
                        else if (GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes >= 10 && GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes <= 12)
                        {
                            Mininum.Visibility = Visibility.Collapsed;
                            MinimumText.Visibility = Visibility.Collapsed;
                            MininumState.Visibility = Visibility.Visible;
                            MaxIndexRealProbability.Text = ((GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes - 9) * 25) + "%";
                            MaxIndexRealProbability.Foreground = GlobalVariables.json.AllSettings.color;
                            Log.Debug("保底状态: 保底中，概率 {Probability}%", (GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes - 9) * 25);
                        }
                        else
                        {
                            Mininum.Visibility = Visibility.Collapsed;
                            MinimumText.Visibility = Visibility.Collapsed;
                            MininumState.Visibility = Visibility.Visible;
                            MaxIndexRealProbability.Text = "100%";
                            MaxIndexRealProbability.Foreground = Brushes.Red;
                            Log.Debug("保底状态: 强制保底，概率 100%");
                        }
                    });

                    Log.Information("旧记忆因子数据加载完成，总因子数: {TrainingCount}, 最大因子: {MaxFactor} ({MaxName})",
                        Trainings.Count, max, thisModeJson[MaxIndex].Name);

                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载旧记忆因子数据时发生异常");
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
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
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OldMemoryFactorMode定时器处理时发生异常");
            }
        }

        public void SaveThisJson()
        {
            Log.Information("保存旧记忆因子设置");
            string configPath = Path.Combine(GlobalVariables.configDir, "Mode_data", "OldMemoryFactoryMode", "Memory.json");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configPath));
                string jsonString = JsonConvert.SerializeObject(thisModeJson);
                File.WriteAllText(configPath, jsonString);
                Log.Debug("旧记忆因子设置保存成功: {Path}", configPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保存旧记忆因子设置失败");
                MessageBoxFunction.ShowMessageBoxError($"保存配置失败: {ex.Message}");
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartButton.IsEnabled = false;
                _speechSynthesizer.SpeakAsyncCancelAll();

                if (StartButton.Content.ToString() == "开始")
                {
                    Log.Information("开始旧记忆因子抽取");
                    FinishNumberText.Visibility = Visibility.Hidden;
                    NowNumberText.Visibility = Visibility.Visible;
                    StartButton.Content = "结束";
                    timer.Interval = GlobalVariables.json.OldMemoryFactorModeSettings.Speed ?? 10;
                    timer.Start();
                    StartButton.IsEnabled = true;
                }
                else
                {
                    Log.Information("结束旧记忆因子抽取");
                    StartButton.Content = "开始";
                    timer.Stop();
                    Random random = new Random();
                    IsCanStop = true;
                    bool mininum = false;
                    string NowNumberTextValue = NowNumberText.Text;

                    if (GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes >= 10)
                    {
                        int get = random.StrictNext(100);
                        if (get <= (GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes - 9) * 25)
                        {
                            NowNumberTextValue = GlobalVariables.json.OldMemoryFactorModeSettings.MaxName;
                            mininum = true;
                            Log.Debug("触发保底机制: {Name}", NowNumberTextValue);
                        }
                    }

                    FinishNumberText.Text = NowNumberTextValue;
                    NowNumberText.Visibility = Visibility.Hidden;
                    FinishNumberText.Visibility = Visibility.Visible;

                    if (GlobalVariables.json.OldMemoryFactorModeSettings.Speech ?? true)
                    {
                        Log.Debug("语音播报: {Name}", NowNumberTextValue);
                        _speechSynthesizer.SpeakAsync(NowNumberTextValue);
                    }

                    int delete = 0, GetRandom, max = 0, MaxIndex = -1, lastfactor = 0;
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
                            Log.Debug("被抽取学生 {Name} 因子重置为0，原因子: {Factor}", NowNumberTextValue, lastfactor);
                        }
                        else if (thisModeJson[i].Factor > max)
                        {
                            max = thisModeJson[i].Factor;
                            MaxIndex = i;
                        }
                    }

                    Trainings.RemoveAll(s => s == NowNumberTextValue);
                    GetRandom = random.StrictNext(thisModeJson.Count);
                    int Past = thisModeJson[GetRandom].Factor;
                    for (int i = 1; i <= thisModeJson[GetRandom].Factor; i++)
                    {
                        Trainings.Add(thisModeJson[GetRandom].Name);
                    }
                    thisModeJson[GetRandom].Factor *= 2;

                    UpName.Text = thisModeJson[GetRandom].Name;
                    UpFactor.Text = Past.ToString() + "→" + thisModeJson[GetRandom].Factor.ToString();
                    UpRealProbability.Text = (Math.Round((double)thisModeJson[GetRandom].Factor / Trainings.Count * 100, 2)).ToString() + "%";

                    if (thisModeJson[GetRandom].Factor > max)
                    {
                        MaxIndex = GetRandom;
                    }

                    if (MaxIndex != -1)
                    {
                        Mininum.Visibility = Visibility.Visible;
                        MinimumText.Visibility = Visibility.Visible;
                        MininumState.Visibility = Visibility.Collapsed;
                        MaxIndexRealProbability.Text = (Math.Round((double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100, 2)).ToString() + "%";
                        MinimumText.Text = (10 - GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes).ToString() + "次";
                        MaxIndexName.Text = thisModeJson[MaxIndex].Name;
                        MaxIndexText.Text = thisModeJson[MaxIndex].Factor.ToString();
                    }

                    if (thisModeJson[MaxIndex].Name == GlobalVariables.json.OldMemoryFactorModeSettings.MaxName)
                    {
                        GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes++;
                        MaxIndexName.Text = thisModeJson[MaxIndex].Name;
                        MaxIndexText.Text = thisModeJson[MaxIndex].Factor.ToString();

                        if (GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes < 10)
                        {
                            Mininum.Visibility = Visibility.Visible;
                            MinimumText.Visibility = Visibility.Visible;
                            MininumState.Visibility = Visibility.Collapsed;
                            MaxIndexRealProbability.Text = (Math.Round((double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100, 2)).ToString() + "%";
                            MinimumText.Text = (10 - GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes).ToString() + "次";
                            Log.Debug("保底次数增加: {Times}，剩余 {Remaining} 次",
                                GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes,
                                10 - GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes);
                        }
                        else if (GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes >= 10 && GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes <= 12)
                        {
                            Mininum.Visibility = Visibility.Collapsed;
                            MinimumText.Visibility = Visibility.Collapsed;
                            MininumState.Visibility = Visibility.Visible;
                            MaxIndexRealProbability.Text = ((GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes - 9) * 25) + "%";
                            MaxIndexRealProbability.Foreground = GlobalVariables.json.AllSettings.color;
                            Log.Debug("进入保底阶段: {Times}次，概率 {Probability}%",
                                GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes,
                                (GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes - 9) * 25);
                        }
                        else
                        {
                            Mininum.Visibility = Visibility.Collapsed;
                            MinimumText.Visibility = Visibility.Collapsed;
                            MininumState.Visibility = Visibility.Visible;
                            MaxIndexRealProbability.Text = "100%";
                            MaxIndexRealProbability.Foreground = Brushes.Red;
                            Log.Debug("强制保底阶段: {Times}次", GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes);
                        }
                    }
                    else
                    {
                        GlobalVariables.json.OldMemoryFactorModeSettings.MaxName = thisModeJson[MaxIndex].Name;
                        GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes = 1;
                        Mininum.Visibility = Visibility.Visible;
                        MinimumText.Visibility = Visibility.Visible;
                        MininumState.Visibility = Visibility.Collapsed;
                        MaxIndexRealProbability.Text = (Math.Round((double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100, 2)).ToString() + "%";
                        MinimumText.Text = (10 - GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes).ToString() + "次";
                        MaxIndexRealProbability.Foreground = Brushes.Black;
                        Log.Information("最大因子学生变更: {OldName} -> {NewName}，因子: {Factor}",
                            GlobalVariables.json.OldMemoryFactorModeSettings.MaxName,
                            thisModeJson[MaxIndex].Name,
                            thisModeJson[MaxIndex].Factor);
                    }

                    if (mininum)
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
                    GlobalVariables.json.OldMemoryFactorModeSettings.LastName = FinishNumberText.Text;
                    NowNumberText.Text = NowNumberTextValue;
                    Count.Text = Trainings.Count.ToString();

                    Log.Information("旧记忆因子抽取完成: {Name}, 因子总数: {TrainingCount}, 最大因子学生: {MaxName} ({MaxFactor})",
                        NowNumberTextValue, Trainings.Count, thisModeJson[MaxIndex].Name, thisModeJson[MaxIndex].Factor);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "旧记忆因子抽取过程中发生异常");
                StartButton.IsEnabled = true;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool newValue = SpeechButton.IsChecked.Value;
            Log.Debug("语音播报开关: {Value}", newValue);
            GlobalVariables.json.OldMemoryFactorModeSettings.Speech = newValue;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Warning("开始重置旧记忆因子数据");
                Directory.CreateDirectory(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups"));
                File.Copy(Path.Combine(GlobalVariables.configDir, "Mode_data", "OldMemoryFactoryMode", "Memory.json"), Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups",
                    DateTime.Now.ToString("yyyy_MM_d_HH_MM_ss")));
                File.Delete(Path.Combine(GlobalVariables.configDir, "Mode_data", "OldMemoryFactoryMode", "Memory.json"));
                GlobalVariables.json.OldMemoryFactorModeSettings.MaxName = null;
                GlobalVariables.json.OldMemoryFactorModeSettings.MaxTimes = 1;
                StartButton.IsEnabled = false;
                ResetButton.IsEnabled = false;
                MessageBoxFunction.ShowMessageBoxInfo("重置成功，请重新打开界面");
                Log.Information("旧记忆因子数据重置成功");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "重置旧记忆因子数据失败");
                MessageBoxFunction.ShowMessageBoxError("出错" + ex.Message);
                return;
            }
        }
    }
}
