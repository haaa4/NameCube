using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using static NameCube.ToolBox.AutomaticProcessPages.Audio;
using Path = System.IO.Path;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessSettingPages
{
    /// <summary>
    /// AudioSettingPag.xaml 的交互逻辑
    /// </summary>
    public partial class AudioSettingPage : Page
    {
        public string url;
        public int waitTime;
        bool canChange = false;
        List<string> AudioFamily = new List<string>();
        string musicPath = Path.Combine(GlobalVariables.configDir, "Music");
        public AudioSettingPage(ProcessData processData)
        {
            InitializeComponent();
            canChange = false;
            url = processData.stringData1;
            waitTime = (int)processData.doubleData;
            WaitTimeNumberBox.Value=waitTime;
            Loaded += AudioSettingPage_Loaded;

        }

        private void AudioSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            AudioFamily.Clear();
            string[] filenames = Directory.GetFiles(musicPath);
            foreach (string filename in filenames)
            {
                FileInfo fileInfo = new FileInfo(filename);
                if (
                    fileInfo.Extension == ".mp3"
                    || fileInfo.Extension == ".wma"
                    || fileInfo.Extension == ".wav"
                )
                {
                    AudioFamily.Add(fileInfo.FullName);
                    AudioComboBox.Items.Add(fileInfo.Name);
                }
            }
            int find=AudioFamily.IndexOf(url);
            if(find!=-1)
            {
                AudioComboBox.SelectedIndex = find;
            }
            else
            {
                url = null;
            }
            canChange = true;
        }

        private void WaitTimeNumberBox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            waitTime = (int)WaitTimeNumberBox.Value;
            WaitTimeNumberBox.Value = waitTime;
        }

        private void WaitTimeNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            int get = WaitTimeNumberBox.Text.ToInt32(-1);
            if(get!=-1)
            {
                waitTime = get;
            }
            else
            {
                waitTime = 0;
            }
            WaitTimeNumberBox.Value = waitTime;
        }

        private void AudioComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(canChange)
            {
                url= AudioFamily[AudioComboBox.SelectedIndex];
            }
        }
    }
}
