using Masuit.Tools.Logging;
using Masuit.Tools.Win32;
using System.Speech.Synthesis;
using System.Windows;

namespace NameCube
{
    /// <summary>
    /// ToolboxWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ToolboxWindow
    {
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();

        public ToolboxWindow()
        {
            InitializeComponent();
            _speechSynthesizer.Volume = 100;
            _speechSynthesizer.Rate = 0;
            MemoryCheck.IsChecked = GlobalVariables.json.StartToDo.AlwaysCleanMemory;

        }





        private void ReadButton_Click(object sender, RoutedEventArgs e)
        {
            _speechSynthesizer.SpeakAsyncCancelAll();
            _speechSynthesizer.SpeakAsync(Read1.Text + Read2.Text);
        }

        private void MemoryCheck_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.json.StartToDo.AlwaysCleanMemory = MemoryCheck.IsChecked.Value;
            GlobalVariables.SaveJson();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LogManager.Info("开始内存清理......");
            Windows.ClearMemorySilent();
        }
    }
}
