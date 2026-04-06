/*
 * 注意！
 * 此处的代码使用了AI进行重构，部分逻辑可能存在问题，尤其是事件处理部分的细节。请务必仔细测试每个功能点，确保逻辑正确且没有遗漏。
 */
using Masuit.Tools;
using NameCube.Function;
using Newtonsoft.Json;
using Serilog;
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
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Wpf.Ui.Controls;
using TextBlock = System.Windows.Controls.TextBlock;

namespace NameCube.Mode
{
    /// <summary>
    /// 记忆因子模式页面
    /// </summary>
    public partial class MemoryFactorMode : Page, IDisposable
    {
        // ---------- 数据模型 ----------
        public class ThisModeJson : INotifyPropertyChanged
        {
            private string _name;
            public string Name
            {
                get => _name;
                set { _name = value; OnPropertyChanged(nameof(Name)); }
            }

            private int _factor;
            public int Factor
            {
                get => _factor;
                set { _factor = value; OnPropertyChanged(nameof(Factor)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class OtherSettings
        {
            public string MaxName { get; set; }
            public int MaxTimes { get; set; } = 1;
            public bool DeterminedByFate { get; set; } = false;
            public List<string> NamesInWheel { get; set; } = new List<string>();
        }

        public class MemoryFactorModeSettingsJson
        {
            public OtherSettings otherSettings { get; set; } = new OtherSettings();
            public ObservableCollection<ThisModeJson> thisModeJson { get; set; } = new ObservableCollection<ThisModeJson>();
        }

        // ---------- 私有字段 ----------
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        private readonly MemoryFactorModeSettingsJson _settings = new MemoryFactorModeSettingsJson();
        private readonly List<int> _incidentPool = new List<int>();      // 事件概率池
        private readonly Dictionary<string, int> _factorCache = new Dictionary<string, int>(); // Name -> Factor 快速查找
        private List<string> _trainings = new List<string>();           // 抽奖池
        private bool _isStopping;                                       // 停止标志
        private bool _disposed;

        // UI 绑定集合（保持与 XAML 绑定兼容）
        public ObservableCollection<ThisModeJson> thisModeJson => _settings.thisModeJson;

        // ---------- 构造函数 ----------
        public MemoryFactorMode()
        {
            InitializeComponent();
            DataContext = this;
            // 初始化新数据
            if (GlobalVariablesData.config.AllSettings.Name.Count <= 9)
            {
                Dispatcher.Invoke(() =>
                {
                    StartButton.IsEnabled = false;
                    ResetButton.IsEnabled = false;
                });
                SnackBarFunction.ShowSnackBarInMainWindow(
                    "学生名单少于10位，无法初始化",
                    ControlAppearance.Caution);
                return;
            }
            // 初始化事件概率分布
            if (GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening?.Count < 10)
            {
                Log.Debug("初始化事件概率分布");
                GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening = new List<int> { 4, 2, 3, 4, 2, 2, 3, 1, 1, 2 };
            }

            // 构建事件池
            for (int i = 0; i <= 9; i++)
            {
                int count = GlobalVariablesData.config.MemoryFactorModeSettings.probabilityOfHappening[i];
                for (int j = 0; j < count; j++)
                    _incidentPool.Add(i);
            }

            Log.Debug("MemoryFactorMode 初始化完成，事件池大小: {Count}", _incidentPool.Count);
        }

        // ---------- 页面生命周期 ----------
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 调试模式设置
                if (!GlobalVariablesData.config.MemoryFactorModeSettings.debug)
                {
                    QuitDebug.Visibility = Visibility.Collapsed;
                    GetTraning.Visibility = Visibility.Collapsed;
                    DebugCheck.Visibility = Visibility.Collapsed;
                    OpenWheel.Visibility = Visibility.Collapsed;
                }

                // 语音合成配置
                if (!GlobalVariablesData.config.AllSettings.SystemSpeech)
                {
                    _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                    _speechSynthesizer.Rate = GlobalVariablesData.config.AllSettings.Speed;
                    _speechSynthesizer.Volume = GlobalVariablesData.config.AllSettings.Volume;
                }


                if (GlobalVariablesData.config.AllSettings.Name.Count <= 9)
                {
                    return;
                }
                _timer.Elapsed += Timer_Elapsed;
                SpeechButton.IsChecked = GlobalVariablesData.config.MemoryFactorModeSettings.Speech;
                SpeechButton.IsEnabled = !GlobalVariablesData.config.MemoryFactorModeSettings.Locked;
                ResetButton.IsEnabled = !GlobalVariablesData.config.MemoryFactorModeSettings.Locked;
                SynchronizationButton.IsEnabled = !GlobalVariablesData.config.MemoryFactorModeSettings.Locked;

                // 确保目录存在
                string dir = Path.Combine(GlobalVariablesData.userDataDir, "Mode_data", "MemoryFactoryMode");
                Directory.CreateDirectory(dir);

                if (GlobalVariablesData.config.MemoryFactorModeSettings.Speed == 0)
                    GlobalVariablesData.config.MemoryFactorModeSettings.Speed = 20;

                // 加载数据
                LoadData();

                // UI 样式初始化
                NowNumberText.Foreground = FinishNumberText.Foreground = GlobalVariablesData.config.AllSettings.color;
                if (GlobalVariablesData.config.MemoryFactorModeSettings.LastName != null)
                    NowNumberText.Text = GlobalVariablesData.config.MemoryFactorModeSettings.LastName;

                // 加载排行榜动画
                ShowStoryboard("FlickerFloorRec");
                ShowStoryboard("ExchangeStoryBoard");
                ShowStoryboard("GetFromOtherStoryBoard");

                Log.Information("MemoryFactorMode 页面加载完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "页面加载异常");
            }

            // 加载排行榜动画（首次显示）
            ChangeTheName(null);
        }

        // ---------- 数据加载与持久化 ----------
        private void LoadData()
        {
            string filePath = Path.Combine(GlobalVariablesData.userDataDir, "Mode_data", "MemoryFactoryMode", "Memory.json");
            if (!File.Exists(filePath))
            {


                _settings.thisModeJson.Clear();
                foreach (var name in GlobalVariablesData.config.AllSettings.Name)
                    _settings.thisModeJson.Add(new ThisModeJson { Name = name, Factor = 1 });
                SaveSettings();
            }
            else
            {
                string json = File.ReadAllText(filePath);
                try
                {
                    // 尝试新版格式
                    var loaded = JsonConvert.DeserializeObject<MemoryFactorModeSettingsJson>(json);
                    if (loaded != null)
                    {
                        _settings.thisModeJson.Clear();
                        foreach (var item in loaded.thisModeJson)
                        {
                            _settings.thisModeJson.Add(item);
                        }
                        _settings.otherSettings = loaded.otherSettings ?? new OtherSettings();
                    }
                }
                catch
                {
                    // 兼容旧版格式
                    try
                    {
                        var oldList = JsonConvert.DeserializeObject<ObservableCollection<ThisModeJson>>(json);
                        _settings.thisModeJson.Clear();
                        if (oldList != null)
                        {
                            foreach (var item in oldList)
                                _settings.thisModeJson.Add(item);
                        }
                        _settings.otherSettings = new OtherSettings();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "配置文件损坏");
                        MessageBoxFunction.ShowMessageBoxError("配置文件损坏，请删除后重试。\n" + ex.Message);
                        return;
                    }
                }
            }

            // 同步因子缓存并重建抽奖池
            SyncFactorCache();
            RebuildTrainings();

            // 更新 UI
            Dispatcher.Invoke(() =>
            {
                Count.Text = _trainings.Count.ToString();
                GuaranteedProgressBar.Value = _settings.otherSettings.MaxTimes;
                RemainingGuaranteedAttempts.Text = (20 - _settings.otherSettings.MaxTimes).ToString();

                // 加载转盘数据
                if (_settings.otherSettings.NamesInWheel?.Any() == true)
                {
                    NavigateToWheel();
                }
            });

            // 设置最大因子学生
            if (string.IsNullOrEmpty(_settings.otherSettings.MaxName) && _settings.thisModeJson.Any())
            {
                var max = _settings.thisModeJson.OrderByDescending(x => x.Factor).First();
                _settings.otherSettings.MaxName = max.Name;
                _settings.otherSettings.MaxTimes = 1;
            }

            Log.Information("数据加载完成，抽奖池大小: {Count}，最大因子: {MaxName} ({MaxFactor})",
                _trainings.Count, _settings.otherSettings.MaxName,
                _settings.thisModeJson.FirstOrDefault(x => x.Name == _settings.otherSettings.MaxName)?.Factor ?? 0);
        }

        private void SaveSettings()
        {
            if (DebugCheck.IsChecked == true) return;

            string dir = Path.Combine(GlobalVariablesData.userDataDir, "Mode_data", "MemoryFactoryMode");
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, "Memory.json");
            string json = JsonConvert.SerializeObject(_settings);
            File.WriteAllText(file, json);
        }

