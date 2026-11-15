// /*
//  * NameCube-<点名器>
//  * Copyright (C) 2025-2026 haaa4
//  *
//  * This program is free software: you can redistribute it and/or modify
//  * it under the terms of the GNU General Public License as published by
//  * the Free Software Foundation, either version 3 of the License, or
//  * (at your option) any later version.
//  *
//  * This program is distributed in the hope that it will be useful,
//  * but WITHOUT ANY WARRANTY

using NameCube.Function;
using NameCube.GlobalVariables.DataClass;
using Serilog; // 添加Serilog引用
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.Setting
{
    /// <summary>
    /// Speech.xaml 的交互逻辑
    /// </summary>
    public partial class Speech : Page
    {
        private static readonly ILogger _logger = Log.ForContext<Speech>(); // 添加Serilog日志实例
        private bool CanChange;

        // Edge 音色列表只在第一次用到时拉取，之后复用（点“刷新”才重新联网）
        private List<EdgeTtsVoice> _edgeVoices;
        private bool _loadingVoices;

        // 第一次用到时才拿单例（页面可能在语音引擎初始化之前就被打开）
        private ISpeechService _speechService;

        // 预加载名单音频时的取消令牌与运行标记（运行中同一个按钮变成“取消”）
        private CancellationTokenSource _preloadCts;
        private bool _preloading;

        public Speech()
        {
            InitializeComponent();
            _logger.Debug("Speech 页面初始化开始");

            AllSettings settings = GlobalVariablesData.config.AllSettings;

            CanChange = false;
            VolumeSlider.Value = settings.Volume;
            SpeedSlider.Value = settings.Speed + 10;
            SystemSpeechCheck.IsChecked = settings.SystemSpeech;
            CacheEnabledCheck.IsChecked = settings.TtsCacheEnabled ?? true;
            EngineComboBox.SelectedIndex = SpeechEngines.IsEdge(settings.TtsEngine) ? 1 : 0;
            UpdateEngineVisibility();
            CanChange = true;

            _logger.Information("语音设置加载完成，引擎: {Engine}, 音量: {Volume}, 语速: {Speed}, 系统语音: {SystemSpeech}, Edge 音色: {EdgeVoice}, 本地缓存: {TtsCache}",
                settings.TtsEngine,
                settings.Volume,
                settings.Speed,
                settings.SystemSpeech,
                settings.EdgeVoice,
                settings.TtsCacheEnabled);
        }

        private bool IsEdgeSelected => EngineComboBox.SelectedIndex == 1;

        /// <summary>
        /// 按当前选中的引擎切换面板：系统引擎显示系统相关选项，Edge 引擎显示音色选择。
        /// </summary>
        private void UpdateEngineVisibility()
        {
            bool edge = IsEdgeSelected;
            EdgePanel.Visibility = edge ? Visibility.Visible : Visibility.Collapsed;
            SystemPanel.Visibility = edge ? Visibility.Collapsed : Visibility.Visible;
            EdgeInfoBar.Visibility = edge ? Visibility.Visible : Visibility.Collapsed;

            // 系统引擎的“使用系统设置”开着时，语速/音量交给系统控制，滑块不可用；
            // Edge 引擎没有这个概念，滑块始终可用。
            bool slidersEnabled = edge || !SystemSpeechCheck.IsChecked.GetValueOrDefault();
            VolumeSlider.IsEnabled = slidersEnabled;
            SpeedSlider.IsEnabled = slidersEnabled;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.Volume = (int)VolumeSlider.Value;
                GlobalVariablesData.SaveConfig();
                _speechService?.ApplySettings(); // 立即生效，不用等下次打开模式页
                _logger.Debug("音量设置修改为: {Volume}", (int)VolumeSlider.Value);
            }
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.Speed = (int)SpeedSlider.Value - 10;
                GlobalVariablesData.SaveConfig();
                _speechService?.ApplySettings(); // 立即生效，不用等下次打开模式页
                _logger.Debug("语速设置修改为: {Speed}", (int)SpeedSlider.Value - 10);
            }
        }

        private void SystemSpeechCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.AllSettings.SystemSpeech = SystemSpeechCheck.IsChecked.Value;
                GlobalVariablesData.SaveConfig();
                _speechService?.ApplySettings(); // 立即生效，不用等下次打开模式页
                UpdateEngineVisibility();
                _logger.Information("系统语音设置修改为: {SystemSpeech}", SystemSpeechCheck.IsChecked.Value);
            }
        }

        /// <summary>
        /// 切换朗读引擎。切换后立刻重新初始化：系统引擎要建 SpeechSynthesizer，
        /// Edge 引擎则顺手把音色列表准备好，避免用户点“测试”时才发现没列表。
        /// </summary>
        private async void EngineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!CanChange)
            {
                return;
            }

            string engine = IsEdgeSelected ? SpeechEngines.Edge : SpeechEngines.System;
            GlobalVariablesData.config.AllSettings.TtsEngine = engine;
            GlobalVariablesData.SaveConfig();
            UpdateEngineVisibility();
            _logger.Information("朗读引擎切换为: {Engine}", engine);

            // 换引擎时把正在念的那句停掉，避免两套引擎的声音叠在一起
            _speechService?.CancelAll();
            _speechService?.EnsureInitialized();
            UpdateWarningBar();

            if (IsEdgeSelected)
            {
                UpdateCacheUi();
                await EnsureEdgeVoicesAsync(forceRefresh: false);
            }
            else if (!_speechService.IsAvailable)
            {
                _logger.Warning("系统语音不可用: {Error}", _speechService.LastError);
            }
        }

        // ---------------------------------------------------------------- 系统语音不可用提示

        /// <summary>
        /// 系统语音不可用时只提示、只禁用系统相关的控件，绝不动整页或 Edge 相关设置。
        /// </summary>
        private void UpdateWarningBar()
        {
            bool show = !IsEdgeSelected
                && _speechService != null
                && !_speechService.IsAvailable;

            WarningBar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            UseEdgeButton.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>系统语音不可用时的一键补救：直接切到 Edge 引擎。</summary>
        private void UseEdgeButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEdgeSelected)
            {
                return;
            }

            // 改选定项会走 EngineComboBox_SelectionChanged，那里负责保存配置并准备音色列表
            EngineComboBox.SelectedIndex = 1;
            _logger.Information("系统语音不可用，已一键切换到 Edge 朗读");
        }

        // ---------------------------------------------------------------- Edge 音色

        /// <summary>
        /// 确保音色列表可用。第一次联网失败仍然会用内置音色兜底，保证下拉框永远有东西可选。
        /// </summary>
        private async Task EnsureEdgeVoicesAsync(bool forceRefresh)
        {
            if (_loadingVoices)
            {
                return;
            }

            if (!forceRefresh && _edgeVoices != null)
            {
                return;
            }

            _loadingVoices = true;
            RefreshVoicesButton.IsEnabled = false;
            EdgeVoiceComboBox.IsEnabled = false;
            VoiceStatusText.Text = "正在获取音色列表...";

            try
            {
                List<EdgeTtsVoice> voices = await EdgeTtsClient.GetVoicesAsync(CancellationToken.None);
                bool fromNetwork = voices.Count > 0;
                if (!fromNetwork)
                {
                    voices = EdgeTtsVoice.GetFallbackVoices();
                }

                _edgeVoices = voices;
                ApplyVoiceList(fromNetwork
                    ? $"已获取 {voices.Count} 个音色"
                    : $"联网失败，暂时使用 {voices.Count} 个内置音色（可点“刷新”重试）");
                _logger.Information("Edge 音色列表准备完成，来源: {Source}, 数量: {Count}",
                    fromNetwork ? "在线" : "内置", voices.Count);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "准备 Edge 音色列表时发生异常");
                _edgeVoices = EdgeTtsVoice.GetFallbackVoices();
                ApplyVoiceList("获取音色失败，已改用内置音色");
            }
            finally
            {
                _loadingVoices = false;
                RefreshVoicesButton.IsEnabled = true;
                EdgeVoiceComboBox.IsEnabled = true;
            }
        }

        /// <summary>
        /// 把音色列表灌进下拉框，并选中配置里保存的那个（若列表里没有就补一个占位项）。
        /// </summary>
        private void ApplyVoiceList(string status)
        {
            CanChange = false;
            try
            {
                string saved = GlobalVariablesData.config.AllSettings.EdgeVoice;
                List<EdgeTtsVoice> voices = _edgeVoices ?? EdgeTtsVoice.GetFallbackVoices();
                if (!string.IsNullOrWhiteSpace(saved) &&
                    voices.All(v => !string.Equals(v.ShortName, saved, StringComparison.OrdinalIgnoreCase)))
                {
                    voices.Add(new EdgeTtsVoice
                    {
                        ShortName = saved,
                        Locale = string.Empty,
                        Gender = string.Empty,
                        FriendlyName = saved,
                    });
                }

                EdgeVoiceComboBox.ItemsSource = voices;
                EdgeVoiceComboBox.DisplayMemberPath = nameof(EdgeTtsVoice.DisplayName);
                EdgeVoiceComboBox.SelectedItem =
                    voices.FirstOrDefault(v => string.Equals(v.ShortName, saved, StringComparison.OrdinalIgnoreCase))
                    ?? voices.FirstOrDefault();
                VoiceStatusText.Text = status;
            }
            finally
            {
                CanChange = true;
            }
        }

        private void EdgeVoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!CanChange || EdgeVoiceComboBox.SelectedItem is not EdgeTtsVoice voice)
            {
                return;
            }

            GlobalVariablesData.config.AllSettings.EdgeVoice = voice.ShortName;
            GlobalVariablesData.SaveConfig();
            _speechService?.ApplySettings();
            _logger.Information("Edge 音色切换为: {Voice}", voice.ShortName);
        }

        private async void RefreshVoicesButton_Click(object sender, RoutedEventArgs e)
        {
            await EnsureEdgeVoicesAsync(forceRefresh: true);
        }

        // ---------------------------------------------------------------- 本地缓存与预加载

        /// <summary>
        /// 刷新缓存统计与按钮可用状态（只改显示，不动配置）。
        /// </summary>
        private void UpdateCacheUi()
        {
            bool enabled = CacheEnabledCheck.IsChecked.GetValueOrDefault();
            var (count, bytes) = EdgeTtsCache.GetStats();

            CacheStatsText.Text = enabled
                ? $"已缓存 {count} 个音频，占用 {FormatSize(bytes)}"
                : $"缓存已关闭（本地仍有 {count} 个音频，{FormatSize(bytes)}；关闭后既不读取也不写入）";

            PreloadButton.IsEnabled = _preloading || enabled;
            CleanCacheButton.IsEnabled = !_preloading && count > 0;
            ClearCacheButton.IsEnabled = !_preloading && count > 0;
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024)
            {
                return $"{bytes} B";
            }

            if (bytes < 1024 * 1024)
            {
                return $"{bytes / 1024.0:0.#} KB";
            }

            return $"{bytes / 1024.0 / 1024.0:0.##} MB";
        }

        private void CacheEnabledCheck_Click(object sender, RoutedEventArgs e)
        {
            if (!CanChange)
            {
                return;
            }

            bool enabled = CacheEnabledCheck.IsChecked.GetValueOrDefault();
            GlobalVariablesData.config.AllSettings.TtsCacheEnabled = enabled;
            GlobalVariablesData.SaveConfig();
            UpdateCacheUi();
            CacheActionText.Text = enabled
                ? "打开后，合成过的音频会自动存到本地。"
                : "缓存关闭后，同一句话每次都会重新联网合成，磁盘上已有的音频不会自动删除。";
            _logger.Information("Edge 朗读本地缓存 {State}", enabled ? "已开启" : "已关闭");
        }

        /// <summary>
        /// 预加载名单里所有人名的音频。运行中再点一次就是取消。
        /// </summary>
        private async void PreloadButton_Click(object sender, RoutedEventArgs e)
        {
            // 正在跑：同一个按钮当“取消”用
            if (_preloading)
            {
                CacheActionText.Text = "正在取消预加载...";
                _preloadCts?.Cancel();
                return;
            }

            if (!CacheEnabledCheck.IsChecked.GetValueOrDefault())
            {
                return;
            }

            // 名单就是模式里用的那份（AllSettings.Name），这里只去掉空名字和重复项
            List<string> names = (GlobalVariablesData.config.AllSettings.Name ?? new List<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (names.Count == 0)
            {
                CacheActionText.Text = "名单是空的，没有需要预加载的名字";
                SnackBarFunction.ShowSnackBarInSettingWindow(
                    "名单是空的，先添加几个名字吧", Wpf.Ui.Controls.ControlAppearance.Caution);
                return;
            }

            AllSettings settings = GlobalVariablesData.config.AllSettings;
            string voice = string.IsNullOrWhiteSpace(settings.EdgeVoice)
                ? "zh-CN-XiaoxiaoNeural"
                : settings.EdgeVoice;
            int ratePercent = Math.Clamp(settings.Speed, -10, 10) * 10;

            _preloading = true;
            _preloadCts = new CancellationTokenSource();
            PreloadButton.Content = "取消预加载";
            UpdateCacheUi();

            var progress = new Progress<(int Done, int Total)>(p =>
            {
                CacheActionText.Text = p.Total == 0
                    ? "名单里没有需要预加载的名字"
                    : $"正在预加载 {p.Done}/{p.Total}...";
            });

            _logger.Information(
                "开始预加载名单音频：{Count} 个名字，音色 {Voice}，语速 {Rate}%",
                names.Count, voice, ratePercent);

            try
            {
                var (succeeded, failed, total) = await EdgeTtsClient.PreloadAsync(
                    names, voice, ratePercent, progress, _preloadCts.Token);

                CacheActionText.Text = failed == 0
                    ? $"预加载完成：{succeeded}/{total} 个名字已就绪"
                    : $"预加载完成：成功 {succeeded} 个，失败 {failed} 个（检查网络后可再试一次）";
                SnackBarFunction.ShowSnackBarInSettingWindow(
                    failed == 0 ? "名单音频已经准备好了" : $"有 {failed} 个名字没有合成成功",
                    failed == 0
                        ? Wpf.Ui.Controls.ControlAppearance.Success
                        : Wpf.Ui.Controls.ControlAppearance.Caution);
            }
            catch (OperationCanceledException)
            {
                CacheActionText.Text = "已取消预加载，已完成的音频仍然保存在本地";
                _logger.Information("预加载名单音频已取消");
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "预加载名单音频时发生异常");
                CacheActionText.Text = "预加载失败：" + ex.Message;
                SnackBarFunction.ShowSnackBarInSettingWindow(
                    "预加载失败，请检查网络连接", Wpf.Ui.Controls.ControlAppearance.Danger);
            }
            finally
            {
                _preloading = false;
                PreloadButton.Content = "预加载名单音频";
                _preloadCts?.Dispose();
                _preloadCts = null;
                UpdateCacheUi();
            }
        }

        private async void CleanCacheButton_Click(object sender, RoutedEventArgs e)
        {
            if (_preloading)
            {
                return;
            }

            if (!await ConfirmAsync("清理名单外缓存", "确定要删除不在当前名单里的本地音频吗？"))
            {
                return;
            }

            List<string> names = GlobalVariablesData.config.AllSettings.Name ?? new List<string>();
            int removed = EdgeTtsCache.RemoveTextsNotIn(names);
            UpdateCacheUi();
            CacheActionText.Text = removed == 0
                ? "没有需要清理的音频"
                : $"已清理 {removed} 个名单外的音频";
            SnackBarFunction.ShowSnackBarInSettingWindow(
                removed == 0 ? "没有需要清理的音频" : $"已清理 {removed} 个音频",
                Wpf.Ui.Controls.ControlAppearance.Success);
        }

        private async void ClearCacheButton_Click(object sender, RoutedEventArgs e)
        {
            if (_preloading)
            {
                return;
            }

            var (count, bytes) = EdgeTtsCache.GetStats();
            if (!await ConfirmAsync(
                    "清空本地缓存",
                    $"确定删除全部 {count} 个本地音频（{FormatSize(bytes)}）吗？之后朗读时会重新联网合成。"))
            {
                return;
            }

            int removed = EdgeTtsCache.RemoveAll();
            UpdateCacheUi();
            CacheActionText.Text = $"已删除 {removed} 个本地音频";
            SnackBarFunction.ShowSnackBarInSettingWindow(
                removed == 0 ? "本地没有可删除的音频" : $"已删除 {removed} 个音频",
                Wpf.Ui.Controls.ControlAppearance.Success);
        }

        /// <summary>
        /// 删除类操作弹一次确认框：优先用设置窗口自带的 ContentDialog（外观统一），
        /// 拿不到宿主窗口时退化成系统 MessageBox。
        /// </summary>
        private async Task<bool> ConfirmAsync(string title, string message)
        {
            try
            {
                if (Window.GetWindow(this) is SettingsWindow host && host.RootContentDialogPresenter != null)
                {
                    var dialog = new Wpf.Ui.Controls.ContentDialog
                    {
                        Title = title,
                        Content = message,
                        PrimaryButtonText = "确定",
                        CloseButtonText = "取消",
                    };
                    dialog.DialogHostEx = host.RootContentDialogPresenter;
                    return await dialog.ShowAsync() == Wpf.Ui.Controls.ContentDialogResult.Primary;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "弹出确认框失败，改用系统对话框");
            }

            return System.Windows.MessageBox.Show(
                       message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning)
                   == MessageBoxResult.Yes;
        }

        // ---------------------------------------------------------------- 系统语音设置入口

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("尝试打开系统朗读人设置");
            try
            {
                // 使用系统协议直接打开 "设置" 应用中的朗读人界面
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:easeofaccess-narrator", // Win10/11 专用 URI
                    UseShellExecute = true // 必须启用 Shell 执行
                });
                _logger.Debug("成功打开系统朗读人设置");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "无法打开系统朗读人设置界面");
                Console.WriteLine($"无法打开设置界面: {ex.Message}");
            }
        }

        /// <summary>
        /// 打开旧版「语音属性」面板：Win7 等系统没有 ms-settings: 协议，
        /// 语音引擎（SAPI）的设置项在控制面板里，也就是 sapi.cpl 这个控制面板项。
        /// </summary>
        private void LegacySpeechSettings_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("尝试打开旧版语音属性面板");

            // 32 位进程在 64 位系统上访问 System32 会被 WOW64 重定向到 SysWOW64，
            // 想拿到真正的 64 位 System32 只能走 Sysnative；x64 进程则直接用 System32。
            string windows = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string nativeSystem32 = Environment.Is64BitProcess
                ? Path.Combine(windows, "System32")
                : Path.Combine(windows, "Sysnative");

            // 依次尝试：本位数系统目录 → System32 / SysWOW64 里的副本（精简版系统可能只剩其一）
            var candidates = new[]
            {
                // (sapi.cpl 路径, 用来打开它的 control.exe)
                (Path.Combine(nativeSystem32, "Speech", "SpeechUX", "sapi.cpl"),
                 Path.Combine(nativeSystem32, "control.exe")),
                (Path.Combine(windows, "System32", "Speech", "SpeechUX", "sapi.cpl"), "control.exe"),
                (Path.Combine(windows, "SysWOW64", "Speech", "SpeechUX", "sapi.cpl"), "control.exe")
            };

            foreach (var (cpl, control) in candidates)
            {
                if (!File.Exists(cpl)) continue;

                // control.exe 缺失时退化为从 PATH 里找
                string exe = File.Exists(control) ? control : "control.exe";
                if (TryStart(exe, $"\"{cpl}\""))
                {
                    _logger.Debug("已通过 {Cpl} 打开语音属性", cpl);
                    return;
                }
            }

            // 回退：控制面板并没有公开的 canonical name，但部分系统上能识别，
            // 认不出来时 control.exe 会自己弹一个“找不到”的提示，不会崩
            if (TryStart("control.exe", "/name Microsoft.Speech"))
            {
                _logger.Debug("已通过 control.exe 打开语音属性");
                return;
            }

            _logger.Warning("未找到系统语音属性面板，已尝试 System32 / Sysnative / SysWOW64 下的 Speech\\SpeechUX\\sapi.cpl");
            SnackBarFunction.ShowSnackBarInMainWindow(
                "没有找到语音属性面板，这台电脑可能精简了语音组件",
                Wpf.Ui.Controls.ControlAppearance.Caution);
        }

        /// <summary>
        /// 启动一个外部程序，失败（找不到文件/被策略拦截）时返回 false 而不是抛异常。
        /// </summary>
        private bool TryStart(string fileName, string arguments)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "启动 {FileName} 失败", fileName);
                return false;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _speechService = SpeechService.Instance;
            _speechService.EnsureInitialized();

            // 缓存统计要读磁盘，放在页面加载完再算，构造函数里不做任何 IO
            UpdateCacheUi();
            CacheActionText.Text = CacheEnabledCheck.IsChecked.GetValueOrDefault()
                ? $"名单共 {CountNames()} 个名字；预加载会跳过已经缓存过的"
                : "本地缓存关闭时无法预加载（预加载的结果需要写进本地缓存）";

            UpdateWarningBar();

            if (IsEdgeSelected)
            {
                // Edge 引擎不依赖系统语音组件，这里只需要把音色列表准备好
                await EnsureEdgeVoicesAsync(forceRefresh: false);
                return;
            }

            if (!_speechService.IsAvailable)
            {
                // 系统语音不可用时不整页禁用：用户仍然可以切到 Edge 引擎继续用
                _logger.Warning("语音引擎不可用: {Error}", _speechService.LastError);
            }
        }

        /// <summary>当前名单里有效名字的数量（用于提示文字）。</summary>
        private static int CountNames()
        {
            return (GlobalVariablesData.config.AllSettings.Name ?? new List<string>())
                .Count(name => !string.IsNullOrWhiteSpace(name));
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // 服务内部已经吞掉了所有异常，这里不需要 try
                _speechService?.Speak(TestTextBox.Text);
            }
        }
    }
}
