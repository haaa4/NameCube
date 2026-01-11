using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Interop; // 添加这个命名空间
using Serilog;

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
        string musicPath = Path.Combine(GlobalVariables.configDir, "Music");
        private PlayerState _currentState = PlayerState.Stopped;

        public AudioPage(string url, int waitTime, bool debug = false, bool show = true)
        {
            Log.Information("初始化音频播放页面 - URL: {Url}, 等待时间: {WaitTime}, 调试模式: {Debug}, 显示: {Show}",
                url, waitTime, debug, show);
            try
            {
                InitializeComponent();
                SpeedComboBox.SelectionChanged += SpeedComboBox_SelectionChanged;

                // 初始化 MediaPlayer 而不是使用 MediaElement
                _mediaPlayer = new MediaPlayer();
                _mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
                _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
                try
                {
                    string fullPath = Path.Combine(musicPath, url);
                    Log.Debug("尝试加载音频文件: {FullPath}", fullPath);
                    InitializeMediaPlayer(fullPath);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "音频文件加载失败");
                    MessageBoxFunction.ShowMessageBoxError("音频文件加载失败", true, ex);
                    if (isDebug)
                    {
                        CallParentMethodDebug($"音频文件加载失败: {ex.Message}");
                    }
                    else
                    {
                        CallEndThePage();
                    }
                }
                waitTimeInThisPage = waitTime;
                isDebug = debug;
                if (!show)
                {
                    Log.Debug("页面设置为不显示模式，自动触发加载");
                    Page_Loaded(null, null);
                }
                Log.Information("音频播放页面初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "音频播放页面初始化失败");
                throw;
            }
        }

        // 添加 Dispose 方法释放资源
        public void Dispose()
        {
            Log.Debug("释放音频播放页面资源");
            try
            {
                if (_mediaPlayer != null)
                {
                    Log.Debug("停止并关闭MediaPlayer");
                    _mediaPlayer.Stop();
                    _mediaPlayer.Close();
                    _mediaPlayer = null;
                }

                if (_progressTimer != null)
                {
                    Log.Debug("停止进度计时器");
                    _progressTimer.Stop();
                    _progressTimer = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "释放音频播放资源时发生错误");
            }
        }

        public event Action<int> EndThePageAction;
        private void CallEndThePage(int ret = 0)
        {
            Log.Information("调用结束页面，返回码: {ReturnCode}", ret);
            this.Dispose();
            EndThePageAction?.Invoke(ret);
        }

        private void CallParentMethodDebug(string data)
        {
            Log.Debug("调用父页面调试方法，数据: {Data}", data);
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

                Log.Information("音频播放器初始化完成，开始播放: {FileName}", fileInfo.Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化媒体播放器失败");
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
            Log.Debug("媒体文件打开成功");
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
                Log.Debug("音频总时长: {TotalDuration}", _totalDuration);
            }
            else
            {
                TotalTimeText.Text = "N/A";
                Log.Warning("无法获取音频总时长");
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
                Log.Debug("用户拖动进度条到: {NewValue}秒", e.NewValue);
            }
        }

        private void SpeedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_mediaPlayer == null)
                return;

            double oldSpeed = _mediaPlayer.SpeedRatio;
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

            Log.Debug("播放速度从 {OldSpeed} 修改为 {NewSpeed}", oldSpeed, _mediaPlayer.SpeedRatio);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentState == PlayerState.Paused || _currentState == PlayerState.Stopped)
            {
                Log.Information("用户点击播放按钮");
                _mediaPlayer.Play();
                _progressTimer.Start();
                _currentState = PlayerState.Playing;
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentState == PlayerState.Playing)
            {
                Log.Information("用户点击暂停按钮");
                _mediaPlayer.Pause();
                _progressTimer.Stop();
                _currentState = PlayerState.Paused;
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("用户点击停止按钮");
            _mediaPlayer.Stop();
            _progressTimer.Stop();
            _mediaPlayer.Position = TimeSpan.Zero;
            PositionSlider.Value = 0;
            PositionText.Text = FormatTime(TimeSpan.Zero);
            _currentState = PlayerState.Stopped;
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            Log.Information("音频播放结束");
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
                Log.Debug("等待时间 {WaitTime} 秒后重新播放", waitTimeInThisPage);
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
            Log.Debug("音频页面加载完成");
            if (waitTimeInThisPage != 0)
            {
                Log.Information("开始等待 {WaitTime} 秒", waitTimeInThisPage);
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
            Log.Debug("音频页面卸载");
            this.Dispose();
        }
    }
}