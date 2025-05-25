using Masuit.Tools.Logging;
using System;
using System.Drawing;
using System.Speech.Synthesis;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            SpeechCheck.IsChecked = GlobalVariables.json.OnePeopleModeSettings.Speech;
            WaitCheck.IsChecked = GlobalVariables.json.OnePeopleModeSettings.Wait;
            SpeechCheck.IsEnabled = !GlobalVariables.json.OnePeopleModeSettings.Locked;
            WaitCheck.IsEnabled = !GlobalVariables.json.OnePeopleModeSettings.Locked;
            timer = new System.Timers.Timer(GlobalVariables.json.OnePeopleModeSettings.Speed);
            
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            if (GlobalVariables.json.OnePeopleModeSettings.Speed == 0)
            {
                GlobalVariables.json.OnePeopleModeSettings.Speed = 20;
            }
            if (!GlobalVariables.json.AllSettings.SystemSpeech)
            {
                _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult); // 选择女声
                _speechSynthesizer.Rate = GlobalVariables.json.AllSettings.Speed; // 语速 (-10 ~ 10)
                _speechSynthesizer.Volume = GlobalVariables.json.AllSettings.Volume;
            }
            NowNumberText.Foreground = GlobalVariables.json.AllSettings.color;
            FinishText.Foreground = GlobalVariables.json.AllSettings.color;
            NowNumberText.FontFamily = GlobalVariables.json.AllSettings.Font;
            FinishText.FontFamily = GlobalVariables.json.AllSettings.Font;
            if (GlobalVariables.json.OnePeopleModeSettings.LastName != null)
            {
                NowNumberText.Text = GlobalVariables.json.OnePeopleModeSettings.LastName;
            }
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
                    NowNumberText.Text = GlobalVariables.json.AllSettings.Name[NowIndex];
                });
            }
            catch (Exception ex)
            {
                LogManager.Error(ex);
            }
            NowIndex++;
            if (NowIndex >= GlobalVariables.json.AllSettings.Name.Count)
            {
                NowIndex = 0;
            }

        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            _speechSynthesizer.SpeakAsyncCancelAll();
            if (GlobalVariables.json.AllSettings.Name.Count == 0)
            {
                MessageBoxFunction.ShowMessageBoxWarning("I live alone.But I don't fell lonely\n翻译：学生名单为空！");
                StartButton.IsEnabled = true;
                return;
            }
            if (GlobalVariables.json.AllSettings.Name.Count == 1)
            {
                MessageBoxFunction.ShowMessageBoxWarning("如果你要恶搞某人，建议前往小工具\n翻译：学生名单只有一位！");
                StartButton.IsEnabled = true;
                return;
            }
            if (StartButton.Content.ToString() == "开始")
            {
                FinishText.Visibility = Visibility.Hidden;
                NowNumberText.Visibility = Visibility.Visible;
                StartButton.Content = "结束";
                timer.Interval = GlobalVariables.json.OnePeopleModeSettings.Speed;
                timer.Start();
                IsReadyToStop = false;
                StartButton.IsEnabled = true;
            }
            else
            {
                if (!GlobalVariables.json.OnePeopleModeSettings.Wait)
                {
                    StartButton.Content = "暂停中";
                    IsReadyToStop = true;
                    StartButton.IsEnabled = false;
                }
                else
                {
                    string Text = NowNumberText.Text;
                    GlobalVariables.json.OnePeopleModeSettings.LastName = Text;
                    timer.Stop();
                    StartButton.Content = "开始";
                    if (GlobalVariables.json.OnePeopleModeSettings.Speech)
                    {
                        _speechSynthesizer.SpeakAsync(Text);
                    }
                    FinishText.Text = Text;
                    NowNumberText.Visibility = Visibility.Hidden;
                    FinishText.Visibility = Visibility.Visible;
                }
                StartButton.IsEnabled = true;
            }
        }




        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            GlobalVariables.SaveJson();
        }

        private void SpeechCheck_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.OnePeopleModeSettings.Speech = (bool)SpeechCheck.IsChecked;
            GlobalVariables.SaveJson();
        }

        private void WaitCheck_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.OnePeopleModeSettings.Wait = (bool)WaitCheck.IsChecked;
            GlobalVariables.SaveJson();
        }
    }
}
