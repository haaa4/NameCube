using Masuit.Tools;
using Serilog;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Controls;

namespace NameCube.Mode
{
    /// <summary>
    /// PrepareMode.xaml 的交互逻辑
    /// </summary>
    public partial class PrepareMode : Page
    {
        public PrepareMode()
        {
            InitializeComponent();
            Log.Debug("PrepareMode页面初始化完成");
        }

        bool CanChange;
        public System.Timers.Timer timer = new System.Timers.Timer();
        Random Random = new Random();
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        int now;

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                now++;
                if (now >= 5)
                {
                    now = 0;
                }

                this.Dispatcher.Invoke(new Action(() =>
                {
                    NowNumberText.Text = GlobalVariables.json.PrepareModeSetting.Name[now];
                    Ready1.Text = GlobalVariables.json.AllSettings.Name[Random.StrictNext(GlobalVariables.json.AllSettings.Name.Count)];
                    Ready2.Text = GlobalVariables.json.AllSettings.Name[Random.StrictNext(GlobalVariables.json.AllSettings.Name.Count)];
                    Ready3.Text = GlobalVariables.json.AllSettings.Name[Random.StrictNext(GlobalVariables.json.AllSettings.Name.Count)];
                    Ready4.Text = GlobalVariables.json.AllSettings.Name[Random.StrictNext(GlobalVariables.json.AllSettings.Name.Count)];
                    Ready5.Text = GlobalVariables.json.AllSettings.Name[Random.StrictNext(GlobalVariables.json.AllSettings.Name.Count)];
                }));

