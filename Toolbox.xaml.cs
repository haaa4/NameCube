using Masuit.Tools.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Text.Json;
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

namespace NameCube
{
    /// <summary>
    /// Toolbox.xaml 的交互逻辑
    /// </summary>
    public partial class Toolbox : Page
    {
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        Json json= new Json();
        public void SaveJson()
        {
            string jsonString = JsonSerializer.Serialize(json);
            File.WriteAllText("config.json", jsonString);
        }
        public Toolbox()
        {
            InitializeComponent();
            _speechSynthesizer.Volume = 100;
            _speechSynthesizer.Rate = 0;
            string jsonstring = File.ReadAllText("config.json");
            json = JsonSerializer.Deserialize<Json>(jsonstring);
            MemoryCheck.IsChecked = json.AlwaysCleanMemory;
            
        }
    
    
        


        private void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            _speechSynthesizer.SpeakAsync(Read1.Text+Read2.Text);
        }

        private void MemoryCheck_Click(object sender, RoutedEventArgs e)
        {
            json.AlwaysCleanMemory = MemoryCheck.IsChecked.Value;
            SaveJson();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Windows.ClearMemorySilent();
        }
    }
}
