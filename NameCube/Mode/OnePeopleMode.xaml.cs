using Masuit.Tools.Logging;
using Serilog;
using System;
using System.Drawing;
using System.Speech.Synthesis;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NameCube.Mode
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


        public System.Timers.Timer timer;
        int NowIndex = 0;
        bool IsReadyToStop;
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();



        public OnePeopleMode()
        {
            InitializeComponent();
            DataContext = this;
            Log.Debug("OnePeopleMode页面初始化完成");
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (IsReadyToStop)
                {
                    timer.Interval *= 1.5;
                    Log.Verbose("准备停止，间隔增加: {Interval}", timer.Interval);
                }

                if (timer.Interval >= 1500)
                {
                    Random random = new Random();
                    if (random.Next(0, 2) == 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            NowNumberText.Text = GlobalVariables.json.AllSettings.Name[NowIndex];
                        });
                        NowIndex++;
                        if (NowIndex >= GlobalVariables.json.AllSettings.Name.Count)
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
                            string resultText = "";
                            Dispatcher.Invoke(() =>
                            {
                                resultText = NowNumberText.Text;
                            });
                            Log.Information("单抽模式结束，结果: {Result}", resultText);
                            _speechSynthesizer.SpeakAsync(resultText);
                        }
                    });
                    timer.Stop();
                    Log.Information("单抽模式结束，结果: {Result}", NowNumberText.Text);
                    return;
                }

                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        NowNumberText.Text = GlobalVariables.json.AllSettings.Name[NowIndex];
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "更新显示文本时发生异常");
                }

                NowIndex++;
                if (NowIndex >= GlobalVariables.json.AllSettings.Name.Count)
                {
                    NowIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OnePeopleMode定时器处理时发生异常");
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StartButton.IsEnabled = false;
                _speechSynthesizer.SpeakAsyncCancelAll();

                if (GlobalVariables.json.AllSettings.Name.Count == 0)
                {
                    Log.Warning("学生名单为空");
                    SnackBarFunction.ShowSnackBarInMainWindow("学生名单为空！", Wpf.Ui.Controls.ControlAppearance.Caution);
                    StartButton.IsEnabled = true;
                    return;
                }

                if (GlobalVariables.json.AllSettings.Name.Count == 1)
                {
                    Log.Warning("学生名单只有一位");
                    SnackBarFunction.ShowSnackBarInMainWindow("如果你要恶搞某人，建议前往小工具\n翻译：学生名单只有一位！", Wpf.Ui.Controls.ControlAppearance.Caution);
                    StartButton.IsEnabled = true;
                    return;
                }

                var jumpStoryBoard = FindResource("JumpStoryBoard") as Storyboard;
                if (StartButton.Content.ToString() == "开始")
                {
                    Log.Information("开始单抽模式");
                    FinishText.Visibility = Visibility.Hidden;
                    NowNumberText.Visibility = Visibility.Visible;
                    StartButton.Content = "结束";
                    timer.Interval = GlobalVariables.json.OnePeopleModeSettings.Speed;
                    jumpStoryBoard.Begin();
                    timer.Start();
                    IsReadyToStop = false;
                    StartButton.IsEnabled = true;
                    Log.Debug("单抽模式开始，速度: {Speed}ms", GlobalVariables.json.OnePeopleModeSettings.Speed);
                }
                else
                {
                    if (!GlobalVariables.json.OnePeopleModeSettings.Wait)
                    {
                        Log.Debug("暂停模式");
                        StartButton.Content = "暂停中";
                        jumpStoryBoard.Stop();
                        jumpStoryBoard.Remove();
                        IsReadyToStop = true;
                        StartButton.IsEnabled = false;
                    }
                    else
                    {
                        string Text = NowNumberText.Text;
                        GlobalVariables.json.OnePeopleModeSettings.LastName = Text;
                        jumpStoryBoard.Stop();
                        jumpStoryBoard.Remove();
                        timer.Stop();
                        StartButton.Content = "开始";

                        if (GlobalVariables.json.OnePeopleModeSettings.Speech)
                        {
                            Log.Debug("语音播报结果: {Name}", Text);
                            _speechSynthesizer.SpeakAsync(Text);
                        }

                        FinishText.Text = Text;
                        NowNumberText.Visibility = Visibility.Hidden;
                        FinishText.Visibility = Visibility.Visible;
                        Log.Information("单抽模式结束，结果: {Result}", Text);
                    }
                    StartButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OnePeopleMode开始/结束操作时发生异常");
                StartButton.IsEnabled = true;
            }
        }

        private void SpeechCheck_Click(object sender, RoutedEventArgs e)
        {
            bool newValue = (bool)SpeechCheck.IsChecked;
            Log.Debug("语音播报开关: {Value}", newValue);
            GlobalVariables.json.OnePeopleModeSettings.Speech = newValue;
            GlobalVariables.SaveJson();
        }

        private void WaitCheck_Click(object sender, RoutedEventArgs e)
        {
            bool newValue = (bool)WaitCheck.IsChecked;
            Log.Debug("等待模式开关: {Value}", newValue);
            GlobalVariables.json.OnePeopleModeSettings.Wait = newValue;
            GlobalVariables.SaveJson();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("OnePeopleMode页面加载");

                SpeechCheck.IsChecked = GlobalVariables.json.OnePeopleModeSettings.Speech;
                WaitCheck.IsChecked = GlobalVariables.json.OnePeopleModeSettings.Wait;
                SpeechCheck.IsEnabled = !GlobalVariables.json.OnePeopleModeSettings.Locked;
                WaitCheck.IsEnabled = !GlobalVariables.json.OnePeopleModeSettings.Locked;

                timer = new System.Timers.Timer(GlobalVariables.json.OnePeopleModeSettings.Speed);
                timer.AutoReset = true;
                timer.Elapsed += Timer_Elapsed;

                if (GlobalVariables.json.OnePeopleModeSettings.Speed == 0)
                {
                    Log.Debug("重置速度为默认值20");
                    GlobalVariables.json.OnePeopleModeSettings.Speed = 20;
                }

                if (!GlobalVariables.json.AllSettings.SystemSpeech)
                {
                    _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                    _speechSynthesizer.Rate = GlobalVariables.json.AllSettings.Speed;
                    _speechSynthesizer.Volume = GlobalVariables.json.AllSettings.Volume;
                    Log.Debug("配置语音合成器: 性别=Female, 音量={Volume}, 语速={Speed}",
                        GlobalVariables.json.AllSettings.Volume,
                        GlobalVariables.json.AllSettings.Speed);
                }

                NowNumberText.Foreground = GlobalVariables.json.AllSettings.color;
                FinishText.Foreground = GlobalVariables.json.AllSettings.color;
                NowNumberText.FontFamily = GlobalVariables.json.AllSettings.Font;
                FinishText.FontFamily = GlobalVariables.json.AllSettings.Font;

                if (GlobalVariables.json.OnePeopleModeSettings.LastName != null)
                {
                    NowNumberText.Text = GlobalVariables.json.OnePeopleModeSettings.LastName;
                    Log.Debug("设置上次抽取结果: {LastName}", GlobalVariables.json.OnePeopleModeSettings.LastName);
                }

                Log.Information("OnePeopleMode页面加载完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OnePeopleMode页面加载时发生异常");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            GlobalVariables.SaveJson();
        }
    }
}
