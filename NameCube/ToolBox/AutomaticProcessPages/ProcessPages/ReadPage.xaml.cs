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
            EndThePageAction?.Invoke(ret);
        }
        private void CallParentMethodDebug(string data)
        {
            RequestParentAction?.Invoke(data);
        }
        public ReadPage(string text,int time,bool read,bool debug=false)
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            _text = text;
            _time = time;
            _read = read;
            _debug = debug;
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            marqueeText.Text = _text;
            if (_read)
            {
                if (!GlobalVariables.json.AllSettings.SystemSpeech)
                {
                    _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                    _speechSynthesizer.Volume = GlobalVariables.json.AllSettings.Volume;
                    _speechSynthesizer.Rate = GlobalVariables.json.AllSettings.Speed;
                }
                _speechSynthesizer.SpeakAsync(_text);
            }
            await Task.Delay(_time * 1000);
            _speechSynthesizer.SpeakAsyncCancelAll();
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
            _speechSynthesizer.SpeakAsyncCancelAll();
        }
    }
}
