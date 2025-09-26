using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
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
            int max = 0,
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
                    MinimumText.Visibility = Visibility.Visible;
                    MaxIndexRealProbability.Text =
                        (
                            Math.Round(
                                (double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100,
                                2
                            )
                        ).ToString() + "%";
                    MinimumText.Text =
                        (10 - GlobalVariables.json.MemoryFactorModeSettings.MaxTimes).ToString()
                        + "次";
                    NowMaxTimesState.Text = "等待保底";
                }
                else if (
                    GlobalVariables.json.MemoryFactorModeSettings.MaxTimes >= 10
                    && GlobalVariables.json.MemoryFactorModeSettings.MaxTimes <= 12
                )
                {
                    MinimumText.Visibility = Visibility.Collapsed;
                    MaxIndexRealProbability.Text =
                        ((GlobalVariables.json.MemoryFactorModeSettings.MaxTimes - 9) * 25) + "%";
                    NowMaxTimesState.Text = "保底中";
                }
                else
                {
                    MinimumText.Visibility = Visibility.Collapsed;
                    MaxIndexRealProbability.Text = "100%";
                    NowMaxTimesState.Text = "保底中";
                }
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

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            _speechSynthesizer.SpeakAsyncCancelAll();
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
                    lastThisModeJsons.Add(new ThisModeJson
                    {
                        Name = thisModeJson[i].Name,
                        Factor = thisModeJson[i].Factor
                    });
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
                if (GlobalVariables.json.MemoryFactorModeSettings.Speech)
                {
                    _speechSynthesizer.SpeakAsync(NowNumberTextValue);
                }
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
                GetRandom = random.StrictNext(thisModeJson.Count);
                int Past = thisModeJson[GetRandom].Factor;
                for (int i = 1; i <= thisModeJson[GetRandom].Factor; i++)
                {
                    Trainings.Add(thisModeJson[GetRandom].Name);
                }
                thisModeJson[GetRandom].Factor *= 2;
                UpName.Text = thisModeJson[GetRandom].Name;
                UpFactor.Text = Past.ToString() + "→" + thisModeJson[GetRandom].Factor.ToString();
                if (thisModeJson[GetRandom].Factor-Past>=120)
                {
                    ShowStoryBoard("TwoChangeRed");
                }
                else if(thisModeJson[GetRandom].Factor - Past >= 70)
                {
                    ShowStoryBoard("TwoChangeOrange");
                }
                    UpRealProbability.Text =
                        (
                            Math.Round(
                                (double)thisModeJson[GetRandom].Factor / Trainings.Count * 100,
                                2
                            )
                        ).ToString() + "%";
                if (thisModeJson[GetRandom].Factor > max)
                {
                    MaxIndex = GetRandom;
                }
                if (MaxIndex != -1)
                {
                    MinimumText.Visibility = Visibility.Visible;
                    MaxIndexRealProbability.Text =
                        (
                            Math.Round(
                                (double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100,
                                2
                            )
                        ).ToString() + "%";
                    MinimumText.Text =
                        (10 - GlobalVariables.json.MemoryFactorModeSettings.MaxTimes).ToString()
                        + "次";
                   

                }
                if (
                    thisModeJson[MaxIndex].Name
                    == GlobalVariables.json.MemoryFactorModeSettings.MaxName
                )
                {
                    GlobalVariables.json.MemoryFactorModeSettings.MaxTimes++;
                    if (GlobalVariables.json.MemoryFactorModeSettings.MaxTimes < 10)
                    {
                        MinimumText.Visibility = Visibility.Visible;
 
                        MaxIndexRealProbability.Text =
                            (
                                Math.Round(
                                    (double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100,
                                    2
                                )
                            ).ToString() + "%";
                        MinimumText.Text =
                            (10 - GlobalVariables.json.MemoryFactorModeSettings.MaxTimes).ToString()
                            + "次";
                        ShowStoryBoard("Changeblue");
                        NowMaxTimesState.Text = "等待保底";
                    }
                    else if (
                        GlobalVariables.json.MemoryFactorModeSettings.MaxTimes >= 10
                        && GlobalVariables.json.MemoryFactorModeSettings.MaxTimes <= 12
                    )
                    {
                        MinimumText.Visibility = Visibility.Collapsed;
                        MaxIndexRealProbability.Text =
                            ((GlobalVariables.json.MemoryFactorModeSettings.MaxTimes - 9) * 25)
                            + "%";
                        ShowStoryBoard("ChangeYellow");
                        NowMaxTimesState.Text = "保底中";
                    }
                    else
                    {
                        MinimumText.Visibility = Visibility.Collapsed;
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
                    MinimumText.Visibility = Visibility.Visible;
                    MaxIndexRealProbability.Text =
                        (
                            Math.Round(
                                (double)thisModeJson[MaxIndex].Factor / Trainings.Count * 100,
                                2
                            )
                        ).ToString() + "%";
                    MinimumText.Text =
                        (10 - GlobalVariables.json.MemoryFactorModeSettings.MaxTimes).ToString()
                        + "次";
                    ShowStoryBoard("Changegreen");
                    NowMaxTimesState.Text = "等待保底";
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
            List<double> FactorTopList = new List<double>()
            {
                44,
                94,
                144,
                194,
                244,
                294,
                337.96,
            };
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
                RevertChanges(sortedList,lastThisModeJsons);
                StartButton.IsEnabled = true;
            };
            storyboard.Begin();
        }

        private void RevertChanges(List<ThisModeJson> thisModeJsons,List<ThisModeJson> lastThisModeJson)
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
            if(lastThisModeJson!=null)
            {
                if(No1Factor.Text.ToInt32(-1)>findLastFactor(No1Name.Text,lastThisModeJson))
                {
                    ShowStoryBoard("No1Red");
                }
                if (No2Factor.Text.ToInt32(-1) > findLastFactor(No2Name.Text, lastThisModeJson))
                {
                    ShowStoryBoard("No2Red");
                }
                if (No3Factor.Text.ToInt32(-1) > findLastFactor(No3Name.Text, lastThisModeJson))
                {
                    ShowStoryBoard("No3Red");
                }
                if (No4Factor.Text.ToInt32(-1) > findLastFactor(No4Name.Text, lastThisModeJson))
                {
                    ShowStoryBoard("No4Red");
                }
                if (No5Factor.Text.ToInt32(-1) > findLastFactor(No5Name.Text, lastThisModeJson))
                {
                    ShowStoryBoard("No5Red");
                }
                if (No6Factor.Text.ToInt32(-1) > findLastFactor(No6Name.Text, lastThisModeJson))
                {
                    ShowStoryBoard("No6Red");
                }
                if (No1Factor.Text.ToInt32(-1) <= findLastFactor(No1Name.Text, lastThisModeJson))
                {
                    ShowStoryBoard("No1Blue");
                }
                if (No2Factor.Text.ToInt32(-1) <=findLastFactor(No2Name.Text, lastThisModeJson))
                {
                    ShowStoryBoard("No2Blue");
                }
                if (No3Factor.Text.ToInt32(-1) <=findLastFactor(No3Name.Text, lastThisModeJson))
                {
                    ShowStoryBoard("No3Blue");
                }
                if (No4Factor.Text.ToInt32(-1) <=findLastFactor(No4Name.Text, lastThisModeJson))
                {
                    ShowStoryBoard("No4Blue");
                }
                if (No5Factor.Text.ToInt32(-1) <=findLastFactor(No5Name.Text, lastThisModeJson))
                {
                    ShowStoryBoard("No5Blue");
                }
                if (No6Factor.Text.ToInt32(-1) <=findLastFactor(No6Name.Text, lastThisModeJson))
                {
                    ShowStoryBoard("No6Blue");
                }
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
        private int findLastFactor(string name,List<ThisModeJson> thisModeJsons)
        {           
            for(int i=0;i<thisModeJsons.Count;i++)
            {
                if (thisModeJson[i].Name==name)
                {
                    return thisModeJsons[i].Factor;
                }
            }
            return -1;
        }
    }
}
