using Masuit.Tools;
using Masuit.Tools.DateTimeExt;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
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
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace NameCube.Mode
{
    /// <summary>
    /// MemoryMode.xaml 的交互逻辑
    /// </summary>
    public partial class MemoryMode : Page
    {
        public ObservableCollection<string> AllFiles { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> AllNames { get; set; } = new ObservableCollection<string>();
        bool CanChange;
        public System.Timers.Timer timer = new System.Timers.Timer();
        Random Random;
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        int now = 0;
        public MemoryMode()
        {
            InitializeComponent();
            DataContext = this;
            Directory.CreateDirectory(Path.Combine(GlobalVariables.configDir ,"Mode_data" , "MemoryMode" , "permanent"));
            Directory.CreateDirectory(Path.Combine(GlobalVariables.configDir , "Mode_data" , "MemoryMode" , "temporary"));
            string filename = DateTime.Now.ToString("yyyy_M_d_H_m_s ") + ".json";
            if (GlobalVariables.json.MemoryModeSettings.AutoAddFile)
            {
                foreach (string name in GlobalVariables.json.AllSettings.Name)
                {
                    AllNames.Add(name);
                }
                File.WriteAllText(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", filename), JsonConvert.SerializeObject(AllNames));
                AllFiles.Add(filename);
            }          
            string[] fileNames1 = Directory.GetFiles(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "permanent"));
            string[] fileNames2 = Directory.GetFiles(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary"));
            foreach (string file in fileNames1)
            {
                FileInfo fileInfo = new FileInfo(file);
                AllFiles.Add(fileInfo.Name);
            }
            foreach (string file2 in fileNames2)
            {
                FileInfo fileInfo = new FileInfo(file2);
                if (fileInfo.Name != filename)
                {
                    
                    AllFiles.Add(fileInfo.Name);
                }

            }
            AllNames = new ObservableCollection<string>();
            CanChange = false;
            SpeakCheck.IsChecked = GlobalVariables.json.MemoryModeSettings.Speak;
            CanChange = true;
            Canchange = true;
            timer.Elapsed += Timer_Elapsed;
            if (GlobalVariables.json.MemoryModeSettings.Speed == 0)
            {
                GlobalVariables.json.MemoryModeSettings.Speed = 20;
            }
            if (!GlobalVariables.json.AllSettings.SystemSpeech)
            {
                _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                _speechSynthesizer.Volume = GlobalVariables.json.AllSettings.Volume;
                _speechSynthesizer.Rate = GlobalVariables.json.AllSettings.Speed;
            }
            if (GlobalVariables.json.MemoryModeSettings.Locked)
            {
                SpeakCheck.IsEnabled = false;
                ChangeButton.IsEnabled=false;
                DelButton.IsEnabled=false;
            }
            ComboBox.SelectedIndex= 0;
            if(AllFiles.Count==0)
            {
                StartButton.IsEnabled = false;
                ChangeButton.IsEnabled = false;
                DelButton.IsEnabled = false;
            }
            NowNumberText.Foreground = GlobalVariables.json.AllSettings.color;
            FinishText.Foreground = GlobalVariables.json.AllSettings.color;
            NowNumberText.FontFamily = GlobalVariables.json.AllSettings.Font;
            FinishText.FontFamily = GlobalVariables.json.AllSettings.Font;
        }


        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string text = AllNames[now];
            this.Dispatcher.Invoke(new Action(() =>
            {
                NowNumberText.Text = text;
            }));
            now++;
            if(now==AllNames.Count)
            {
                now = 0;
            }
        }

        private void SpeechCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariables.json.MemoryModeSettings.Speak = SpeakCheck.IsChecked.Value;
                GlobalVariables.SaveJson();
            }
        }

      

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _speechSynthesizer.SpeakAsyncCancelAll();
            StartButton.IsEnabled = false;
            if (StartButton.Content.ToString() == "开始")
            {
                timer.Interval = GlobalVariables.json.MemoryModeSettings.Speed;
                FinishText.Visibility = Visibility.Hidden;
                NowNumberText.Visibility = Visibility.Visible;
                ChangeButton.IsEnabled = false;
                DelButton.IsEnabled = false;
                StartButton.Content = "结束";
                timer.Start();
                StartButton.IsEnabled = true;
                now = 0;
            }
            else
            {
                StartButton.Content = "开始";
                timer.Stop();
                string get = NowNumberText.Text;
                FinishText.Text = get;
                NowNumberText.Visibility = Visibility.Hidden;
                FinishText.Visibility = Visibility.Visible;
                if (GlobalVariables.json.MemoryModeSettings.Speak)
                {
                    _speechSynthesizer.SpeakAsync(get);
                }
                AllNames.Remove(get);
                string path1 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "permanent", AllFiles[ComboBox.SelectedIndex]);
                string path2 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", AllFiles[ComboBox.SelectedIndex]);
                if(File.Exists(path1))
                {
                    File.WriteAllText(path1, JsonConvert.SerializeObject(AllNames));
                }
                else
                {
                    File.WriteAllText(path2, JsonConvert.SerializeObject(AllNames));
                }
                if(AllNames.Count==0)
                {
                    MessageBoxFunction.ShowMessageBoxInfo("名单已完成，将删除");
                    if(AllFiles.Count<=1)
                    {
                        foreach (string name in GlobalVariables.json.AllSettings.Name)
                        {
                            AllNames.Add(name);
                        }
                        string filename = DateTime.Now.GetTotalMilliseconds().ToString() + ".json";
                        File.WriteAllText(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", filename), JsonConvert.SerializeObject(AllNames));
                    }
                    Button_Click(sender, e);
                }
                StartButton.IsEnabled = true;
                ChangeButton.IsEnabled = true;
                DelButton.IsEnabled = true;
            }
        }
        bool Canchange;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Canchange)
            {
                if(AllFiles.Count==0)
                {
                    AllNames.Clear();
                    StartButton.IsEnabled = false;
                    ChangeButton.IsEnabled = false;
                    DelButton.IsEnabled = false;
                }
                else
                {
                    StartButton.IsEnabled= true;
                    ChangeButton.IsEnabled= true;
                    DelButton.IsEnabled= true;
                    if (ComboBox.SelectedIndex == -1)
                    {
                        ComboBox.SelectedIndex = 0;
                    }
                    string path1 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "permanent", AllFiles[ComboBox.SelectedIndex]);
                    string path2 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", AllFiles[ComboBox.SelectedIndex]);
                    string jsonstring;
                    now = 0;
                    if (File.Exists(path1))
                    {
                        jsonstring = File.ReadAllText(path1);
                        ChangeButton.IsEnabled = false;
                    }
                    else
                    {
                        jsonstring = File.ReadAllText(path2);
                        if (!GlobalVariables.json.MemoryModeSettings.Locked)
                        {
                            ChangeButton.IsEnabled = true;
                        }

                    }
                    AllNames.Clear();
                    var newNames = JsonConvert.DeserializeObject<ObservableCollection<string>>(jsonstring);
                    foreach (var name in newNames)
                    {
                        AllNames.Add(name);
                    }
                }
                
            }
            
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog1.Visibility = Visibility.Visible;
            InputTextBox.Text = null;
        }

        private void ContentDialog1_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.None) 
            {
                ContentDialog1.Visibility = Visibility.Collapsed;
            }
            else
            {
                if(InputTextBox.Text!=null)
                {
                    try
                    {
                        Canchange = false;
                        ChangeButton.IsEnabled = false;
                        string path1 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "permanent", InputTextBox.Text + ".json");
                        string path2 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", AllFiles[ComboBox.SelectedIndex]);
                        File.Move(path2, path1);
                        AllFiles[ComboBox.SelectedIndex] = InputTextBox.Text + ".json";
                        ContentDialog1.Visibility = Visibility.Collapsed;
                        Canchange = true;
                        ComboBox.SelectedIndex = 0;
                    }
                    catch (Exception ex) 
                    {
                        MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
                string path1 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "permanent", AllFiles[ComboBox.SelectedIndex]);
                string path2 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", AllFiles[ComboBox.SelectedIndex]);
                try
                {
                    if (File.Exists(path1))
                    {
                        File.Delete(path1);
                    }
                    else
                    {
                        File.Delete(path2);
                    }
                }
                catch (Exception ex)
                {
                     MessageBoxFunction.ShowMessageBoxError(ex.Message);
                }
                AllFiles.Remove(AllFiles[ComboBox.SelectedIndex]);
                ComboBox.SelectedIndex = 0;

            

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            List<string> NameList= new List<string>();
            foreach(string name in GlobalVariables.json.AllSettings.Name)
            {
                NameList.Add(name);
            }
            string filename = DateTime.Now.ToString("yyyy_M_d_H_m_s ") + ".json";
            File.WriteAllText(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", filename), JsonConvert.SerializeObject(NameList));
            AllFiles.Insert(0, filename);
            ComboBox.SelectedIndex = 0;
        }
    }
}
