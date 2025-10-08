using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Masuit.Tools;
using Masuit.Tools.Logging;
using Newtonsoft.Json;
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

        /// <summary>
        /// 本模式的局部设置
        /// </summary>
        public ObservableCollection<ThisModeJson> thisModeJson { get; set; } =
            new ObservableCollection<ThisModeJson>();
        public System.Timers.Timer timer = new System.Timers.Timer();
        public SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        public bool IsCanStop = false;
        public List<string> Trainings { get; set; } = new List<string>();

        public MemoryFactorMode()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void StartLoad()
        {
            string FilePath = System.IO.Path.Combine(
                GlobalVariables.configDir,
                "Mode_data",
                "MemoryFactoryMode"
            );
            if (!File.Exists(Path.Combine(FilePath, "Memory.json")))
            {
                if (GlobalVariables.json.AllSettings.Name.Count <= 9)
                {
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

                // 通过 Add 方法填充现有集合
                thisModeJson.Clear();
                foreach (var name in GlobalVariables.json.AllSettings.Name)
                {
                    thisModeJson.Add(new ThisModeJson { Name = name, Factor = 1 });
                }
                SaveThisJson();
            }
            else
            {
                // 反序列化后合并到现有集合
                string jsonString = File.ReadAllText(Path.Combine(FilePath, "Memory.json"));
                var loadedData = JsonConvert.DeserializeObject<ObservableCollection<ThisModeJson>>(
                    jsonString
                );
                this.Dispatcher.Invoke(() =>
                {
                    thisModeJson.Clear();
                    foreach (var item in loadedData)
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
            if (MaxIndex != -1 && GlobalVariables.json.MemoryFactorModeSettings.MaxName == null)
            {
                GlobalVariables.json.MemoryFactorModeSettings.MaxName = thisModeJson[MaxIndex].Name;
                GlobalVariables.json.MemoryFactorModeSettings.MaxTimes = 1;
            }
            this.Dispatcher.Invoke(() =>
            {
                Count.Text = Trainings.Count.ToString();
                if (GlobalVariables.json.MemoryFactorModeSettings.MaxTimes < 10)
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
                    GlobalVariables.json.MemoryFactorModeSettings.MaxTimes >= 10
                    && GlobalVariables.json.MemoryFactorModeSettings.MaxTimes <= 12
                )
                {
                    MaxIndexRealProbability.Text =
                        ((GlobalVariables.json.MemoryFactorModeSettings.MaxTimes - 9) * 25) + "%";
                    NowMaxTimesState.Text = "保底中";
                }
                else
                {
                    MaxIndexRealProbability.Text = "100%";
                    NowMaxTimesState.Text = "保底中";
                }
                ShowFloorRec(GlobalVariables.json.MemoryFactorModeSettings.MaxTimes);
            });
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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
            if (IsCanStop)
            {
                IsCanStop = false;
            }
        }

        public void SaveThisJson()
        {
            LogManager.Info("保存基因因子设置");

            string configPath = Path.Combine(
                GlobalVariables.configDir,
                "Mode_data",
                "MemoryFactoryMode",
                "Memory.json"
            );

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

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            _speechSynthesizer.SpeakAsyncCancelAll();
            FinishNumberText.Foreground = GlobalVariables.json.AllSettings.color;
            var jumpStoryBoard = FindResource("JumpStoryBoard") as Storyboard;
            if (StartButton.Content.ToString() == "开始")
            {
                FinishNumberText.Visibility = Visibility.Hidden;
                NowNumberText.Visibility = Visibility.Visible;
                StartButton.Content = "结束";
                timer.Interval = GlobalVariables.json.MemoryFactorModeSettings.Speed;
                jumpStoryBoard.Begin();
                timer.Start();
                StartButton.IsEnabled = true;
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
                bool mininum = false;
                string NowNumberTextValue = NowNumberText.Text;
                if (GlobalVariables.json.MemoryFactorModeSettings.MaxTimes >= 10)
                {
                    int get = random.StrictNext(100);
                    if (get <= (GlobalVariables.json.MemoryFactorModeSettings.MaxTimes - 9) * 25)
                    {
                        NowNumberTextValue = GlobalVariables.json.MemoryFactorModeSettings.MaxName;
                        mininum = true;
                    }
                }
                FinishNumberText.Text = NowNumberTextValue;
                NowNumberText.Visibility = Visibility.Hidden;
                FinishNumberText.Visibility = Visibility.Visible;
                int delete = 0,
                    GetRandom,
                    max = 0,
                    MaxIndex = -1,
                    lastfactor = 0;
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
                    else if (thisModeJson[i].Factor > max)
                    {
                        max = thisModeJson[i].Factor;
                        MaxIndex = i;
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
                SkipPart.Visibility= Visibility.Collapsed;
                int incident=random.StrictNext(23);
                if(incident<=4)
                {
                    //二倍事件
                    int getRandomIndex=random.StrictNext(thisModeJson.Count);
                    int past = thisModeJson[getRandomIndex].Factor;
                    int newFactor = (int)(thisModeJson[getRandomIndex].Factor * 2);
                    thisModeJson[getRandomIndex].Factor = newFactor;
                    SetFactor(thisModeJson[getRandomIndex].Name, newFactor);
                    DoubleName.Text= thisModeJson[getRandomIndex].Name;
                    DoublePast.Text= past.ToString();
                    DoubleAdd.Text = "X2";
                    DoubleNow.Text= newFactor.ToString();
                    incidentName.Text="二倍事件";
                    DoubleMorePart.Visibility = Visibility.Visible;
                }
                else if (incident>=5&&incident<=6)
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
                else if (incident >= 7 && incident <= 9)
                {
                    //减半事件
                    int getRandomIndex = random.StrictNext(thisModeJson.Count);
                    int past = thisModeJson[getRandomIndex].Factor;
                    int newFactor = (int)(thisModeJson[getRandomIndex].Factor /2);
                    thisModeJson[getRandomIndex].Factor = newFactor;
                    SetFactor(thisModeJson[getRandomIndex].Name, newFactor);
                    DoubleName.Text = thisModeJson[getRandomIndex].Name;
                    DoublePast.Text = past.ToString();
                    DoubleAdd.Text = "X0.5";
                    DoubleNow.Text = newFactor.ToString();
                    incidentName.Text = "减半事件";
                    DoubleMorePart.Visibility = Visibility.Visible;
                }
                else if(incident>=10&&incident<=13)
                {
                    //交换事件
                    int getRandomIndex1 = random.StrictNext(thisModeJson.Count);
                    int getRandomIndex2 = random.StrictNext(thisModeJson.Count);
                    string name1 = thisModeJson[getRandomIndex1].Name,
                        name2= thisModeJson[getRandomIndex2].Name;
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
                else if(incident>=14&&incident<=15)
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
                else if(incident>=16&&incident<=17)
                {
                    //窃取事件
                    int getRandomIndex1 = random.StrictNext(thisModeJson.Count),
                        getRandomIndex2 = random.StrictNext(thisModeJson.Count);
                    int getRandomFactor = random.StrictNext(thisModeJson[getRandomIndex2].Factor);
                    int pastLeft = thisModeJson[getRandomIndex1].Factor;
                    int nowRight= thisModeJson[getRandomIndex2].Factor - getRandomFactor;
                    thisModeJson[getRandomIndex1].Factor+= getRandomFactor;
                    SetFactor(thisModeJson[getRandomIndex1].Name, thisModeJson[getRandomIndex1].Factor);
                    thisModeJson[getRandomIndex2].Factor = nowRight;
                    SetFactor(thisModeJson[getRandomIndex2].Name, nowRight);
                    GetFactorFromOtherName.Text = thisModeJson[getRandomIndex1].Name;
                    BeDeductedName.Text = thisModeJson[getRandomIndex2].Name;
                    BeforeGetOtherFactorFactor.Text=pastLeft.ToString();
                    ChangeFactor.Text = getRandomFactor.ToString();
                    NowBeDeductedFactor.Text = nowRight.ToString();
                    incidentName.Text = "窃取事件";
                    GetFactorFromOtherPart.Visibility = Visibility.Visible;
                }
                else if(incident>=18&&incident<=20)
                {
                    //保底事件，于后面完成
                }
                else if(incident==21)
                {
                    //跳过事件
                    int getRandomIndex= random.StrictNext(thisModeJson.Count);
                    string newName= thisModeJson[getRandomIndex].Name,
                        lastName= NowNumberTextValue;
                    await Task.Delay(2000);
                    NowNumberTextValue= newName;
                    NowNumberText.Text= NowNumberTextValue;
                    FinishNumberText.Text= NowNumberTextValue;
                    FinishNumberText.Foreground = Brushes.Purple;
                    BeSkipedName.Text= lastName;
                    AfterNowName.Text= NowNumberTextValue;
                    SkipPart.Visibility= Visibility.Visible;
                    incidentName.Text = "跳过事件";
                }
                else if(incident==22)
                {
                    //平静事件
                    incidentName.Text = "平静无事";
                }
                if (GlobalVariables.json.MemoryFactorModeSettings.Speech)
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
                    == GlobalVariables.json.MemoryFactorModeSettings.MaxName
                )
                {
                    GlobalVariables.json.MemoryFactorModeSettings.MaxTimes++;
                    if (GlobalVariables.json.MemoryFactorModeSettings.MaxTimes < 10)
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
                        GlobalVariables.json.MemoryFactorModeSettings.MaxTimes >= 10
                        && GlobalVariables.json.MemoryFactorModeSettings.MaxTimes <= 12
                    )
                    {
                        MaxIndexRealProbability.Text =
                            ((GlobalVariables.json.MemoryFactorModeSettings.MaxTimes - 9) * 25)
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
                    GlobalVariables.json.MemoryFactorModeSettings.MaxName = thisModeJson[
                        MaxIndex
                    ].Name;
                    GlobalVariables.json.MemoryFactorModeSettings.MaxTimes = 1;
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
                if (mininum)
                {
                    LastFactorText.Text = "保底";
                }
                else
                {
                    LastFactorText.Text = lastfactor.ToString();
                }
                //此处完成部分事件
                if (incident >= 18 && incident <= 20)
                {
                    //保底事件
                    int getRandomAdd=random.StrictNext(3)+1;
                    GlobalVariables.json.MemoryFactorModeSettings.MaxTimes+=getRandomAdd;
                    FloorAdd.Text= getRandomAdd.ToString();
                    incidentName.Text = "保底事件";
                    FloorAddPart.Visibility = Visibility.Visible;
                    //重复上述对maxtimes进行的变化
                    if (GlobalVariables.json.MemoryFactorModeSettings.MaxTimes < 10)
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
                        GlobalVariables.json.MemoryFactorModeSettings.MaxTimes >= 10
                        && GlobalVariables.json.MemoryFactorModeSettings.MaxTimes <= 12
                    )
                    {
                        MaxIndexRealProbability.Text =
                            ((GlobalVariables.json.MemoryFactorModeSettings.MaxTimes - 9) * 25)
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
                SaveThisJson();
                GlobalVariables.SaveJson();
                ShowFloorRec(GlobalVariables.json.MemoryFactorModeSettings.MaxTimes);
                IsCanStop = false;
                GlobalVariables.json.MemoryFactorModeSettings.LastName = FinishNumberText.Text;
                NowNumberText.Text = NowNumberTextValue;
                Count.Text = Trainings.Count.ToString();
                ChangeTheName(lastThisModeJsons);
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
                Directory.CreateDirectory(
                    Path.Combine(
                        GlobalVariables.configDir,
                        "Mode_data",
                        "MemoryFactoryMode",
                        "Backups"
                    )
                );
                File.Copy(
                    Path.Combine(
                        GlobalVariables.configDir,
                        "Mode_data",
                        "MemoryFactoryMode",
                        "Memory.json"
                    ),
                    Path.Combine(
                        GlobalVariables.configDir,
                        "Mode_data",
                        "MemoryFactoryMode",
                        "Backups",
                        DateTime.Now.ToString("yyyy_MM_d_HH_MM_ss")
                    )
                );
                File.Delete(
                    Path.Combine(
                        GlobalVariables.configDir,
                        "Mode_data",
                        "MemoryFactoryMode",
                        "Memory.json"
                    )
                );
                GlobalVariables.json.MemoryFactorModeSettings.MaxName = null;
                GlobalVariables.json.MemoryFactorModeSettings.MaxTimes = 1;
                StartButton.IsEnabled = false;
                ResetButton.IsEnabled = false;
                SnackBarFunction.ShowSnackBarInMainWindow(
                    "重置成功，请重新打开界面",
                    Wpf.Ui.Controls.ControlAppearance.Primary
                );
            }
            catch (Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError("出错" + ex.Message);
                return;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if(!GlobalVariables.json.MemoryFactorModeSettings.debug)
            {
                QuitDebug.Visibility= Visibility.Collapsed;
                GetTraning.Visibility= Visibility.Collapsed;
            }
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
            string FilePath = System.IO.Path.Combine(
                GlobalVariables.configDir,
                "Mode_data",
                "MemoryFactoryMode"
            );
            Directory.CreateDirectory(FilePath);
            if (GlobalVariables.json.MemoryFactorModeSettings.Speed == 0)
            {
                GlobalVariables.json.MemoryFactorModeSettings.Speed = 20;
            }
            StartLoad();
            NowNumberText.Foreground = GlobalVariables.json.AllSettings.color;
            FinishNumberText.Foreground = GlobalVariables.json.AllSettings.color;
            NowNumberText.FontFamily = GlobalVariables.json.AllSettings.Font;
            FinishNumberText.FontFamily = GlobalVariables.json.AllSettings.Font;
            if (GlobalVariables.json.MemoryFactorModeSettings.LastName != null)
            {
                NowNumberText.Text = GlobalVariables.json.MemoryFactorModeSettings.LastName;
            }
            ChangeTheName(null);
            ShowStoryBoard("FlickerFloorRec");
            ShowStoryBoard("ExchangeStoryBoard");
            ShowStoryBoard("GetFromOtherStoryBoard");
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
        private void ShowFloorRec(int index)
        {
            FloorRec1.Visibility = Visibility.Hidden;
            FloorRec2.Visibility = Visibility.Hidden;
            FloorRec3.Visibility = Visibility.Hidden;
            FloorRec4.Visibility = Visibility.Hidden;
            FloorRec5.Visibility = Visibility.Hidden;
            FloorRec6.Visibility = Visibility.Hidden;
            FloorRec7.Visibility = Visibility.Hidden;
            FloorRec8.Visibility = Visibility.Hidden;
            FloorRec9.Visibility = Visibility.Hidden;
            FloorRec10.Visibility = Visibility.Hidden;
            FloorRec11.Visibility = Visibility.Hidden;
            FloorRec12.Visibility = Visibility.Hidden;
            FloorRec13.Visibility = Visibility.Hidden;
            if (index >= 1)
            {
                FloorRec1.Visibility = Visibility.Visible;
            }
            if (index >= 2)
            {
                FloorRec2.Visibility = Visibility.Visible;
            }
            if (index >= 3)
            {
                FloorRec3.Visibility = Visibility.Visible;
            }
            if (index >= 4)
            {
                FloorRec4.Visibility = Visibility.Visible;
            }
            if (index >= 5)
            {
                FloorRec5.Visibility = Visibility.Visible;
            }
            if (index >= 6)
            {
                FloorRec6.Visibility = Visibility.Visible;
            }
            if (index >= 7)
            {
                FloorRec7.Visibility = Visibility.Visible;
            }
            if (index >= 8)
            {
                FloorRec8.Visibility = Visibility.Visible;
            }
            if (index >= 9)
            {
                FloorRec9.Visibility = Visibility.Visible;
            }
            if (index >= 10)
            {
                FloorRec10.Visibility = Visibility.Visible;
            }
            if (index >= 11)
            {
                FloorRec11.Visibility = Visibility.Visible;
            }
            if (index >= 12)
            {
                FloorRec12.Visibility = Visibility.Visible;
            }
            if (index >= 13)
            {
                FloorRec13.Visibility = Visibility.Visible;
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
            GlobalVariables.json.MemoryFactorModeSettings.debug = false;
            GlobalVariables.SaveJson();
            QuitDebug.Visibility = Visibility.Collapsed;
            GetTraning.Visibility = Visibility.Collapsed;
        }

        private void GetTraning_Click(object sender, RoutedEventArgs e)
        {
            string text = "";
            for (int i = 0;i< Trainings.Count; i++)
            {
                text += Trainings[i] + "\r\n";
            }
            System.Windows.Clipboard.SetText(text);
            SnackBarFunction.ShowSnackBarInMainWindow("已复制到剪贴板",Wpf.Ui.Controls.ControlAppearance.Primary);
        }
    }
}
