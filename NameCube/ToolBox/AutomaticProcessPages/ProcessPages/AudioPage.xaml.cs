using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Interop; // 添加这个命名空间

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessPages
{
    public partial class AudioPage : Page
    {
        private DispatcherTimer _progressTimer;
        private bool _isUserDragging = false;
        private int waitTimeInThisPage = 0;
        public event Action<string> RequestParentAction;
        private bool isDebug = false;
        private MediaPlayer _mediaPlayer; // 使用 MediaPlayer 替代 MediaElement

        private enum PlayerState
        {
            Stopped,
            Playing,
            Paused,
        }

        private PlayerState _currentState = PlayerState.Stopped;

        public AudioPage(string url, int waitTime, bool debug = false, bool show = true)
        {
            InitializeComponent();
            SpeedComboBox.SelectionChanged += SpeedComboBox_SelectionChanged;

            // 初始化 MediaPlayer 而不是使用 MediaElement
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            InitializeMediaPlayer(url);
            waitTimeInThisPage = waitTime;
            isDebug = debug;
            if(!show)
            {
                Page_Loaded(null,null);
            }
        }

        // 添加 Dispose 方法释放资源
        public void Dispose()
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Close();
                _mediaPlayer = null;
            }

            if (_progressTimer != null)
            {
                _progressTimer.Stop();
                _progressTimer = null;
            }
        }

        public event Action<int> EndThePageAction;
        private void CallEndThePage(int ret = 0)
        {
            this.Dispose();
            EndThePageAction?.Invoke(ret);
        }
        private void CallParentMethodDebug(string data)
        {
            this.Dispose();
            RequestParentAction?.Invoke(data);
        }

        private TimeSpan _totalDuration;

        private void InitializeMediaPlayer(string url)
        {
            try
            {
                // 使用 MediaPlayer 打开媒体文件
                _mediaPlayer.Open(new Uri(url, UriKind.Absolute));
                FileInfo fileInfo = new FileInfo(url);
                AudioNameText.Text = fileInfo.Name;

                _progressTimer = new DispatcherTimer();
                _progressTimer.Interval = TimeSpan.FromMilliseconds(100);
                _progressTimer.Tick += UpdateProgress;

                // 播放媒体
                _mediaPlayer.Play();
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

        private void MediaPlayer_MediaOpened(object sender, EventArgs e)
        {
            if (_mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                PositionSlider.Maximum = _mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            }
            PositionText.Text = FormatTime(TimeSpan.Zero);

            _mediaPlayer.SpeedRatio = 1.0;
            if (_mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                _totalDuration = _mediaPlayer.NaturalDuration.TimeSpan;
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
                && _mediaPlayer.NaturalDuration.HasTimeSpan
                && _currentState != PlayerState.Stopped
            )
            {
                PositionSlider.Value = _mediaPlayer.Position.TotalSeconds;
                PositionText.Text = FormatTime(_mediaPlayer.Position);
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
            if (!_mediaPlayer.NaturalDuration.HasTimeSpan)
                return;

            var slider = sender as Slider;
            if (slider == null)
                return;

            if (slider.IsMouseCaptureWithin)
            {
                _isUserDragging = true;
                PositionText.Text = FormatTime(TimeSpan.FromSeconds(e.NewValue));

                _mediaPlayer.Position = TimeSpan.FromSeconds(e.NewValue);

                if (_currentState == PlayerState.Playing)
                {
                    _mediaPlayer.Play();
                }

                PositionText.Text = FormatTime(_mediaPlayer.Position);
            }
        }

        private void SpeedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_mediaPlayer == null)
                return;

            switch (SpeedComboBox.SelectedIndex)
            {
                case 0:
                    _mediaPlayer.SpeedRatio = 0.5;
                    break;
                case 1:
                    _mediaPlayer.SpeedRatio = 1.0;
                    break;
                case 2:
                    _mediaPlayer.SpeedRatio = 1.5;
                    break;
                case 3:
                    _mediaPlayer.SpeedRatio = 2.0;
                    break;
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentState == PlayerState.Paused || _currentState == PlayerState.Stopped)
            {
                _mediaPlayer.Play();
                _progressTimer.Start();
                _currentState = PlayerState.Playing;
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentState == PlayerState.Playing)
            {
                _mediaPlayer.Pause();
                _progressTimer.Stop();
                _currentState = PlayerState.Paused;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Stop();
            _progressTimer.Stop();
            _mediaPlayer.Position = TimeSpan.Zero;
            PositionSlider.Value = 0;
            PositionText.Text = FormatTime(TimeSpan.Zero);
            _currentState = PlayerState.Stopped;
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            _progressTimer.Stop();
            _mediaPlayer.Position = TimeSpan.Zero;
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
                            _mediaPlayer.Play();
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

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Dispose();
        }
    }
}