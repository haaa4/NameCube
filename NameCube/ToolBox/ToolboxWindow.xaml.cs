using Masuit.Tools.Logging;
using Masuit.Tools.Win32;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Media.Animation;

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
            var showStoryBoard = FindResource("ShowStoryBoard") as Storyboard;
            showStoryBoard.Stop();
            showStoryBoard.Remove();
            border.Visibility = System.Windows.Visibility.Visible;
            showStoryBoard.Completed += (s, en) =>
            {
                border.Visibility = System.Windows.Visibility.Collapsed;
            };
            showStoryBoard.Begin();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AppFunction.Restart();
        }
    }
}
