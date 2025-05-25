using Masuit.Tools.Logging;
using Masuit.Tools.Win32;
using System.Speech.Synthesis;
using System.Windows;

namespace NameCube.ToolBox
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
            if (GlobalVariables.json.AllSettings.Top)
            {
                Topmost = true;
            }

        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationMenu.Navigate(typeof(ToolBox.SpeechToolbox));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AppFunction.Restart();
        }
    }
}
