using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessPages
{
    public partial class AudioPage : Page
    {
        private DispatcherTimer _progressTimer;
        private bool _isUserDragging = false;
        private int waitTimeInThisPage = 0;
        public event Action<string> RequestParentAction;
        private bool isDebug = false;

        private enum PlayerState
        {
            Stopped,
            Playing,
            Paused,
        }

        private PlayerState _currentState = PlayerState.Stopped;

        public AudioPage(string url, int waitTime, bool debug=false)
        {
            InitializeComponent();
            SpeedComboBox.SelectionChanged += SpeedComboBox_SelectionChanged;

            InitializeMediaPlayer(url);
            waitTimeInThisPage = waitTime;
            isDebug = debug;
        }
        public event Action<int> EndThePageAction;
        private void CallEndThePage(int ret = 0)
        {
            EndThePageAction?.Invoke(ret);
        }
        private void CallParentMethodDebug(string data)
        {
            RequestParentAction?.Invoke(data);
        }

        private TimeSpan _totalDuration;

        private void InitializeMediaPlayer(string url)
        {
            try
            {
                MediaPlayer.Source = new Uri(url, UriKind.Absolute);
                FileInfo fileInfo = new FileInfo(url);
                AudioNameText.Text = fileInfo.Name;

                _progressTimer = new DispatcherTimer();
                _progressTimer.Interval = TimeSpan.FromMilliseconds(100);
                _progressTimer.Tick += UpdateProgress;
                MediaPlayer.Play();
                _progressTimer.Start();
                _currentState = PlayerState.Playing;
            }
            catch (Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError("初始化失败", true, ex);
                if (isDebug)
                {
                    CallParentMethodDebug($"初始化失败: {ex.Message}");
                }
                else
                {
                    CallEndThePage();
                }

            }
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                PositionSlider.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            }
            PositionText.Text = FormatTime(TimeSpan.Zero);

            MediaPlayer.SpeedRatio = 1.0;
            if (MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                _totalDuration = MediaPlayer.NaturalDuration.TimeSpan;

                TotalTimeText.Text = FormatTime(_totalDuration);
            }
            else
            {
                TotalTimeText.Text = "N/A";
            }
        }

        private void UpdateProgress(object sender, EventArgs e)
        {
            if (
                !_isUserDragging
                && MediaPlayer.NaturalDuration.HasTimeSpan
                && _currentState != PlayerState.Stopped
            )
            {
                PositionSlider.Value = MediaPlayer.Position.TotalSeconds;
                PositionText.Text = FormatTime(MediaPlayer.Position);
            }
        }

        private string FormatTime(TimeSpan time)
        {
            return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        private void PositionSlider_ValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e
        )
        {
            if (!MediaPlayer.NaturalDuration.HasTimeSpan)
                return;

            var slider = sender as Slider;
            if (slider == null)
                return;

            if (slider.IsMouseCaptureWithin)
            {
                _isUserDragging = true;
                PositionText.Text = FormatTime(TimeSpan.FromSeconds(e.NewValue));

                MediaPlayer.Position = TimeSpan.FromSeconds(e.NewValue);

                if (_currentState == PlayerState.Playing)
                {
                    MediaPlayer.Play();
                }

                PositionText.Text = FormatTime(MediaPlayer.Position);
            }
        }

        private void SpeedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MediaPlayer == null)
                return;

            switch (SpeedComboBox.SelectedIndex)
            {
                case 0:
                    MediaPlayer.SpeedRatio = 0.5;
                    break;
                case 1:
                    MediaPlayer.SpeedRatio = 1.0;
                    break;
                case 2:
                    MediaPlayer.SpeedRatio = 1.5;
                    break;
                case 3:
                    MediaPlayer.SpeedRatio = 2.0;
                    break;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentState == PlayerState.Paused || _currentState == PlayerState.Stopped)
            {
                MediaPlayer.Play();
                _progressTimer.Start();
                _currentState = PlayerState.Playing;
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentState == PlayerState.Playing)
            {
                MediaPlayer.Pause();
                _progressTimer.Stop();
                _currentState = PlayerState.Paused;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            _progressTimer.Stop();
            MediaPlayer.Position = TimeSpan.Zero;
            PositionSlider.Value = 0;
            PositionText.Text = FormatTime(TimeSpan.Zero);
            _currentState = PlayerState.Stopped;
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            _progressTimer.Stop();
            MediaPlayer.Position = TimeSpan.Zero;
            PositionSlider.Value = 0;
            PositionText.Text = FormatTime(TimeSpan.Zero);
            _currentState = PlayerState.Stopped;

            if (waitTimeInThisPage == 0)
            {
                if (isDebug)
                {
                    CallParentMethodDebug("音频播放结束，自动关闭");
                }
                else
                {
                    CallEndThePage();
                }
            }
            else
            {
                Task.Delay(100)
                    .ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            MediaPlayer.Play();
                            _progressTimer.Start();
                            _currentState = PlayerState.Playing;
                        });
                    });
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (waitTimeInThisPage != 0)
            {
                await Task.Delay(waitTimeInThisPage * 1000);
                if (isDebug)
                {
                    CallParentMethodDebug("等待时间已到");
                }
                else
                {
                    CallEndThePage();
                }
            }
        }

        private void PositionSlider_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isUserDragging = false;
        }
    }
}
