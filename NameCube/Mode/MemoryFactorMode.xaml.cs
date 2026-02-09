using Masuit.Tools;
using Masuit.Tools.Logging;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NameCube.Function;
using System.Configuration.Internal;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;

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
        public class otherJson
        {
            public string MaxName { get; set; }
            public int MaxTimes { get; set; }
            public bool DeterminedByFate { get; set; } = false;
        }
        public class MemoryFactorModeSettingsJson
        {
            public otherJson otherSettings { get; set; } = new otherJson();
            public ObservableCollection<ThisModeJson> thisModeJson { get; set; } =
            new ObservableCollection<ThisModeJson>();
        }
        /// <summary>
        /// 本模式的局部设置（旧）
        /// </summary>
        public ObservableCollection<ThisModeJson> thisModeJson { get; set; } =
            new ObservableCollection<ThisModeJson>();
        public MemoryFactorModeSettingsJson memoryFactorModeSettingsJson = new MemoryFactorModeSettingsJson();
        public System.Timers.Timer timer = new System.Timers.Timer();
        public SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        public bool IsCanStop = false;
        public List<string> Trainings { get; set; } = new List<string>();
        public List<int> incident = new List<int>();

        public MemoryFactorMode()
        {
            InitializeComponent();
            DataContext = this;

            if (GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening == null || GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening.Count < 10)
            {
                Log.Debug("初始化事件概率分布");
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening = new List<int> { 4, 2, 3, 4, 2, 2, 3, 1, 1, 2 };
            }

            // 填充事件列表
            for (int i = 0; i <= 9; i++)
            {
                for (int ii = 1; ii <= GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[i]; ii++)
                {
                    incident.Add(i);
                }
            }

            Log.Debug("MemoryFactorMode页面初始化完成，事件总数: {IncidentCount}", incident.Count);
        }

        private void StartLoad()
        {
            try
            {
                Log.Debug("开始加载记忆因子数据");
                string FilePath = System.IO.Path.Combine(
                    GlobalVariablesData.userDataDir,
                    "Mode_data",
                    "MemoryFactoryMode"
                );

                if (!File.Exists(Path.Combine(FilePath, "Memory.json")))
                {
                    if (GlobalVariablesData.config.AllSettings.Name.Count <= 9)
                    {
                        Log.Warning("学生名单数量不足: {Count} <= 9", GlobalVariablesData.config.AllSettings.Name.Count);
                        this.Dispatcher.Invoke(
                            new Action(() =>
                            {
                                StartButton.IsEnabled = false;
                                ResetButton.IsEnabled = false;
                            })
                        );
                        SnackBarFunction.ShowSnackBarInMainWindow(
                            "我们，拒 绝 签 字\n翻译：学生名单少于10位，无法初始化",
                            Wpf.Ui.Controls.ControlAppearance.Caution
                        );
                        return;
                    }

                    Log.Information("初始化新的记忆因子数据，学生数量: {Count}", GlobalVariablesData.config.AllSettings.Name.Count);

                    thisModeJson.Clear();
                    foreach (var name in GlobalVariablesData.config.AllSettings.Name)
                    {
                        thisModeJson.Add(new ThisModeJson { Name = name, Factor = 1 });
                    }
                    SaveThisJson();
                }
                else
                {
                    Log.Debug("加载现有记忆因子数据");
                    string jsonString = File.ReadAllText(Path.Combine(FilePath, "Memory.json"));
                    try
                    {
                        memoryFactorModeSettingsJson = JsonConvert.DeserializeObject<MemoryFactorModeSettingsJson>(jsonString);
                        Log.Debug("使用新版JSON格式加载");
                    }
                    catch
                    {
                        try
                        {
                            // 针对旧版本的兼容处理
                            memoryFactorModeSettingsJson.thisModeJson = JsonConvert.DeserializeObject<ObservableCollection<ThisModeJson>>(jsonString);
                            memoryFactorModeSettingsJson.otherSettings = new otherJson();
                            Log.Debug("使用旧版JSON格式加载，进行兼容处理");
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "配置文件损坏，无法加载");
                            MessageBoxFunction.ShowMessageBoxError("配置文件损坏，无法加载，请删除后重试。\n错误信息：" + ex.Message);
                        }
                    }

                    this.Dispatcher.Invoke(() =>
                    {
                        thisModeJson.Clear();
                        foreach (var item in memoryFactorModeSettingsJson.thisModeJson)
                        {
                            thisModeJson.Add(item);
                        }
                    });
                }

                int max = -1,
                    MaxIndex = -1;
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

                if (MaxIndex != -1 && memoryFactorModeSettingsJson.otherSettings.MaxName == null)
                {
                    memoryFactorModeSettingsJson.otherSettings.MaxName = thisModeJson[MaxIndex].Name;
                    memoryFactorModeSettingsJson.otherSettings.MaxTimes = 1;
                    Log.Debug("设置最大因子学生: {Name}, 因子: {Factor}",
                        thisModeJson[MaxIndex].Name,
                        thisModeJson[MaxIndex].Factor);
                }

                this.Dispatcher.Invoke(() =>
                {
                    Count.Text = Trainings.Count.ToString();
                    if (memoryFactorModeSettingsJson.otherSettings.MaxTimes < 10)
                    {
                        MaxIndexRealProbability.Text =
                            (
                                Math.Round(
                                    (double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100,
                                    2
                                )
                            ).ToString() + "%";
                        NowMaxTimesState.Text = "等待保底";
                        Log.Debug("保底状态: 等待保底，最大因子学生: {Name}, 概率: {Probability}%",
                            thisModeJson[MaxIndex].Name,
                            Math.Round((double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100, 2));
                    }
                    else if (
                        memoryFactorModeSettingsJson.otherSettings.MaxTimes >= 10
                        && memoryFactorModeSettingsJson.otherSettings.MaxTimes <= 12
                    )
                    {
                        MaxIndexRealProbability.Text =
                            ((memoryFactorModeSettingsJson.otherSettings.MaxTimes - 9) * 25) + "%";
                        NowMaxTimesState.Text = "保底中";
                        Log.Debug("保底状态: 保底中，概率: {Probability}%", (memoryFactorModeSettingsJson.otherSettings.MaxTimes - 9) * 25);
                    }
                    else
                    {
                        MaxIndexRealProbability.Text = "100%";
                        NowMaxTimesState.Text = "保底中";
                        Log.Debug("保底状态: 强制保底，概率: 100%");
                    }
                    ShowFloor(memoryFactorModeSettingsJson.otherSettings.MaxTimes);
                });

                Log.Information("记忆因子数据加载完成，总因子数: {TrainingCount}, 最大因子: {MaxFactor} ({MaxName})",
                    Trainings.Count, max, thisModeJson[MaxIndex].Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载记忆因子数据时发生异常");
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
                this.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        NowNumberText.Text = Trainings[rnd.StrictNext(Trainings.Count)];
                    })
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MemoryFactorMode定时器处理时发生异常");
            }
        }

        public void SaveThisJson()
        {
            if (!DebugCheck.IsChecked.Value)
            {
                Log.Information("保存记忆因子设置");
                string configPath = Path.Combine(
                    GlobalVariablesData.userDataDir,
                    "Mode_data",
                    "MemoryFactoryMode",
                    "Memory.json"
                );

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(configPath));
                    memoryFactorModeSettingsJson.thisModeJson = thisModeJson;
                    string jsonString = JsonConvert.SerializeObject(memoryFactorModeSettingsJson);
                    File.WriteAllText(configPath, jsonString);
                    Log.Debug("记忆因子设置保存成功: {Path}", configPath);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "保存记忆因子设置失败");
                    MessageBoxFunction.ShowMessageBoxError($"保存配置失败: {ex.Message}");
                }
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var flicker = FindResource("flicker") as Storyboard;
                flicker.Begin();
                Log.Information("开始记忆因子抽取");
                StartButton.IsEnabled = false;
                _speechSynthesizer.SpeakAsyncCancelAll();
                FinishNumberText.Foreground = GlobalVariablesData.config.AllSettings.color;
                var jumpStoryBoard = FindResource("JumpStoryBoard") as Storyboard;

                if (StartButton.Content.ToString() == "开始")
                {
                    FinishNumberText.Visibility = Visibility.Hidden;
                    NowNumberText.Visibility = Visibility.Visible;
                    StartButton.Content = "结束";
                    timer.Interval = GlobalVariablesData.config.MemoryFactorModeSettings.Speed;
                    jumpStoryBoard.Begin();
                    timer.Start();
                    StartButton.IsEnabled = true;
                    Log.Debug("记忆因子开始轮播");
                }
                else
                {
                    StartButton.Content = "开始";
                    timer.Stop();
                    List<ThisModeJson> lastThisModeJsons = new List<ThisModeJson>();
                    for (int i = 0; i < thisModeJson.Count; i++)
                    {
                        lastThisModeJsons.Add(
                            new ThisModeJson
                            {
                                Name = thisModeJson[i].Name,
                                Factor = thisModeJson[i].Factor,
                            }
                        );
                    }
                    jumpStoryBoard.Stop();
                    jumpStoryBoard.Remove();
                    Random random = new Random();
                    IsCanStop = true;
                    string SpecialOutcome = null;
                    string NowNumberTextValue = NowNumberText.Text;
                    if (memoryFactorModeSettingsJson.otherSettings.MaxTimes >= 10)
                    {
                        int get = random.StrictNext(100);
                        if (get <= (memoryFactorModeSettingsJson.otherSettings.MaxTimes - 9) * 25)
                        {
                            NowNumberTextValue = memoryFactorModeSettingsJson.otherSettings.MaxName;
                            SpecialOutcome = "保底";
                        }
                    }
                    if (memoryFactorModeSettingsJson.otherSettings.DeterminedByFate && SpecialOutcome == null)
                    {
                        int get = random.StrictNext(7);
                        if (get == 0)
                        {
                            NowNumberTextValue = No1Name.Text;
                        }
                        else if (get == 1)
                        {
                            NowNumberTextValue = No2Name.Text;
                        }
                        else if (get == 2)
                        {
                            NowNumberTextValue = No3Name.Text;
                        }
                        else if (get == 3)
                        {
                            NowNumberTextValue = No4Name.Text;
                        }
                        else if (get == 4)
                        {
                            NowNumberTextValue = No5Name.Text;
                        }
                        else if (get == 5)
                        {
                            NowNumberTextValue = No6Name.Text;
                        }
                        SpecialOutcome = "命定";
                        memoryFactorModeSettingsJson.otherSettings.DeterminedByFate = false;
                    }
                    FinishNumberText.Text = NowNumberTextValue;
                    NowNumberText.Visibility = Visibility.Hidden;
                    FinishNumberText.Visibility = Visibility.Visible;
                    int
                        GetRandom,
                        lastfactor = -1;
                    for (int i = 0; i < thisModeJson.Count; i++)
                    {
                        GetRandom = random.StrictNext(6);
                        thisModeJson[i].Factor += GetRandom;
                        for (int j = 1; j <= GetRandom; j++)
                        {
                            Trainings.Add(thisModeJson[i].Name);
                        }
                        if (thisModeJson[i].Name == NowNumberTextValue)
                        {
                            lastfactor = thisModeJson[i].Factor;
                            thisModeJson[i].Factor = 0;
                        }
                    }

                    Trainings.RemoveAll(s => s == NowNumberTextValue);
                    //这里插入事件,以下是原来的双倍旧代码
                    //GetRandom = random.StrictNext(thisModeJson.Count);
                    //int Past = thisModeJson[GetRandom].Factor;
                    //for (int i = 1; i <= thisModeJson[GetRandom].Factor; i++)
                    //{
                    //    Trainings.Add(thisModeJson[GetRandom].Name);
                    //}
                    //thisModeJson[GetRandom].Factor *= 2;
                    //UpName.Text = thisModeJson[GetRandom].Name;
                    //UpFactor.Text = Past.ToString() + "→" + thisModeJson[GetRandom].Factor.ToString();
                    //if (thisModeJson[GetRandom].Factor - Past >= 120)
                    //{
                    //    ShowStoryBoard("TwoChangeRed");
                    //}
                    //else if (thisModeJson[GetRandom].Factor - Past >= 70)
                    //{
                    //    ShowStoryBoard("TwoChangeOrange");
                    //}
                    //UpRealProbability.Text =
                    //    (
                    //        Math.Round(
                    //            (double)thisModeJson[GetRandom].Factor / Trainings.Count * 100,
                    //            2
                    //        )
                    //    ).ToString() + "%";
                    //if (thisModeJson[GetRandom].Factor > max)
                    //{
                    //    MaxIndex = GetRandom;
                    //}
                    DoubleMorePart.Visibility = Visibility.Collapsed;
                    ExchangePart.Visibility = Visibility.Collapsed;
                    GetFactorFromOtherPart.Visibility = Visibility.Collapsed;
                    FloorAddPart.Visibility = Visibility.Collapsed;
                    SkipPart.Visibility = Visibility.Collapsed;
                    DeterminedByFatePart.Visibility = Visibility.Collapsed;
                    int getIncident = incident[random.StrictNext(incident.Count)];
                    if (getIncident == 0)
                    {
                        //二倍事件
                        int getRandomIndex = random.StrictNext(thisModeJson.Count);
                        int past = thisModeJson[getRandomIndex].Factor;
                        int newFactor = (int)(thisModeJson[getRandomIndex].Factor * 2);
                        thisModeJson[getRandomIndex].Factor = newFactor;
                        SetFactor(thisModeJson[getRandomIndex].Name, newFactor);
                        DoubleName.Text = thisModeJson[getRandomIndex].Name;
                        DoublePast.Text = past.ToString();
                        DoubleAdd.Text = "X2";
                        DoubleNow.Text = newFactor.ToString();
                        incidentName.Text = "二倍事件";
                        DoubleMorePart.Visibility = Visibility.Visible;
                    }
                    else if (getIncident == 1)
                    {
                        //三倍事件
                        int getRandomIndex = random.StrictNext(thisModeJson.Count);
                        int past = thisModeJson[getRandomIndex].Factor;
                        int newFactor = (int)(thisModeJson[getRandomIndex].Factor * 3);
                        thisModeJson[getRandomIndex].Factor = newFactor;
                        SetFactor(thisModeJson[getRandomIndex].Name, newFactor);
                        DoubleName.Text = thisModeJson[getRandomIndex].Name;
                        DoublePast.Text = past.ToString();
                        DoubleAdd.Text = "X3";
                        DoubleNow.Text = newFactor.ToString();
                        incidentName.Text = "三倍事件";
                        DoubleMorePart.Visibility = Visibility.Visible;
                    }
                    else if (getIncident == 2)
                    {
                        //减半事件
                        int getRandomIndex = random.StrictNext(thisModeJson.Count);
                        int past = thisModeJson[getRandomIndex].Factor;
                        int newFactor = (int)(thisModeJson[getRandomIndex].Factor / 2);
                        thisModeJson[getRandomIndex].Factor = newFactor;
                        SetFactor(thisModeJson[getRandomIndex].Name, newFactor);
                        DoubleName.Text = thisModeJson[getRandomIndex].Name;
                        DoublePast.Text = past.ToString();
                        DoubleAdd.Text = "X0.5";
                        DoubleNow.Text = newFactor.ToString();
                        incidentName.Text = "减半事件";
                        DoubleMorePart.Visibility = Visibility.Visible;
                    }
                    else if (getIncident == 3)
                    {
                        //交换事件
                        int getRandomIndex1 = random.StrictNext(thisModeJson.Count);
                        int getRandomIndex2 = random.StrictNext(thisModeJson.Count);
                        string name1 = thisModeJson[getRandomIndex1].Name,
                            name2 = thisModeJson[getRandomIndex2].Name;
                        int factor1 = thisModeJson[getRandomIndex1].Factor,
                            factor2 = thisModeJson[getRandomIndex2].Factor;
                        SetFactor(name1, factor2);
                        thisModeJson[getRandomIndex1].Factor = factor2;
                        SetFactor(name2, factor1);
                        thisModeJson[getRandomIndex2].Factor = factor1;
                        ExchangeLeftName.Text = name1;
                        ExchangeLeftFactor.Text = factor1.ToString();
                        ExchangeRightName.Text = name2;
                        ExchangeRightFactor.Text = factor2.ToString();
                        incidentName.Text = "交换事件";
                        ExchangePart.Visibility = Visibility.Visible;
                    }
                    else if (getIncident == 4)
                    {
                        //复制事件
                        int getRandomIndex1 = random.StrictNext(thisModeJson.Count),
                        getRandomIndex2 = random.StrictNext(thisModeJson.Count);
                        int past = thisModeJson[getRandomIndex1].Factor;
                        int newFactor = thisModeJson[getRandomIndex2].Factor;
                        thisModeJson[getRandomIndex1].Factor = newFactor;
                        SetFactor(thisModeJson[getRandomIndex1].Name, newFactor);
                        DoubleName.Text = thisModeJson[getRandomIndex1].Name;
                        DoublePast.Text = past.ToString();
                        DoubleAdd.Text = thisModeJson[getRandomIndex2].Name;
                        DoubleNow.Text = newFactor.ToString();
                        incidentName.Text = "复制事件";
                        DoubleMorePart.Visibility = Visibility.Visible;
                    }
                    else if (getIncident == 5)
                    {
                        //窃取事件
                        int getRandomIndex1 = random.StrictNext(thisModeJson.Count),
                            getRandomIndex2 = random.StrictNext(thisModeJson.Count);
                        int getRandomFactor = random.StrictNext(thisModeJson[getRandomIndex2].Factor);
                        int pastLeft = thisModeJson[getRandomIndex1].Factor;
                        int nowRight = thisModeJson[getRandomIndex2].Factor - getRandomFactor;
                        thisModeJson[getRandomIndex1].Factor += getRandomFactor;
                        SetFactor(thisModeJson[getRandomIndex1].Name, thisModeJson[getRandomIndex1].Factor);
                        thisModeJson[getRandomIndex2].Factor = nowRight;
                        SetFactor(thisModeJson[getRandomIndex2].Name, nowRight);
                        GetFactorFromOtherName.Text = thisModeJson[getRandomIndex1].Name;
                        BeDeductedName.Text = thisModeJson[getRandomIndex2].Name;
                        BeforeGetOtherFactorFactor.Text = pastLeft.ToString();
                        ChangeFactor.Text = getRandomFactor.ToString();
                        NowBeDeductedFactor.Text = nowRight.ToString();
                        incidentName.Text = "窃取事件";
                        GetFactorFromOtherPart.Visibility = Visibility.Visible;
                    }
                    else if (getIncident == 6)
                    {
                        //保底事件，于后面完成
                    }
                    else if (getIncident == 7)
                    {
                        //跳过事件
                        int getRandomIndex = random.StrictNext(thisModeJson.Count), findName;
                        string newName = thisModeJson[getRandomIndex].Name,
                            lastName = NowNumberTextValue;
                        for (findName = 0; findName < thisModeJson.Count; findName++)
                        {
                            if (thisModeJson[findName].Name == NowNumberTextValue)
                            {
                                break;
                            }
                        }
                        thisModeJson[findName].Factor = thisModeJson[getRandomIndex].Factor;
                        thisModeJson[getRandomIndex].Factor = 0;
                        AfterSkipNowFactor.Text = thisModeJson[findName].Factor.ToString();
                        await Task.Delay(2000);
                        NowNumberTextValue = newName;
                        NowNumberText.Text = NowNumberTextValue;
                        FinishNumberText.Text = NowNumberTextValue;
                        FinishNumberText.Foreground = Brushes.Purple;
                        BeSkipedName.Text = lastName;
                        AfterNowName.Text = NowNumberTextValue;
                        SkipPart.Visibility = Visibility.Visible;
                        incidentName.Text = "跳过事件";
                    }
                    else if (getIncident == 8)
                    {
                        //平静事件
                        await Task.Delay(2000);
                        incidentName.Text = "平静无事";
                    }
                    else if (getIncident == 9)
                    {
                        DeterminedByFatePart.Visibility = Visibility.Visible;
                        memoryFactorModeSettingsJson.otherSettings.DeterminedByFate = true;
                        incidentName.Text = "命定事件";
                    }
                    int Max = -1,
                        MaxIndex = -1;
                    foreach (var item in thisModeJson)
                    {
                        if (item.Factor > Max)
                        {
                            Max = item.Factor;
                            MaxIndex = thisModeJson.IndexOf(item);
                        }
                    }
                    if (GlobalVariablesData.config.MemoryFactorModeSettings.Speech)
                    {
                        _speechSynthesizer.SpeakAsync(NowNumberTextValue);
                    }
                    if (MaxIndex != -1)
                    {
                        MaxIndexRealProbability.Text =
                            (
                                Math.Round(
                                    (double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100,
                                    2
                                )
                            ).ToString() + "%";
                    }
                    if (
                        thisModeJson[MaxIndex].Name
                        == memoryFactorModeSettingsJson.otherSettings.MaxName
                    )
                    {
                        memoryFactorModeSettingsJson.otherSettings.MaxTimes++;
                        if (memoryFactorModeSettingsJson.otherSettings.MaxTimes < 10)
                        {

                            MaxIndexRealProbability.Text =
                                (
                                    Math.Round(
                                        (double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100,
                                        2
                                    )
                                ).ToString() + "%";
                            NowMaxTimesState.Text = "等待保底";
                        }
                        else if (
                            memoryFactorModeSettingsJson.otherSettings.MaxTimes >= 10
                            && memoryFactorModeSettingsJson.otherSettings.MaxTimes <= 12
                        )
                        {
                            MaxIndexRealProbability.Text =
                                ((memoryFactorModeSettingsJson.otherSettings.MaxTimes - 9) * 25)
                                + "%";
                            ShowStoryBoard("ChangeYellow");
                            NowMaxTimesState.Text = "保底中";
                        }
                        else
                        {
                            MaxIndexRealProbability.Text = "100%";
                            ShowStoryBoard("ChangeRed");
                            NowMaxTimesState.Text = "保底中";
                        }
                    }
                    else
                    {
                        memoryFactorModeSettingsJson.otherSettings.MaxName = thisModeJson[
                            MaxIndex
                        ].Name;
                        memoryFactorModeSettingsJson.otherSettings.MaxTimes /=2;
                        if(memoryFactorModeSettingsJson.otherSettings.MaxTimes<1)
                        {
                            memoryFactorModeSettingsJson.otherSettings.MaxTimes = 1;
                        }
                        MaxIndexRealProbability.Text =
                            (
                                Math.Round(
                                    (double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100,
                                    2
                                )
                            ).ToString() + "%";

                        ShowStoryBoard("Changegreen");
                        NowMaxTimesState.Text = "等待保底";
                    }
                    if (SpecialOutcome != null)
                    {
                        LastFactorText.Text = SpecialOutcome;
                    }
                    else
                    {
                        LastFactorText.Text = lastfactor.ToString();
                    }
                    //此处完成部分事件
                    if (getIncident == 6)
                    {
                        //保底事件
                        int getRandomAdd = random.StrictNext(3) + 1;
                        memoryFactorModeSettingsJson.otherSettings.MaxTimes += getRandomAdd;
                        FloorAdd.Text = getRandomAdd.ToString();
                        incidentName.Text = "保底事件";
                        FloorAddPart.Visibility = Visibility.Visible;
                        //重复上述对maxtimes进行的变化
                        if (memoryFactorModeSettingsJson.otherSettings.MaxTimes < 10)
                        {

                            MaxIndexRealProbability.Text =
                                (
                                    Math.Round(
                                        (double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100,
                                        2
                                    )
                                ).ToString() + "%";
                            NowMaxTimesState.Text = "等待保底";
                        }
                        else if (
                            memoryFactorModeSettingsJson.otherSettings.MaxTimes >= 10
                            && memoryFactorModeSettingsJson.otherSettings.MaxTimes <= 12
                        )
                        {
                            MaxIndexRealProbability.Text =
                                ((memoryFactorModeSettingsJson.otherSettings.MaxTimes - 9) * 25)
                                + "%";
                            ShowStoryBoard("ChangeYellow");
                            NowMaxTimesState.Text = "保底中";
                        }
                        else
                        {
                            MaxIndexRealProbability.Text = "100%";
                            ShowStoryBoard("ChangeRed");
                            NowMaxTimesState.Text = "保底中";
                        }
                    }
                    // 在关键位置添加日志
                    Log.Information("记忆因子抽取完成，结果: {Result}", NowNumberTextValue);

                    if (SpecialOutcome != null)
                    {
                        Log.Debug("特殊结果: {Outcome}", SpecialOutcome);
                    }

                    Log.Information("事件触发: {IncidentName}", incidentName.Text);

                    SaveThisJson();
                    if (!DebugCheck.IsChecked.Value)
                    {
                        GlobalVariablesData.SaveConfig();
                    }

                    ShowFloor(memoryFactorModeSettingsJson.otherSettings.MaxTimes);
                    IsCanStop = false;
                    if (!DebugCheck.IsChecked.Value)
                    {
                        GlobalVariablesData.config.MemoryFactorModeSettings.LastName = FinishNumberText.Text;
                    }

                    NowNumberText.Text = NowNumberTextValue;
                    Count.Text = Trainings.Count.ToString();
                    ChangeTheName(lastThisModeJsons);

                    Log.Information("记忆因子抽取处理完成，因子总数: {TrainingCount}", Trainings.Count);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "记忆因子抽取过程中发生异常");
                StartButton.IsEnabled = true;
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool newValue = SpeechButton.IsChecked.Value;
            Log.Debug("语音播报开关: {Value}", newValue);
            GlobalVariablesData.config.MemoryFactorModeSettings.Speech = newValue;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Warning("开始重置记忆因子数据");
                Directory.CreateDirectory(
                    Path.Combine(
                        GlobalVariablesData.userDataDir,
                        "Mode_data",
                        "MemoryFactoryMode",
                        "Backups"
                    )
                );
                File.Copy(
                    Path.Combine(
                        GlobalVariablesData.userDataDir,
                        "Mode_data",
                        "MemoryFactoryMode",
                        "Memory.json"
                    ),
                    Path.Combine(
                        GlobalVariablesData.userDataDir,
                        "Mode_data",
                        "MemoryFactoryMode",
                        "Backups",
                        DateTime.Now.ToString("yyyy_MM_d_HH_MM_ss")
                    )
                );
                File.Delete(
                    Path.Combine(
                        GlobalVariablesData.userDataDir,
                        "Mode_data",
                        "MemoryFactoryMode",
                        "Memory.json"
                    )
                );
                memoryFactorModeSettingsJson.otherSettings.MaxName = null;
                memoryFactorModeSettingsJson.otherSettings.MaxTimes = 1;
                StartButton.IsEnabled = false;
                ResetButton.IsEnabled = false;
                SnackBarFunction.ShowSnackBarInMainWindow(
                    "重置成功，请重新打开界面",
                    Wpf.Ui.Controls.ControlAppearance.Primary
                );
                Log.Information("记忆因子数据重置成功");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "重置记忆因子数据失败");
                MessageBoxFunction.ShowMessageBoxError("出错" + ex.Message);
                return;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("MemoryFactorMode页面加载");

                if (!GlobalVariablesData.config.MemoryFactorModeSettings.debug)
                {
                    QuitDebug.Visibility = Visibility.Collapsed;
                    GetTraning.Visibility = Visibility.Collapsed;
                    DebugCheck.Visibility = Visibility.Collapsed;
                    Log.Debug("调试功能已隐藏");
                }

                if (!GlobalVariablesData.config.AllSettings.SystemSpeech)
                {
                    _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                    _speechSynthesizer.Rate = GlobalVariablesData.config.AllSettings.Speed;
                    _speechSynthesizer.Volume = GlobalVariablesData.config.AllSettings.Volume;
                    Log.Debug("配置语音合成器: 性别=Female, 音量={Volume}, 语速={Speed}",
                        GlobalVariablesData.config.AllSettings.Volume,
                        GlobalVariablesData.config.AllSettings.Speed);
                }

                timer.Elapsed += Timer_Elapsed;
                SpeechButton.IsChecked = GlobalVariablesData.config.MemoryFactorModeSettings.Speech;
                SpeechButton.IsEnabled = !GlobalVariablesData.config.MemoryFactorModeSettings.Locked;
                ResetButton.IsEnabled = !GlobalVariablesData.config.MemoryFactorModeSettings.Locked;

                string FilePath = System.IO.Path.Combine(
                    GlobalVariablesData.userDataDir,
                    "Mode_data",
                    "MemoryFactoryMode"
                );
                Directory.CreateDirectory(FilePath);

                if (GlobalVariablesData.config.MemoryFactorModeSettings.Speed == 0)
                {
                    Log.Debug("重置速度为默认值20");
                    GlobalVariablesData.config.MemoryFactorModeSettings.Speed = 20;
                }

                StartLoad();
                NowNumberText.Foreground = GlobalVariablesData.config.AllSettings.color;
                FinishNumberText.Foreground = GlobalVariablesData.config.AllSettings.color;
                //NowNumberText.FontFamily = GlobalVariablesData.config.AllSettings.Font;
                //FinishNumberText.FontFamily = GlobalVariablesData.config.AllSettings.Font;

                if (GlobalVariablesData.config.MemoryFactorModeSettings.LastName != null)
                {
                    NowNumberText.Text = GlobalVariablesData.config.MemoryFactorModeSettings.LastName;
                    Log.Debug("设置上次抽取结果: {LastName}", GlobalVariablesData.config.MemoryFactorModeSettings.LastName);
                }

                ChangeTheName(null);
                ShowStoryBoard("FlickerFloorRec");
                ShowStoryBoard("ExchangeStoryBoard");
                ShowStoryBoard("GetFromOtherStoryBoard");

                Log.Information("MemoryFactorMode页面加载完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MemoryFactorMode页面加载时发生异常");
            }
        }


        private int HaveTheSameName(string name, List<ThisModeJson> thisModeJsona)
        {
            for (int i = 0; i < thisModeJsona.Count && i <= 5; i++)
            {
                if (thisModeJsona[i].Name == name)
                {
                    return i;
                }
            }
            return 6;
        }

        // 修改 CreateDoubleAnimation 方法的参数类型，将 Shape target 改为 DependencyObject target
        private void CreateDoubleAnimation(
            Storyboard storyboard,
            DependencyObject target,
            DependencyProperty property,
            double from,
            double to,
            double duration
        )
        {
            // 创建DoubleAnimation
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = from;
            animation.To = to;
            animation.Duration = new Duration(System.TimeSpan.FromSeconds(duration));

            // 设置动画的缓动函数，使运动更自然
            animation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

            // 将动画添加到Storyboard，并指定目标控件和属性
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));

            // 将动画添加到Storyboard
            storyboard.Children.Add(animation);
        }

        private Dictionary<TextBlock, double> originalTops = new Dictionary<TextBlock, double>();
        private List<TextBlock> addedTextBlocks = new List<TextBlock>();

        private void ChangeTheName(List<ThisModeJson> lastThisModeJsons)
        {
            StoreOriginalPositions();
            List<ThisModeJson> sortedList = new List<ThisModeJson>();
            foreach (ThisModeJson thisModeJson in thisModeJson)
            {
                sortedList.Add(thisModeJson);
            }
            System.Windows.Media.Animation.Storyboard storyboard = new Storyboard();
            sortedList.Sort((a, b) => b.Factor.CompareTo(a.Factor));
            List<double> NameTopList = new List<double>() { 32, 82, 132, 182, 232, 282, 325.96 };
            List<double> FactorTopList = new List<double>() { 44, 94, 144, 194, 244, 294, 337.96 };
            bool[] isFill = new bool[6];
            isFill[0] = false;
            isFill[1] = false;
            isFill[2] = false;
            isFill[3] = false;
            isFill[4] = false;
            isFill[5] = false;
            const double MOVETIME = 1;
            int get = HaveTheSameName(No1Name.Text, sortedList);
            CreateDoubleAnimation(
                storyboard,
                No1Name,
                Canvas.TopProperty,
                Canvas.GetTop(No1Name),
                NameTopList[get],
                MOVETIME
            );
            CreateDoubleAnimation(
                storyboard,
                No1Factor,
                Canvas.TopProperty,
                Canvas.GetTop(No1Factor),
                FactorTopList[get],
                MOVETIME
            );
            if (get < 6)
            {
                isFill[get] = true;
            }
            get = HaveTheSameName(No2Name.Text, sortedList);
            CreateDoubleAnimation(
                storyboard,
                No2Name,
                Canvas.TopProperty,
                Canvas.GetTop(No2Name),
                NameTopList[get],
                MOVETIME
            );
            CreateDoubleAnimation(
                storyboard,
                No2Factor,
                Canvas.TopProperty,
                Canvas.GetTop(No2Factor),
                FactorTopList[get],
                MOVETIME
            );
            if (get < 6)
            {
                isFill[get] = true;
            }
            get = HaveTheSameName(No3Name.Text, sortedList);
            CreateDoubleAnimation(
                storyboard,
                No3Name,
                Canvas.TopProperty,
                Canvas.GetTop(No3Name),
                NameTopList[get],
                MOVETIME
            );
            CreateDoubleAnimation(
                storyboard,
                No3Factor,
                Canvas.TopProperty,
                Canvas.GetTop(No3Factor),
                FactorTopList[get],
                MOVETIME
            );
            if (get < 6)
            {
                isFill[get] = true;
            }
            get = HaveTheSameName(No4Name.Text, sortedList);
            CreateDoubleAnimation(
                storyboard,
                No4Name,
                Canvas.TopProperty,
                Canvas.GetTop(No4Name),
                NameTopList[get],
                MOVETIME
            );
            CreateDoubleAnimation(
                storyboard,
                No4Factor,
                Canvas.TopProperty,
                Canvas.GetTop(No4Factor),
                FactorTopList[get],
                MOVETIME
            );
            if (get < 6)
            {
                isFill[get] = true;
            }
            get = HaveTheSameName(No5Name.Text, sortedList);
            CreateDoubleAnimation(
                storyboard,
                No5Name,
                Canvas.TopProperty,
                Canvas.GetTop(No5Name),
                NameTopList[get],
                MOVETIME
            );
            CreateDoubleAnimation(
                storyboard,
                No5Factor,
                Canvas.TopProperty,
                Canvas.GetTop(No5Factor),
                FactorTopList[get],
                MOVETIME
            );
            if (get < 6)
            {
                isFill[get] = true;
            }
            get = HaveTheSameName(No6Name.Text, sortedList);
            CreateDoubleAnimation(
                storyboard,
                No6Name,
                Canvas.TopProperty,
                Canvas.GetTop(No6Name),
                NameTopList[get],
                MOVETIME
            );
            CreateDoubleAnimation(
                storyboard,
                No6Factor,
                Canvas.TopProperty,
                Canvas.GetTop(No6Factor),
                FactorTopList[get],
                MOVETIME
            );
            if (get < 6)
            {
                isFill[get] = true;
            }
            for (int i = 0; i < 6; i++)
            {
                if (!isFill[i])
                {
                    TextBlock newName = new TextBlock()
                    {
                        FontSize = 36,
                        Text = sortedList[i].Name,
                    };
                    TextBlock newFactor = new TextBlock()
                    {
                        FontSize = 24,
                        Text = sortedList[i].Factor.ToString(),
                    };
                    canvas.Children.Add(newName);
                    canvas.Children.Add(newFactor);
                    addedTextBlocks.Add(newName);
                    addedTextBlocks.Add(newFactor);
                    Canvas.SetLeft(newName, 65.17);
                    Canvas.SetTop(newName, 325.96);
                    Canvas.SetLeft(newFactor, 211.41);
                    Canvas.SetTop(newFactor, 337.96);
                    CreateDoubleAnimation(
                        storyboard,
                        newName,
                        Canvas.TopProperty,
                        325.96,
                        NameTopList[i],
                        MOVETIME
                    );
                    CreateDoubleAnimation(
                        storyboard,
                        newFactor,
                        Canvas.TopProperty,
                        337.96,
                        FactorTopList[i],
                        MOVETIME
                    );
                }
            }
            storyboard.Completed += (s, en) =>
            {
                storyboard.Stop();
                storyboard.Remove();
                RevertChanges(sortedList, lastThisModeJsons);
                StartButton.IsEnabled = true;
            };
            storyboard.Begin();
        }

        private void RevertChanges(
            List<ThisModeJson> thisModeJsons,
            List<ThisModeJson> lastThisModeJson
        )
        {
            // 立即停止所有动画

            // 还原所有原始TextBlock的位置到初始值
            Canvas.SetTop(No1Name, originalTops[No1Name]);
            Canvas.SetTop(No1Factor, originalTops[No1Factor]);
            Canvas.SetTop(No2Name, originalTops[No2Name]);
            Canvas.SetTop(No2Factor, originalTops[No2Factor]);
            Canvas.SetTop(No3Name, originalTops[No3Name]);
            Canvas.SetTop(No3Factor, originalTops[No3Factor]);
            Canvas.SetTop(No4Name, originalTops[No4Name]);
            Canvas.SetTop(No4Factor, originalTops[No4Factor]);
            Canvas.SetTop(No5Name, originalTops[No5Name]);
            Canvas.SetTop(No5Factor, originalTops[No5Factor]);
            Canvas.SetTop(No6Name, originalTops[No6Name]);
            Canvas.SetTop(No6Factor, originalTops[No6Factor]);
            No1Name.Text = thisModeJsons[0].Name;
            No2Name.Text = thisModeJsons[1].Name;
            No3Name.Text = thisModeJsons[2].Name;
            No4Name.Text = thisModeJsons[3].Name;
            No5Name.Text = thisModeJsons[4].Name;
            No6Name.Text = thisModeJsons[5].Name;
            No1Factor.Text = thisModeJsons[0].Factor.ToString();
            No2Factor.Text = thisModeJsons[1].Factor.ToString();
            No3Factor.Text = thisModeJsons[2].Factor.ToString();
            No4Factor.Text = thisModeJsons[3].Factor.ToString();
            No5Factor.Text = thisModeJsons[4].Factor.ToString();
            No6Factor.Text = thisModeJsons[5].Factor.ToString();
            // 删除所有新增的TextBlock
            foreach (var textBlock in addedTextBlocks)
            {
                canvas.Children.Remove(textBlock);
            }
            addedTextBlocks.Clear();
            if (lastThisModeJson != null)
            {
                //if(No1Factor.Text.ToInt32(-1)>findLastFactor(No1Name.Text,lastThisModeJson))
                //{
                //    ShowStoryBoard("No1Red");
                //}
                int no1Change =
                        No1Factor.Text.ToInt32(-1) - findLastFactor(No1Name.Text, lastThisModeJson),
                    no2Change =
                        No2Factor.Text.ToInt32(-1) - findLastFactor(No2Name.Text, lastThisModeJson),
                    no3Change =
                        No3Factor.Text.ToInt32(-1) - findLastFactor(No3Name.Text, lastThisModeJson),
                    no4Change =
                        No4Factor.Text.ToInt32(-1) - findLastFactor(No4Name.Text, lastThisModeJson),
                    no5Change =
                        No5Factor.Text.ToInt32(-1) - findLastFactor(No5Name.Text, lastThisModeJson),
                    no6Change =
                        No6Factor.Text.ToInt32(-1) - findLastFactor(No6Name.Text, lastThisModeJson);
                if (no1Change > 0)
                {
                    AddText1.Text = "+" + no1Change;
                    AddText1.Foreground = Brushes.Red;
                }
                else
                {
                    AddText1.Text = no1Change.ToString();
                    AddText1.Foreground = Brushes.Blue;
                }
                if (no2Change > 0)
                {
                    AddText2.Text = "+" + no2Change;
                    AddText2.Foreground = Brushes.Red;
                }
                else
                {
                    AddText2.Text = no2Change.ToString();
                    AddText2.Foreground = Brushes.Blue;
                }
                if (no3Change > 0)
                {
                    AddText3.Text = "+" + no3Change;
                    AddText3.Foreground = Brushes.Red;
                }
                else
                {
                    AddText3.Text = no3Change.ToString();
                    AddText3.Foreground = Brushes.Blue;
                }
                if (no4Change > 0)
                {
                    AddText4.Text = "+" + no4Change;
                    AddText4.Foreground = Brushes.Red;
                }
                else
                {
                    AddText4.Text = no4Change.ToString();
                    AddText4.Foreground = Brushes.Blue;
                }
                if (no5Change > 0)
                {
                    AddText5.Text = "+" + no5Change;
                    AddText5.Foreground = Brushes.Red;
                }
                else
                {
                    AddText5.Text = no5Change.ToString();
                    AddText5.Foreground = Brushes.Blue;
                }
                if (no6Change > 0)
                {
                    AddText6.Text = "+" + no6Change;
                    AddText6.Foreground = Brushes.Red;
                }
                else
                {
                    AddText6.Text = no6Change.ToString();
                    AddText6.Foreground = Brushes.Blue;
                }
                var addstoryboard = FindResource("NewAddStoryBoard") as Storyboard;
                addstoryboard.Begin();
            }
        }

        private void StoreOriginalPositions()
        {
            originalTops[No1Name] = Canvas.GetTop(No1Name);
            originalTops[No1Factor] = Canvas.GetTop(No1Factor);
            originalTops[No2Name] = Canvas.GetTop(No2Name);
            originalTops[No2Factor] = Canvas.GetTop(No2Factor);
            originalTops[No3Name] = Canvas.GetTop(No3Name);
            originalTops[No3Factor] = Canvas.GetTop(No3Factor);
            originalTops[No4Name] = Canvas.GetTop(No4Name);
            originalTops[No4Factor] = Canvas.GetTop(No4Factor);
            originalTops[No5Name] = Canvas.GetTop(No5Name);
            originalTops[No5Factor] = Canvas.GetTop(No5Factor);
            originalTops[No6Name] = Canvas.GetTop(No6Name);
            originalTops[No6Factor] = Canvas.GetTop(No6Factor);
        }

        private void ShowStoryBoard(string resource)
        {
            Storyboard storyboard = FindResource(resource) as Storyboard;
            storyboard.Begin();
        }

        private int findLastFactor(string name, List<ThisModeJson> thisModeJsons)
        {
            for (int i = 0; i < thisModeJsons.Count; i++)
            {
                if (thisModeJson[i].Name == name)
                {
                    return thisModeJsons[i].Factor;
                }
            }
            return -1;
        }
        private void ShowFloor(int index)
        {
            if (index <= 9)
            {
                GuaranteedProgressBar.Value = index;
                RemainingGuaranteedAttempts.Text = (10 - index).ToString();
                NotGuaranteed.Visibility = Visibility.Visible;
                Guaranteed.Visibility = Visibility.Collapsed;
            }
            else
            {
                Guaranteed.Visibility = Visibility.Visible;
                NotGuaranteed.Visibility = Visibility.Collapsed;
            }
        }
        private void SetFactor(string name, int newFactor)
        {
            Trainings.RemoveAll(s => s == name);
            for (int j = 1; j <= newFactor; j++)
            {
                Trainings.Add(name);
            }
        }

        private void QuitDebug_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("退出调试模式");
            GlobalVariablesData.config.MemoryFactorModeSettings.debug = false;
            if (!DebugCheck.IsChecked.Value)
            {
                GlobalVariablesData.SaveConfig();
            }
            QuitDebug.Visibility = Visibility.Collapsed;
            GetTraning.Visibility = Visibility.Collapsed;
            DebugCheck.Visibility = Visibility.Collapsed;
        }

        private void GetTraning_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("复制因子列表到剪贴板，数量: {Count}", Trainings.Count);
                string text = "";
                for (int i = 0; i < Trainings.Count; i++)
                {
                    text += Trainings[i] + "\r\n";
                }
                System.Windows.Clipboard.SetText(text);
                SnackBarFunction.ShowSnackBarInMainWindow("已复制到剪贴板", Wpf.Ui.Controls.ControlAppearance.Primary);
                Log.Information("因子列表已复制到剪贴板");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "复制因子列表失败");
            }
        }
    }
}