                Log.Verbose("预备模式轮播，当前索引: {Index}", now);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PrepareMode定时器处理时发生异常");
            }
        }

        private void SpeechCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                bool newValue = SpeakCheck.IsChecked.Value;
                Log.Debug("语音播报开关: {Value}", newValue);
                GlobalVariables.json.PrepareModeSetting.Speak = newValue;
                GlobalVariables.SaveJson();
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _speechSynthesizer.SpeakAsyncCancelAll();
                StartButton.IsEnabled = false;
                var jumpStoryBoard = FindResource("JumpStoryBoard") as Storyboard;

                if (StartButton.Content.ToString() == "开始")
                {
                    Log.Information("开始预备模式");
                    timer.Interval = GlobalVariables.json.PrepareModeSetting.Speed;

                    // 显示预备名字
                    BeforeReady1.Text = "(" + GlobalVariables.json.PrepareModeSetting.Name[0] + ")";
                    BeforeReady2.Text = "(" + GlobalVariables.json.PrepareModeSetting.Name[1] + ")";
                    BeforeReady3.Text = "(" + GlobalVariables.json.PrepareModeSetting.Name[2] + ")";
                    BeforeReady4.Text = "(" + GlobalVariables.json.PrepareModeSetting.Name[3] + ")";
                    BeforeReady5.Text = "(" + GlobalVariables.json.PrepareModeSetting.Name[4] + ")";

                    Log.Debug("预备名单: {Name1}, {Name2}, {Name3}, {Name4}, {Name5}",
                        GlobalVariables.json.PrepareModeSetting.Name[0],
                        GlobalVariables.json.PrepareModeSetting.Name[1],
                        GlobalVariables.json.PrepareModeSetting.Name[2],
                        GlobalVariables.json.PrepareModeSetting.Name[3],
                        GlobalVariables.json.PrepareModeSetting.Name[4]);

                    BeforeReady1.Visibility = Visibility.Visible;
                    BeforeReady2.Visibility = Visibility.Visible;
                    BeforeReady3.Visibility = Visibility.Visible;
                    BeforeReady4.Visibility = Visibility.Visible;
                    BeforeReady5.Visibility = Visibility.Visible;
                    FinishText.Visibility = Visibility.Hidden;
                    FinishReady1.Visibility = Visibility.Hidden;
                    FinishReady2.Visibility = Visibility.Hidden;
                    FinishReady3.Visibility = Visibility.Hidden;
                    FinishReady4.Visibility = Visibility.Hidden;
                    FinishReady5.Visibility = Visibility.Hidden;
                    NowNumberText.Visibility = Visibility.Visible;
                    Ready1.Visibility = Visibility.Visible;
                    Ready2.Visibility = Visibility.Visible;
                    Ready3.Visibility = Visibility.Visible;
                    Ready4.Visibility = Visibility.Visible;
                    Ready5.Visibility = Visibility.Visible;
                    StartButton.Content = "结束";
                    jumpStoryBoard.Begin();
                    timer.Start();
                    StartButton.IsEnabled = true;
                }
                else
                {
                    Log.Information("结束预备模式");
                    StartButton.Content = "开始";
                    jumpStoryBoard.Stop();
                    jumpStoryBoard.Remove();
                    timer.Stop();

                    string get = NowNumberText.Text,
                        get1 = Ready1.Text,
                        get2 = Ready2.Text,
                        get3 = Ready3.Text,
                        get4 = Ready4.Text,
                        get5 = Ready5.Text;

                    FinishText.Text = get;
                    FinishReady1.Text = get1;
                    FinishReady2.Text = get2;
                    FinishReady3.Text = get3;
                    FinishReady4.Text = get4;
                    FinishReady5.Text = get5;

                    NowNumberText.Visibility = Visibility.Hidden;
                    Ready1.Visibility = Visibility.Hidden;
                    Ready2.Visibility = Visibility.Hidden;
                    Ready3.Visibility = Visibility.Hidden;
                    Ready4.Visibility = Visibility.Hidden;
                    Ready5.Visibility = Visibility.Hidden;
                    FinishText.Visibility = Visibility.Visible;
                    FinishReady1.Visibility = Visibility.Visible;
                    FinishReady2.Visibility = Visibility.Visible;
                    FinishReady3.Visibility = Visibility.Visible;
                    FinishReady4.Visibility = Visibility.Visible;
                    FinishReady5.Visibility = Visibility.Visible;

                    GlobalVariables.json.PrepareModeSetting.Name = new List<string> { get1, get2, get3, get4, get5 };
                    GlobalVariables.json.PrepareModeSetting.LastName = get;

                    if (GlobalVariables.json.PrepareModeSetting.Speak)
                    {
                        Log.Debug("语音播报结果: {Name}", get);
                        _speechSynthesizer.SpeakAsync(get);
                    }

                    StartButton.IsEnabled = true;
                    Log.Information("预备模式完成，结果: {Result}, 新预备名单: {Name1}, {Name2}, {Name3}, {Name4}, {Name5}",
                        get, get1, get2, get3, get4, get5);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PrepareMode开始/结束操作时发生异常");
                StartButton.IsEnabled = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("PrepareMode页面加载");

                CanChange = false;
                SpeakCheck.IsChecked = GlobalVariables.json.PrepareModeSetting.Speak;
                CanChange = true;
                timer.Elapsed += Timer_Elapsed;

                if (GlobalVariables.json.PrepareModeSetting.Speed == 0)
                {
                    Log.Debug("重置速度为默认值20");
                    GlobalVariables.json.PrepareModeSetting.Speed = 20;
                }

                if (!GlobalVariables.json.AllSettings.SystemSpeech)
                {
                    _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                    _speechSynthesizer.Volume = GlobalVariables.json.AllSettings.Volume;
                    _speechSynthesizer.Rate = GlobalVariables.json.AllSettings.Speed;
                    Log.Debug("配置语音合成器: 性别=Female, 音量={Volume}, 语速={Speed}",
                        GlobalVariables.json.AllSettings.Volume,
                        GlobalVariables.json.AllSettings.Speed);
                }

                if (GlobalVariables.json.PrepareModeSetting.Locked)
                {
                    Log.Debug("页面设置为锁定状态");
                    SpeakCheck.IsEnabled = false;
                }

                if (GlobalVariables.json.PrepareModeSetting.Name == null || GlobalVariables.json.PrepareModeSetting.Name.Count == 0)
                {
                    Log.Information("初始化预备名单");
                    GlobalVariables.json.PrepareModeSetting.Name = new List<string>();
                    Random random = new Random();

                    for (int i = 1; i <= 5; i++)
                    {
                        string name = GlobalVariables.json.AllSettings.Name[random.StrictNext(GlobalVariables.json.AllSettings.Name.Count)];
                        GlobalVariables.json.PrepareModeSetting.Name.Add(name);
                        Log.Debug("添加预备名单 {Index}: {Name}", i, name);
                    }

                    GlobalVariables.SaveJson();
                }

                if (GlobalVariables.json.PrepareModeSetting.LastName != null)
                {
                    NowNumberText.Text = GlobalVariables.json.PrepareModeSetting.LastName;
                    Log.Debug("设置上次抽取结果: {LastName}", GlobalVariables.json.PrepareModeSetting.LastName);
                }

                Ready1.Text = GlobalVariables.json.PrepareModeSetting.Name[0];
                Ready2.Text = GlobalVariables.json.PrepareModeSetting.Name[1];
                Ready3.Text = GlobalVariables.json.PrepareModeSetting.Name[2];
                Ready4.Text = GlobalVariables.json.PrepareModeSetting.Name[3];
                Ready5.Text = GlobalVariables.json.PrepareModeSetting.Name[4];

                NowNumberText.Foreground = GlobalVariables.json.AllSettings.color;
                FinishText.Foreground = GlobalVariables.json.AllSettings.color;
                Ready1.Foreground = GlobalVariables.json.AllSettings.color;
                Ready2.Foreground = GlobalVariables.json.AllSettings.color;
                Ready3.Foreground = GlobalVariables.json.AllSettings.color;
                Ready4.Foreground = GlobalVariables.json.AllSettings.color;
                Ready5.Foreground = GlobalVariables.json.AllSettings.color;
                FinishReady1.Foreground = GlobalVariables.json.AllSettings.color;
                FinishReady2.Foreground = GlobalVariables.json.AllSettings.color;
                FinishReady3.Foreground = GlobalVariables.json.AllSettings.color;
                FinishReady4.Foreground = GlobalVariables.json.AllSettings.color;
                FinishReady5.Foreground = GlobalVariables.json.AllSettings.color;
                NowNumberText.FontFamily = GlobalVariables.json.AllSettings.Font;
                FinishText.FontFamily = GlobalVariables.json.AllSettings.Font;
                Ready1.FontFamily = GlobalVariables.json.AllSettings.Font;
                Ready2.FontFamily = GlobalVariables.json.AllSettings.Font;
                Ready3.FontFamily = GlobalVariables.json.AllSettings.Font;
                Ready4.FontFamily = GlobalVariables.json.AllSettings.Font;
                Ready5.FontFamily = GlobalVariables.json.AllSettings.Font;
                FinishReady1.FontFamily = GlobalVariables.json.AllSettings.Font;
                FinishReady2.FontFamily = GlobalVariables.json.AllSettings.Font;
                FinishReady3.FontFamily = GlobalVariables.json.AllSettings.Font;
                FinishReady4.FontFamily = GlobalVariables.json.AllSettings.Font;
                FinishReady5.FontFamily = GlobalVariables.json.AllSettings.Font;

                Log.Information("PrepareMode页面加载完成，预备名单已设置");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PrepareMode页面加载时发生异常");
            }
        }
    }
}