using Masuit.Tools;
using Serilog;
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
        Random Random = new Random();
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();

        public NumberMode()
        {
            InitializeComponent();
            Log.Debug("NumberMode页面初始化完成");
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string text = Random.StrictNext(GlobalVariablesData.config.NumberModeSettings.Num + 1).ToString();
                if (text == "0")
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
            catch (Exception ex)
            {
                Log.Error(ex, "NumberMode定时器处理时发生异常");
            }
        }

        private void SpeechCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                bool newValue = SpeakCheck.IsChecked.Value;
                Log.Debug("语音播报开关: {Value}", newValue);
                GlobalVariablesData.config.NumberModeSettings.Speak = newValue;
                GlobalVariablesData.SaveConfig();
            }
        }

        private void NumberBox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (CanChange)
            {
                if (NumberBox.Value == null)
                {
                    Log.Debug("数字框值为空，重置为名单数量");
                    Button_Click(sender, null);
                }
                else
                {
                    Log.Debug("数字范围变化: {Value}", NumberBox.Value);
                }

                GlobalVariablesData.config.NumberModeSettings.Num = (int)NumberBox.Value;
                GlobalVariablesData.SaveConfig();
            }
        }

        private void NumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CanChange)
            {
                int get = NumberBox.Text.ToInt32(-1);
                if (get != -1)
                {
                    Log.Debug("文本输入有效: {Value}", get);
                    NumberBox.Value = get;
                }
                else
                {
                    Log.Warning("文本输入无效，重置为名单数量");
                    NumberBox.Value = GlobalVariablesData.config.AllSettings.Name.Count;
                    NumberBox.Text = null;
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("重置数字范围为名单数量: {Count}", GlobalVariablesData.config.AllSettings.Name.Count);
            NumberBox.Value = GlobalVariablesData.config.AllSettings.Name.Count;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var flicker = FindResource("flicker") as Storyboard;
                flicker.Begin();
                _speechSynthesizer.SpeakAsyncCancelAll();
                StartButton.IsEnabled = false;
                var jumpStoryBoard = FindResource("JumpStoryBoard") as Storyboard;

                if (StartButton.Content.ToString() == "开始")
                {
                    Log.Information("开始数字模式抽取，范围: 1-{Num}", GlobalVariablesData.config.NumberModeSettings.Num);
                    timer.Interval = GlobalVariablesData.config.NumberModeSettings.Speed;
                    FinishText.Visibility = Visibility.Hidden;
                    NowNumberText.Visibility = Visibility.Visible;
                    StartButton.Content = "结束";
                    jumpStoryBoard.Begin();
                    timer.Start();
                    StartButton.IsEnabled = true;
                }
                else
                {
                    Log.Information("结束数字模式抽取");
                    StartButton.Content = "开始";
                    jumpStoryBoard.Stop();
                    jumpStoryBoard.Remove();
                    timer.Stop();
                    string get = NowNumberText.Text;
                    FinishText.Text = get;
                    NowNumberText.Visibility = Visibility.Hidden;
                    FinishText.Visibility = Visibility.Visible;

                    if (GlobalVariablesData.config.NumberModeSettings.Speak)
                    {
                        Log.Debug("语音播报数字: {Number}", get);
                        _speechSynthesizer.SpeakAsync(get);
                    }

                    GlobalVariablesData.config.NumberModeSettings.LastName = get;
                    StartButton.IsEnabled = true;
                    Log.Information("数字模式抽取结果: {Number}", get);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "NumberMode开始/结束操作时发生异常");
                StartButton.IsEnabled = true;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("NumberMode页面加载");

                CanChange = false;
                SpeakCheck.IsChecked = GlobalVariablesData.config.NumberModeSettings.Speak;
                NumberBox.Value = GlobalVariablesData.config.NumberModeSettings.Num;
                CanChange = true;
                timer.Elapsed += Timer_Elapsed;

                if (GlobalVariablesData.config.NumberModeSettings.Speed == 0)
                {
                    Log.Debug("重置速度为默认值20");
                    GlobalVariablesData.config.NumberModeSettings.Speed = 20;
                }

                if (!GlobalVariablesData.config.AllSettings.SystemSpeech)
                {
                    _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                    _speechSynthesizer.Volume = GlobalVariablesData.config.AllSettings.Volume;
                    _speechSynthesizer.Rate = GlobalVariablesData.config.AllSettings.Speed;
                    Log.Debug("配置语音合成器: 性别=Female, 音量={Volume}, 语速={Speed}",
                        GlobalVariablesData.config.AllSettings.Volume,
                        GlobalVariablesData.config.AllSettings.Speed);
                }

                if (GlobalVariablesData.config.NumberModeSettings.Locked)
                {
                    Log.Debug("页面设置为锁定状态");
                    SpeakCheck.IsEnabled = false;
                    NumberBox.IsEnabled = false;
                    Button1.IsEnabled = false;
                }

                if (GlobalVariablesData.config.NumberModeSettings.LastName != null)
                {
                    NowNumberText.Text = GlobalVariablesData.config.NumberModeSettings.LastName;
                    Log.Debug("设置上次抽取结果: {LastName}", GlobalVariablesData.config.NumberModeSettings.LastName);
                }

                NowNumberText.Foreground = GlobalVariablesData.config.AllSettings.color;
                FinishText.Foreground = GlobalVariablesData.config.AllSettings.color;
                //NowNumberText.FontFamily = GlobalVariablesData.config.AllSettings.Font;
                //FinishText.FontFamily = GlobalVariablesData.config.AllSettings.Font;

                Log.Information("NumberMode页面加载完成，数字范围: 1-{Num}", GlobalVariablesData.config.NumberModeSettings.Num);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "NumberMode页面加载时发生异常");
            }
        }
    }
}