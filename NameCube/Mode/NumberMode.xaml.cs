using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
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

namespace NameCube.Mode
{
    /// <summary>
    /// NumberMode.xaml 的交互逻辑
    /// </summary>
    public partial class NumberMode : Page
    {
        bool CanChange;
        public System.Timers.Timer timer = new System.Timers.Timer();
        Random Random=new Random();
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();

        public NumberMode()
        {
            InitializeComponent();
           
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string text= Random.StrictNext(GlobalVariables.json.NumberModeSettings.Num + 1).ToString();
            if(text=="0")
            {
                return;
            }
            else
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    NowNumberText.Text = text;
                }));
            }
            
        }

        private void SpeechCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.NumberModeSettings.Speak = SpeakCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void NumberBox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if(CanChange)
            {
                if(NumberBox.Value==null)
                {
                    Button_Click(sender,null);
                }
                GlobalVariables.json.NumberModeSettings.Num = (int)NumberBox.Value;
                GlobalVariables.SaveJson();
            }
        }

        private void NumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(CanChange)
            {
                int get = NumberBox.Text.ToInt32(-1);
                if(get!=-1)
                {
                    NumberBox.Value=get;
                }
                else
                {
                    NumberBox.Value=GlobalVariables.json.AllSettings.Name.Count;
                    NumberBox.Text = null;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NumberBox.Value = GlobalVariables.json.AllSettings.Name.Count;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _speechSynthesizer.SpeakAsyncCancelAll();
            StartButton.IsEnabled = false;
            var jumpStoryBoard = FindResource("JumpStoryBoard") as Storyboard;
            if (StartButton.Content.ToString()=="开始")
            {
                timer.Interval = GlobalVariables.json.NumberModeSettings.Speed;
                FinishText.Visibility = Visibility.Hidden;
                NowNumberText.Visibility = Visibility.Visible;
                StartButton.Content = "结束";
                jumpStoryBoard.Begin();
                timer.Start();
                StartButton.IsEnabled=true;
            }
            else
            {
                StartButton.Content = "开始";
                jumpStoryBoard.Stop();
                jumpStoryBoard.Remove();
                timer.Stop();
                string get = NowNumberText.Text;
                FinishText.Text= get;
                NowNumberText.Visibility = Visibility.Hidden;
                FinishText.Visibility= Visibility.Visible;
                if(GlobalVariables.json.NumberModeSettings.Speak)
                {
                    _speechSynthesizer.SpeakAsync(get);
                }
                GlobalVariables.json.NumberModeSettings.LastName = get;
                StartButton.IsEnabled = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CanChange = false;
            SpeakCheck.IsChecked = GlobalVariables.json.NumberModeSettings.Speak;
            NumberBox.Value = GlobalVariables.json.NumberModeSettings.Num;
            CanChange = true;
            timer.Elapsed += Timer_Elapsed;
            if (GlobalVariables.json.NumberModeSettings.Speed == 0)
            {
                GlobalVariables.json.NumberModeSettings.Speed = 20;
            }
            if (!GlobalVariables.json.AllSettings.SystemSpeech)
            {
                _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                _speechSynthesizer.Volume = GlobalVariables.json.AllSettings.Volume;
                _speechSynthesizer.Rate = GlobalVariables.json.AllSettings.Speed;
            }
            if (GlobalVariables.json.NumberModeSettings.Locked)
            {
                SpeakCheck.IsEnabled = false;
                NumberBox.IsEnabled = false;
                Button1.IsEnabled = false;
            }
            if (GlobalVariables.json.NumberModeSettings.LastName != null)
            {
                NowNumberText.Text = GlobalVariables.json.NumberModeSettings.LastName;
            }
            NowNumberText.Foreground = GlobalVariables.json.AllSettings.color;
            FinishText.Foreground = GlobalVariables.json.AllSettings.color;
            NowNumberText.FontFamily = GlobalVariables.json.AllSettings.Font;
            FinishText.FontFamily = GlobalVariables.json.AllSettings.Font;
        }
    }
}
