using AngleSharp.Css.Dom;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Serilog;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessPages
{
    /// <summary>
    /// ReadPage.xaml 的交互逻辑
    /// </summary>
    public partial class ReadPage : Page
    {
        private string _text;
        private int _time;
        private bool _read;
        private bool _debug;
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        public event Action<string> RequestParentAction;
        public event Action<int> EndThePageAction;

        private void CallEndThePage(int ret = 0)
        {
            Log.Debug("调用结束页面，返回码: {ReturnCode}", ret);
            EndThePageAction?.Invoke(ret);
        }

        private void CallParentMethodDebug(string data)
        {
            Log.Debug("调用父页面调试方法，数据: {Data}", data);
            RequestParentAction?.Invoke(data);
        }

        public ReadPage(string text, int time, bool read, bool debug = false, bool show = true)
        {
            Log.Information("初始化阅读页面 - 文本长度: {TextLength}, 时间: {Time}秒, 朗读: {Read}, 调试模式: {Debug}, 显示: {Show}",
                text?.Length ?? 0, time, read, debug, show);
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            _text = text;
            _time = time;
            _read = read;
            _debug = debug;
            if (!show)
            {
                Log.Debug("页面设置为不显示模式，自动触发加载");
                MainWindow_Loaded(null, null);
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Information("开始阅读页面加载");
            marqueeText.Text = _text;
            if (_read)
            {
                Log.Information("开始朗读文本，长度: {TextLength}", _text?.Length ?? 0);
                if (!GlobalVariables.json.AllSettings.SystemSpeech)
                {
                    _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                    _speechSynthesizer.Volume = GlobalVariables.json.AllSettings.Volume;
                    _speechSynthesizer.Rate = GlobalVariables.json.AllSettings.Speed;
                    Log.Debug("语音合成器设置 - 音量: {Volume}, 速度: {Speed}",
                        GlobalVariables.json.AllSettings.Volume, GlobalVariables.json.AllSettings.Speed);
                }
                _speechSynthesizer.SpeakAsync(_text);
            }
            else
            {
                Log.Debug("朗读功能已禁用");
            }

            Log.Information("等待 {Time} 秒", _time);
            await Task.Delay(_time * 1000);

            Log.Debug("取消所有语音合成");
            _speechSynthesizer.SpeakAsyncCancelAll();

            Log.Information("阅读完成");
            if (_debug)
            {
                CallParentMethodDebug("阅读完成，自动关闭");
            }
            else
            {
                CallEndThePage();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("页面卸载，取消语音合成");
            _speechSynthesizer.SpeakAsyncCancelAll();
        }
    }
}