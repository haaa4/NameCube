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

namespace NameCube.ToolBox
{
    /// <summary>
    /// SpeechToolbox.xaml 的交互逻辑
    /// </summary>
    
    public partial class SpeechToolbox : Page
    {
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        public SpeechToolbox()
        {
            InitializeComponent();
            if(!GlobalVariables.json.AllSettings.SystemSpeech)
            {
                _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                _speechSynthesizer.Volume = GlobalVariables.json.AllSettings.Volume;
                _speechSynthesizer.Rate = GlobalVariables.json.AllSettings.Speed;
            }
           
        }
        private void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            _speechSynthesizer.SpeakAsyncCancelAll();
            _speechSynthesizer.SpeakAsync(Read1.Text + Read2.Text);
        }

        private void Read1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { 
                e.Handled= true;
                Read2.Focus();
            }
        }

        private void Read2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
            {
                e.Handled = true;
                ReadButton_Click(sender, e);
            }
        }
    }
}
