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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Serilog;

namespace NameCube.ToolBox
{
    /// <summary>
    /// SpeechToolbox.xaml 的交互逻辑
    /// </summary>
    public partial class SpeechToolbox : Page
    {
        private static readonly ILogger _logger = Log.ForContext<SpeechToolbox>();
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();

        public SpeechToolbox()
        {
            InitializeComponent();
            _logger.Debug("语音工具箱页面初始化");

            if (!GlobalVariablesData.config.AllSettings.SystemSpeech)
            {
                _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                _speechSynthesizer.Volume = GlobalVariablesData.config.AllSettings.Volume;
                _speechSynthesizer.Rate = GlobalVariablesData.config.AllSettings.Speed;
                _logger.Debug("使用自定义语音合成器，音量: {Volume}, 语速: {Speed}",
                    GlobalVariablesData.config.AllSettings.Volume,
                    GlobalVariablesData.config.AllSettings.Speed);
            }
            else
            {
                _logger.Debug("使用系统语音合成器");
            }
        }

        private void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            string textToRead = Read1.Text + Read2.Text;
            _logger.Information("开始朗读文本，长度: {Length}", textToRead.Length);

            _speechSynthesizer.SpeakAsyncCancelAll();
            _speechSynthesizer.SpeakAsync(textToRead);

            _logger.Debug("朗读任务已启动");
        }

        private void Read1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Read2.Focus();
                _logger.Debug("焦点从Read1移动到Read2");
            }
        }

        private void Read2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                _logger.Debug("在Read2中按Enter键，触发朗读");
                ReadButton_Click(sender, e);
            }
        }
    }
}