        // 同步因子缓存 (Name->Factor)
        private void SyncFactorCache()
        {
            _factorCache.Clear();
            foreach (var item in _settings.thisModeJson)
                _factorCache[item.Name] = item.Factor;
        }

        // 根据当前因子重建抽奖池
        private void RebuildTrainings()
        {
            _trainings.Clear();
            foreach (var item in _settings.thisModeJson)
            {
                for (int i = 0; i < item.Factor; i++)
                    _trainings.Add(item.Name);
            }
        }

        // 更新抽奖池（增量：移除指定名字，添加指定名字多次）
        private void UpdateTrainings(string nameToRemove, int addCountForName = 0, string nameToAdd = null)
        {
            if (nameToRemove != null)
                _trainings.RemoveAll(s => s == nameToRemove);
            if (addCountForName > 0 && !string.IsNullOrEmpty(nameToAdd))
            {
                for (int i = 0; i < addCountForName; i++)
                    _trainings.Add(nameToAdd);
            }
        }

        // ---------- 核心逻辑 ----------
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isStopping) return;
            try
            {
                var rnd = new Random();
                Dispatcher.Invoke(() =>
                {
                    if (_trainings.Count > 0)
                        NowNumberText.Text = _trainings[rnd.Next(_trainings.Count)];
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "定时器刷新异常");
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            FinishNumberText.Foreground = GlobalVariablesData.config.AllSettings.color;
            try
            {
                var flicker = FindResource("flicker") as Storyboard;
                flicker?.Begin();

                if (StartButton.Content.ToString() == "开始")
                {
                    // 开始轮播
                    Log.Information("开始轮播");
                    StartButton.IsEnabled = false;
                    _speechSynthesizer.SpeakAsyncCancelAll();
                    FinishNumberText.Visibility = Visibility.Hidden;
                    NowNumberText.Visibility = Visibility.Visible;
                    StartButton.Content = "结束";
                    _timer.Interval = GlobalVariablesData.config.MemoryFactorModeSettings.Speed;
                    var jump = FindResource("JumpStoryBoard") as Storyboard;
                    jump?.Begin();
                    _timer.Start();
                    StartButton.IsEnabled = true;
                }
                else if (StartButton.Content.ToString() == "结束")
                {
                    StartButton.IsEnabled = false;
                    // 停止并抽取
                    StartButton.Content = "开始";
                    _timer.Stop();
                    var jump = FindResource("JumpStoryBoard") as Storyboard;
                    jump?.Stop();
                    jump?.Remove();

                    _isStopping = true;
                    string selectedName = NowNumberText.Text;
                    string specialOutcome = null;

                    // 命定事件处理
                    if (_settings.otherSettings.DeterminedByFate)
                    {
                        var rnd = new Random();
                        int fate = rnd.Next(7);
                        switch (fate)
                        {
                            case 0: selectedName = No1Name.Text; break;
                            case 1: selectedName = No2Name.Text; break;
                            case 2: selectedName = No3Name.Text; break;
                            case 3: selectedName = No4Name.Text; break;
                            case 4: selectedName = No5Name.Text; break;
                            case 5: selectedName = No6Name.Text; break;
                        }
                        specialOutcome = "命定";
                        _settings.otherSettings.DeterminedByFate = false;
                    }

                    FinishNumberText.Text = selectedName;
                    NowNumberText.Visibility = Visibility.Hidden;
                    FinishNumberText.Visibility = Visibility.Visible;

                    // 备份当前因子（用于排行榜变化显示）
                    var lastFactors = _settings.thisModeJson.Select(x => new ThisModeJson { Name = x.Name, Factor = x.Factor }).ToList();

                    // 增加随机因子（每个学生 +0~5）
                    var rndGen = new Random();
                    foreach (var item in _settings.thisModeJson)
                    {
                        int inc = rndGen.Next(6);
                        item.Factor += inc;
                        // 更新缓存
                        _factorCache[item.Name] = item.Factor;
                    }

                    // 被抽中的学生因子归零
                    var selectedItem = _settings.thisModeJson.FirstOrDefault(x => x.Name == selectedName);
                    int lastFactor = selectedItem?.Factor ?? 0;
                    if (selectedItem != null)
                    {
                        selectedItem.Factor = 0;
                        _factorCache[selectedItem.Name] = 0;
                    }

                    // 重建抽奖池（移除被抽中的名字，包含新因子）
                    RebuildTrainings();

                    // 触发随机事件
                    int incidentIndex = _incidentPool[rndGen.Next(_incidentPool.Count)];
                    await ApplyIncident(incidentIndex, selectedName, rndGen);

                    // 更新最大因子学生及保底计数器
                    var maxItem = _settings.thisModeJson.OrderByDescending(x => x.Factor).FirstOrDefault();
                    if (maxItem != null)
                    {
                        if (maxItem.Name == _settings.otherSettings.MaxName)
                            _settings.otherSettings.MaxTimes++;
                        else
                        {
                            _settings.otherSettings.MaxName = maxItem.Name;
                            _settings.otherSettings.MaxTimes = Math.Max(1, _settings.otherSettings.MaxTimes - 2);
                        }
                        if (_settings.otherSettings.MaxTimes > 20)
                            _settings.otherSettings.MaxTimes = 20;

                        Dispatcher.Invoke(() =>
                        {
                            MaxIndexRealProbability.Text = $"{Math.Round((double)maxItem.Factor / _trainings.Count * 100, 2)}%";
                        });
                    }


                    // 最终 UI 更新
                    Dispatcher.Invoke(() =>
                    {
                        Count.Text = _trainings.Count.ToString();
                        LastFactorText.Text = specialOutcome ?? lastFactor.ToString();
                        GuaranteedProgressBar.Value = _settings.otherSettings.MaxTimes;
                        RemainingGuaranteedAttempts.Text = (20 - _settings.otherSettings.MaxTimes).ToString();
                    });

                    // 语音播报
                    if (GlobalVariablesData.config.MemoryFactorModeSettings.Speech)
                        _speechSynthesizer.SpeakAsync(FinishNumberText.Text);

                    // 保存设置
                    SaveSettings();
                    if (!DebugCheck.IsChecked.Value)
                    {
                        GlobalVariablesData.config.MemoryFactorModeSettings.LastName = FinishNumberText.Text;
                        GlobalVariablesData.SaveConfig();
                    }

                    // 更新排行榜动画（仅使用动画方法，不提前更新文本）
                    await Dispatcher.InvokeAsync(() => ChangeTheName(lastFactors));

                    _isStopping = false;

                    // 如果保底满20次，显示继续按钮
                    if (_settings.otherSettings.MaxTimes >= 20)
                    {
                        StartButton.Content = "继续";
                    }

                }
                else
                {
                    // “继续”按钮逻辑：生成转盘并显示
                    var sorted = _settings.thisModeJson.OrderByDescending(x => x.Factor).Take(10).ToList();
                    _settings.otherSettings.NamesInWheel = sorted.Select(x => x.Name).ToList();
                    SaveSettings();
                    NavigateToWheel();
                    StartButton.Content = "开始";
                    StartButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "抽取过程异常");
                StartButton.IsEnabled = true;
                _isStopping = false;
            }
        }
        private void ChangeTheName(List<ThisModeJson> lastThisModeJsons)
        {
            StoreOriginalPositions();

            // 获取排序后的列表（因子降序）
            List<ThisModeJson> sortedList = _settings.thisModeJson.OrderByDescending(x => x.Factor).ToList();

            Storyboard storyboard = new Storyboard();

            // 目标位置（与原 XAML 中的 Canvas.Top 值一致）
            List<double> NameTopList = new List<double>() { 32, 82, 132, 182, 232, 282, 325.96 };
            List<double> FactorTopList = new List<double>() { 44, 94, 144, 194, 244, 294, 337.96 };
            bool[] isFill = new bool[6];

            // 辅助方法：查找某个名字在排序列表中的位置（前6名内）
            int FindPosition(string name)
            {
                for (int i = 0; i < sortedList.Count && i <= 5; i++)
                {
                    if (sortedList[i].Name == name)
                        return i;
                }
                return 6; // 超出前6名，放到备用位置
            }

            // 处理六个固定 TextBlock
            TextBlock[] nameBlocks = { No1Name, No2Name, No3Name, No4Name, No5Name, No6Name };
            TextBlock[] factorBlocks = { No1Factor, No2Factor, No3Factor, No4Factor, No5Factor, No6Factor };

            for (int i = 0; i < nameBlocks.Length; i++)
            {
                int pos = FindPosition(nameBlocks[i].Text);
                if (pos < 6)
                    isFill[pos] = true;

                CreateDoubleAnimation(storyboard, nameBlocks[i], Canvas.TopProperty,
                    Canvas.GetTop(nameBlocks[i]), NameTopList[pos], 1);
                CreateDoubleAnimation(storyboard, factorBlocks[i], Canvas.TopProperty,
                    Canvas.GetTop(factorBlocks[i]), FactorTopList[pos], 1);
            }

            // 处理新增的 TextBlock（前6名中未被现有控件占用的位置）
            for (int i = 0; i < 6; i++)
            {
                if (!isFill[i] && i < sortedList.Count)
                {
                    TextBlock newName = new TextBlock()
                    {
                        FontSize = 36,
                        Text = sortedList[i].Name,
                        Foreground = GlobalVariablesData.config.AllSettings.color
                    };
                    TextBlock newFactor = new TextBlock()
                    {
                        FontSize = 24,
                        Text = sortedList[i].Factor.ToString(),
                        Foreground = GlobalVariablesData.config.AllSettings.color
                    };
                    canvas.Children.Add(newName);
                    canvas.Children.Add(newFactor);
                    addedTextBlocks.Add(newName);
                    addedTextBlocks.Add(newFactor);
                    Canvas.SetLeft(newName, 65.17);
                    Canvas.SetTop(newName, 325.96);
                    Canvas.SetLeft(newFactor, 211.41);
                    Canvas.SetTop(newFactor, 337.96);
                    CreateDoubleAnimation(storyboard, newName, Canvas.TopProperty, 325.96, NameTopList[i], 1);
                    CreateDoubleAnimation(storyboard, newFactor, Canvas.TopProperty, 337.96, FactorTopList[i], 1);
                }
            }

            storyboard.Completed += (s, e) =>
            {
                storyboard.Stop();
                storyboard.Remove();
                RevertChanges(sortedList, lastThisModeJsons);
                StartButton.IsEnabled = true;
            };
            storyboard.Begin();
        }

        private void CreateDoubleAnimation(Storyboard storyboard, DependencyObject target,
            DependencyProperty property, double from, double to, double duration)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(duration)),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));
            storyboard.Children.Add(animation);
        }

        private void RevertChanges(List<ThisModeJson> sortedList, List<ThisModeJson> lastThisModeJson)
        {
            // 停止所有动画（通过停止并移除Storyboard，这里不做额外处理）
            // 还原固定TextBlock的位置
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

            // 更新文本内容
            No1Name.Text = sortedList[0].Name;
            No2Name.Text = sortedList[1].Name;
            No3Name.Text = sortedList[2].Name;
            No4Name.Text = sortedList[3].Name;
            No5Name.Text = sortedList[4].Name;
            No6Name.Text = sortedList[5].Name;
            No1Factor.Text = sortedList[0].Factor.ToString();
            No2Factor.Text = sortedList[1].Factor.ToString();
            No3Factor.Text = sortedList[2].Factor.ToString();
            No4Factor.Text = sortedList[3].Factor.ToString();
            No5Factor.Text = sortedList[4].Factor.ToString();
            No6Factor.Text = sortedList[5].Factor.ToString();

            // 移除动态添加的TextBlock
            foreach (var tb in addedTextBlocks)
                canvas.Children.Remove(tb);
            addedTextBlocks.Clear();

            // 如果有旧数据，显示变化差值
            if (lastThisModeJson != null)
            {
                // 辅助方法：查找旧因子
                int FindLastFactor(string name)
                {
                    var item = lastThisModeJson.FirstOrDefault(x => x.Name == name);
                    return item?.Factor ?? 0;
                }

                TextBlock[] nameBlocks = { No1Name, No2Name, No3Name, No4Name, No5Name, No6Name };
                TextBlock[] addBlocks = { AddText1, AddText2, AddText3, AddText4, AddText5, AddText6 };
                for (int i = 0; i < nameBlocks.Length; i++)
                {
                    int current = sortedList[i].Factor;
                    int last = FindLastFactor(sortedList[i].Name);
                    int diff = current - last;
                    addBlocks[i].Text = diff > 0 ? $"+{diff}" : diff.ToString();
                    addBlocks[i].Foreground = diff > 0 ? Brushes.Red : Brushes.Blue;
                }

                var addStoryboard = FindResource("NewAddStoryBoard") as Storyboard;
                addStoryboard?.Begin();
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
        private async Task ApplyIncident(int incident, string selectedName, Random rnd)
        {
            // 隐藏所有事件面板（UI 操作，已在 UI 线程）
            DoubleMorePart.Visibility = Visibility.Collapsed;
            ExchangePart.Visibility = Visibility.Collapsed;
            GetFactorFromOtherPart.Visibility = Visibility.Collapsed;
            FloorAddPart.Visibility = Visibility.Collapsed;
            SkipPart.Visibility = Visibility.Collapsed;
            DeterminedByFatePart.Visibility = Visibility.Collapsed;

            switch (incident)
            {
                case 0: // 二倍
                    ApplyDoubleEvent(2);
                    break;
                case 1: // 三倍
                    ApplyDoubleEvent(3);
                    break;
                case 2: // 减半
                    ApplyHalfEvent();
                    break;
                case 3: // 交换
                    ApplyExchangeEvent();
                    break;
                case 4: // 复制
                    ApplyCopyEvent();
                    break;
                case 5: // 窃取
                    ApplyStealEvent(rnd);
                    break;
                case 6: // 保底事件
                    int add = rnd.Next(10) + 1;
                    _settings.otherSettings.MaxTimes += add;
                    if (_settings.otherSettings.MaxTimes > 20) _settings.otherSettings.MaxTimes = 20;
                    FloorAdd.Text = add.ToString();
                    incidentName.Text = "激活事件";
                    FloorAddPart.Visibility = Visibility.Visible;
                    break;
                case 7: // 跳过事件 - 需要等待
                    await ApplySkipEvent(selectedName, rnd);
                    break;
                case 8: // 平静事件
                    incidentName.Text = "平静无事";
                    break;
                case 9: // 命定事件
                    _settings.otherSettings.DeterminedByFate = true;
                    incidentName.Text = "命定事件";
                    DeterminedByFatePart.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void ApplyDoubleEvent(int multiplier)
        {
            var rnd = new Random();
            int idx = rnd.Next(_settings.thisModeJson.Count);
            var item = _settings.thisModeJson[idx];
            int old = item.Factor;
            item.Factor *= multiplier;
            _factorCache[item.Name] = item.Factor;
            DoubleName.Text = item.Name;
            DoublePast.Text = old.ToString();
            DoubleAdd.Text = $"X{multiplier}";
            DoubleNow.Text = item.Factor.ToString();
            incidentName.Text = multiplier == 2 ? "二倍事件" : "三倍事件";
            DoubleMorePart.Visibility = Visibility.Visible;
        }

        private void ApplyHalfEvent()
        {
            var rnd = new Random();
            int idx = rnd.Next(_settings.thisModeJson.Count);
            var item = _settings.thisModeJson[idx];
            int old = item.Factor;
            item.Factor /= 2;
            _factorCache[item.Name] = item.Factor;
            DoubleName.Text = item.Name;
            DoublePast.Text = old.ToString();
            DoubleAdd.Text = "X0.5";
            DoubleNow.Text = item.Factor.ToString();
            incidentName.Text = "减半事件";
            DoubleMorePart.Visibility = Visibility.Visible;
        }

        private void ApplyExchangeEvent()
        {
            var rnd = new Random();
            int idx1 = rnd.Next(_settings.thisModeJson.Count);
            int idx2 = rnd.Next(_settings.thisModeJson.Count);
            var a = _settings.thisModeJson[idx1];
            var b = _settings.thisModeJson[idx2];
            int tmp = a.Factor;
            a.Factor = b.Factor;
            b.Factor = tmp;
            _factorCache[a.Name] = a.Factor;
            _factorCache[b.Name] = b.Factor;

            ExchangeLeftName.Text = a.Name;
            ExchangeLeftFactor.Text = a.Factor.ToString();
            ExchangeRightName.Text = b.Name;
            ExchangeRightFactor.Text = b.Factor.ToString();
            incidentName.Text = "交换事件";
            ExchangePart.Visibility = Visibility.Visible;
        }

        private void ApplyCopyEvent()
        {
            var rnd = new Random();
            int idx1 = rnd.Next(_settings.thisModeJson.Count);
            int idx2 = rnd.Next(_settings.thisModeJson.Count);
            var target = _settings.thisModeJson[idx1];
            var source = _settings.thisModeJson[idx2];
            int old = target.Factor;
            target.Factor = source.Factor;
            _factorCache[target.Name] = target.Factor;
            DoubleName.Text = target.Name;
            DoublePast.Text = old.ToString();
            DoubleAdd.Text = source.Name;
            DoubleNow.Text = target.Factor.ToString();
            incidentName.Text = "复制事件";
            DoubleMorePart.Visibility = Visibility.Visible;
        }

        private void ApplyStealEvent(Random rnd)
        {
            int idxVictim = rnd.Next(_settings.thisModeJson.Count);
            int idxThief = rnd.Next(_settings.thisModeJson.Count);
            var victim = _settings.thisModeJson[idxVictim];
            var thief = _settings.thisModeJson[idxThief];
            int steal = rnd.Next(victim.Factor + 1);
            thief.Factor += steal;
            victim.Factor -= steal;
            _factorCache[thief.Name] = thief.Factor;
            _factorCache[victim.Name] = victim.Factor;

            GetFactorFromOtherName.Text = thief.Name;
            BeDeductedName.Text = victim.Name;
            BeforeGetOtherFactorFactor.Text = (thief.Factor - steal).ToString();
            ChangeFactor.Text = steal.ToString();
            NowBeDeductedFactor.Text = victim.Factor.ToString();
            incidentName.Text = "窃取事件";
            GetFactorFromOtherPart.Visibility = Visibility.Visible;
        }

        private async Task ApplySkipEvent(string selectedName, Random rnd)
        {
            int newIdx = rnd.Next(_settings.thisModeJson.Count);
            var newItem = _settings.thisModeJson[newIdx];
            var oldItem = _settings.thisModeJson.FirstOrDefault(x => x.Name == selectedName);
            if (oldItem != null)
            {
                oldItem.Factor = newItem.Factor;
                newItem.Factor = 0;
                _factorCache[oldItem.Name] = oldItem.Factor;
                _factorCache[newItem.Name] = 0;
                AfterSkipNowFactor.Text = oldItem.Factor.ToString();
                await Task.Delay(2000);
                // 更新 UI 显示
                Dispatcher.Invoke(() =>
                {
                    NowNumberText.Text = newItem.Name;
                    FinishNumberText.Text = newItem.Name;
                    FinishNumberText.Foreground = Brushes.Purple;
                    BeSkipedName.Text = selectedName;
                    AfterNowName.Text = newItem.Name;
                });
                selectedName = newItem.Name;
            }
            incidentName.Text = "跳过事件";
            SkipPart.Visibility = Visibility.Visible;
        }

        // 排行榜动画更新（原 ChangeTheName 逻辑重构）
        private void UpdateRankingDisplay(List<ThisModeJson> previousFactors)
        {
            Dispatcher.Invoke(() =>
            {
                var sorted = _settings.thisModeJson.OrderByDescending(x => x.Factor).ToList();
                // 更新 UI 文本块（1-6名）
                var nameBlocks = new[] { No1Name, No2Name, No3Name, No4Name, No5Name, No6Name };
                var factorBlocks = new[] { No1Factor, No2Factor, No3Factor, No4Factor, No5Factor, No6Factor };
                var addBlocks = new[] { AddText1, AddText2, AddText3, AddText4, AddText5, AddText6 };

                for (int i = 0; i < 6 && i < sorted.Count; i++)
                {
                    nameBlocks[i].Text = sorted[i].Name;
                    factorBlocks[i].Text = sorted[i].Factor.ToString();

                    if (previousFactors != null)
                    {
                        var prev = previousFactors.FirstOrDefault(x => x.Name == sorted[i].Name);
                        int diff = sorted[i].Factor - (prev?.Factor ?? 0);
                        addBlocks[i].Text = diff > 0 ? $"+{diff}" : diff.ToString();
                        addBlocks[i].Foreground = diff > 0 ? Brushes.Red : Brushes.Blue;
                    }
                }

                // 显示变化动画
                if (previousFactors != null)
                {
                    var story = FindResource("NewAddStoryBoard") as Storyboard;
                    story?.Begin();
                }

            });
        }


        // ---------- 辅助方法 ----------
        private void NavigateToWheel()
        {
            if (GlobalVariablesData.config.MemoryFactorModeSettings.Speech)
                WheelFrame.Navigate(new MemoryFactorChildrenPage.Wheel(_settings.otherSettings.NamesInWheel, _speechSynthesizer));
            else
                WheelFrame.Navigate(new MemoryFactorChildrenPage.Wheel(_settings.otherSettings.NamesInWheel, null));
            WheelFrame.Navigated += WheelFrame_Navigated;
            var show = FindResource("FrameShow") as Storyboard;
            show?.Begin();
        }
        private Dictionary<TextBlock, double> originalTops = new Dictionary<TextBlock, double>();
        private List<TextBlock> addedTextBlocks = new List<TextBlock>();

        private void WheelFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is MemoryFactorChildrenPage.Wheel wheel)
            {
                wheel.DataSubmitted += (s, data) =>
                {
                    var item = _settings.thisModeJson.FirstOrDefault(x => x.Name == data);
                    if (item != null)
                    {
                        item.Factor = 0;
                        _factorCache[data] = 0;
                        if (data == _settings.otherSettings.MaxName)
                        {
                            // 被清除的是最大因子学生，重置保底
                            var hide = FindResource("FrameHide") as Storyboard;
                            hide?.Begin();
                            WheelFrame.Visibility = Visibility.Collapsed;
                            _settings.otherSettings.NamesInWheel.Clear();
                            _settings.otherSettings.MaxTimes = 1;
                            SaveSettings();

                            // 重新计算最大因子
                            var max = _settings.thisModeJson.OrderByDescending(x => x.Factor).First();
                            _settings.otherSettings.MaxName = max.Name;
                            NowNumberText.Text = data;
                            FinishNumberText.Text = data;
                            // 刷新排行榜
                            UpdateRankingDisplay(null);
                        }
                        else
                        {
                            // 替换转盘中的名字为当前最大因子学生
                            int idx = _settings.otherSettings.NamesInWheel.IndexOf(data);
                            if (idx >= 0 && !string.IsNullOrEmpty(_settings.otherSettings.MaxName))
                            {
                                _settings.otherSettings.NamesInWheel[idx] = _settings.otherSettings.MaxName;
                            }
                            SaveSettings();
                        }
                    }
                };
            }
        }

        private void ShowStoryboard(string resourceKey)
        {
            var story = FindResource(resourceKey) as Storyboard;
            story?.Begin();
        }

        // ---------- 事件处理 ----------
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariablesData.config.MemoryFactorModeSettings.Speech = SpeechButton.IsChecked == true;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string dir = Path.Combine(GlobalVariablesData.userDataDir, "Mode_data", "MemoryFactoryMode");
                string backupDir = Path.Combine(dir, "Backups");
                Directory.CreateDirectory(backupDir);
                string file = Path.Combine(dir, "Memory.json");
                string backup = Path.Combine(backupDir, DateTime.Now.ToString("yyyy_MM_d_HH_mm_ss"));
                File.Copy(file, backup);
                File.Delete(file);

                _settings.otherSettings.MaxName = null;
                _settings.otherSettings.MaxTimes = 1;
                StartButton.IsEnabled = false;
                ResetButton.IsEnabled = false;
                WheelFrame.IsEnabled = false;
                SnackBarFunction.ShowSnackBarInMainWindow("重置成功，请重新打开界面", ControlAppearance.Primary);
                Log.Information("记忆因子数据重置成功");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "重置失败");
                MessageBoxFunction.ShowMessageBoxError("出错：" + ex.Message);
            }
        }

        private void QuitDebug_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariablesData.config.MemoryFactorModeSettings.debug = false;
            if (!DebugCheck.IsChecked.Value) GlobalVariablesData.SaveConfig();
            QuitDebug.Visibility = Visibility.Collapsed;
            GetTraning.Visibility = Visibility.Collapsed;
            DebugCheck.Visibility = Visibility.Collapsed;
            OpenWheel.Visibility = Visibility.Collapsed;
        }

        private void GetTraning_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = string.Join(Environment.NewLine, _trainings);
                System.Windows.Clipboard.SetText(text);
                SnackBarFunction.ShowSnackBarInMainWindow("已复制到剪贴板", ControlAppearance.Primary);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "复制失败");
            }
        }

        private void OpenWheel_Click(object sender, RoutedEventArgs e)
        {
            NavigateToWheel();
            WheelFrame.Visibility = Visibility.Visible;
        }

        // ---------- 资源释放 ----------
        public void Dispose()
        {
            if (_disposed) return;
            _timer?.Stop();
            _timer?.Dispose();
            _speechSynthesizer?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~MemoryFactorMode()
        {
            Dispose();
        }

        private async void SynchronizationButton_Click(object sender, RoutedEventArgs e)
        {
            //需要被删除的名字索引
            List<int> NeedDelName = new List<int>();
            foreach (ThisModeJson name in _settings.thisModeJson)
            {
                bool needDel = true;
                foreach (string GetName in GlobalVariablesData.config.AllSettings.Name)
                {
                    if (name.Name == GetName)
                    {
                        needDel = false;
                        break;
                    }
                }
                if (needDel)
                {
                    NeedDelName.Add(_settings.thisModeJson.IndexOf(name));
                }
            }
            //需要被添加的名字索引
            List<int> NeedAddName = new List<int>();
            foreach (string GetName in GlobalVariablesData.config.AllSettings.Name)
            {
                bool needAdd = true;
                foreach (ThisModeJson name in _settings.thisModeJson)
                {
                    if (name.Name == GetName)
                    {
                        needAdd = false;
                        break;
                    }
                }
                if (needAdd)
                {
                    NeedAddName.Add(GlobalVariablesData.config.AllSettings.Name.IndexOf(GetName));
                }
            }
            if (NeedAddName.Count == 0 && NeedDelName.Count == 0)
            {
                SnackBarFunction.ShowSnackBarInMainWindow("无需同步", ControlAppearance.Info);
                return;
            }
            if (GlobalVariablesData.config.AllSettings.Name.Count < 10)
            {
                SnackBarFunction.ShowSnackBarInMainWindow("名字总数不能少于10个", ControlAppearance.Caution);
            }
            string text = "需要被删除的名字：";
            foreach (int index in NeedDelName)
            {
                text = text + "\n" + _settings.thisModeJson[index].Name + " - " + _settings.thisModeJson[index].Factor.ToString();
            }
            text += "\n需要被添加的名字：";
            foreach (int index in NeedAddName)
            {
                text = text + "\n" + GlobalVariablesData.config.AllSettings.Name[index];
            }
            text += "\n请确认是否进行同步";
            var dialog = new Wpf.Ui.Controls.ContentDialog
            {
                Title = "同步确认",
                Content = text,
                PrimaryButtonText = "确定",
                IsSecondaryButtonEnabled = false,
                CloseButtonText = "取消"
            };

            dialog.DialogHost = RootContentDialogPresenter;
            ContentDialogResult contentDialogResult = await dialog.ShowAsync();
            if (contentDialogResult == ContentDialogResult.Primary)
            {
                for (int i = NeedDelName.Count - 1; i >= 0; i--)
                {
                    _settings.thisModeJson.RemoveAt(NeedDelName[i]);
                }
                foreach (int index in NeedAddName)
                {
                    _settings.thisModeJson.Add(new ThisModeJson { Name = GlobalVariablesData.config.AllSettings.Name[index], Factor = 1 });
                }
                SaveSettings();
                SnackBarFunction.ShowSnackBarInMainWindow("同步完成,请重新打开界面", ControlAppearance.Success);
                _settings.otherSettings.MaxName = null;
                _settings.otherSettings.MaxTimes = 1;
                StartButton.IsEnabled = false;
                ResetButton.IsEnabled = false;
                WheelFrame.IsEnabled = false;
            }
        }
    }
}