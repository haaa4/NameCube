using Masuit.Tools;
using Masuit.Tools.DateTimeExt;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.UI.Xaml.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using Wpf.Ui.Violeta.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Button = Wpf.Ui.Controls.Button;
using ContentDialog = Windows.UI.Xaml.Controls.ContentDialog;
using ContentDialogResult = Wpf.Ui.Controls.ContentDialogResult;
using MessageBox = System.Windows.MessageBox;
using StackPanel = System.Windows.Controls.StackPanel;
using TextBlock = Wpf.Ui.Controls.TextBlock;
using TextBox = Wpf.Ui.Controls.TextBox;

namespace NameCube.Mode
{
    /// <summary>
    /// MemoryMode.xaml 的交互逻辑
    /// </summary>
    public partial class MemoryMode : System.Windows.Controls.Page
    {
        public ObservableCollection<string> AllFiles { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> AllNames { get; set; } = new ObservableCollection<string>();
        bool CanChange;
        public System.Timers.Timer timer = new System.Timers.Timer();
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        int now = 0;

        public MemoryMode()
        {
            InitializeComponent();
            DataContext = this;
            Log.Debug("MemoryMode页面初始化完成");
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string text = AllNames[now];
                this.Dispatcher.Invoke(new Action(() =>
                {
                    NowNumberText.Text = text;
                }));
                now++;
                if (now == AllNames.Count)
                {
                    now = 0;
                    Log.Verbose("记忆模式轮播重置到开头");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MemoryMode定时器处理时发生异常");
            }
        }

        private void SpeechCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                bool newValue = SpeakCheck.IsChecked.Value;
                Log.Debug("语音播报开关: {Value}", newValue);
                GlobalVariables.json.MemoryModeSettings.Speak = newValue;
                GlobalVariables.SaveJson();
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _speechSynthesizer.SpeakAsyncCancelAll();
                StartButton.IsEnabled = false;
                var jumpStoryBoard = FindResource("JumpStoryBoard") as Storyboard;

                if (StartButton.Content.ToString() == "开始")
                {
                    Log.Information("开始记忆模式抽取");

                    if (AllNames.Count == 0)
                    {
                        Log.Warning("名单为空，重新初始化");
                        SnackBarFunction.ShowSnackBarInMainWindow("名单已完成，将删除", ControlAppearance.Primary);
                        if (AllFiles.Count <= 1)
                        {
                            foreach (string name in GlobalVariables.json.AllSettings.Name)
                            {
                                AllNames.Add(name);
                            }
                            string filename = DateTime.Now.GetTotalMilliseconds().ToString() + ".json";
                            File.WriteAllText(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", filename), JsonConvert.SerializeObject(AllNames));
                            Log.Information("创建临时名单文件: {Filename}", filename);
                        }
                        Button_Click(sender, e);
                        return;
                    }

                    timer.Interval = GlobalVariables.json.MemoryModeSettings.Speed;
                    FinishText.Visibility = Visibility.Hidden;
                    NowNumberText.Visibility = Visibility.Visible;
                    ChangeButton.IsEnabled = false;
                    DelButton.IsEnabled = false;
                    StartButton.Content = "结束";
                    jumpStoryBoard.Begin();
                    timer.Start();
                    StartButton.IsEnabled = true;
                    now = 0;

                    Log.Debug("记忆模式开始，轮播间隔: {Speed}ms", GlobalVariables.json.MemoryModeSettings.Speed);
                }
                else
                {
                    Log.Information("结束记忆模式抽取");
                    StartButton.Content = "开始";
                    jumpStoryBoard.Stop();
                    jumpStoryBoard.Remove();
                    timer.Stop();
                    string get = NowNumberText.Text;
                    FinishText.Text = get;
                    NowNumberText.Visibility = Visibility.Hidden;
                    FinishText.Visibility = Visibility.Visible;

                    if (GlobalVariables.json.MemoryModeSettings.Speak)
                    {
                        Log.Debug("语音播报: {Name}", get);
                        _speechSynthesizer.SpeakAsync(get);
                    }

                    AllNames.Remove(get);
                    GlobalVariables.json.MemoryModeSettings.LastName = get;

                    string path1 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "permanent", AllFiles[ComboBox.SelectedIndex]);
                    string path2 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", AllFiles[ComboBox.SelectedIndex]);

                    if (File.Exists(path1))
                    {
                        File.WriteAllText(path1, JsonConvert.SerializeObject(AllNames));
                        Log.Debug("更新永久名单文件: {Filename}", AllFiles[ComboBox.SelectedIndex]);
                    }
                    else
                    {
                        File.WriteAllText(path2, JsonConvert.SerializeObject(AllNames));
                        Log.Debug("更新临时名单文件: {Filename}", AllFiles[ComboBox.SelectedIndex]);
                    }

                    if (AllNames.Count == 0)
                    {
                        Log.Information("名单已完成");
                        SnackBarFunction.ShowSnackBarInMainWindow("名单已完成，将删除", ControlAppearance.Primary);
                        if (AllFiles.Count <= 1)
                        {
                            foreach (string name in GlobalVariables.json.AllSettings.Name)
                            {
                                AllNames.Add(name);
                            }
                            string filename = DateTime.Now.GetTotalMilliseconds().ToString() + ".json";
                            File.WriteAllText(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", filename), JsonConvert.SerializeObject(AllNames));
                            Log.Debug("创建新临时名单文件: {Filename}", filename);
                        }
                        Button_Click(sender, e);
                    }

                    StartButton.IsEnabled = true;
                    ChangeButton.IsEnabled = true;
                    DelButton.IsEnabled = true;
                    Log.Information("抽取结果: {Name}", get);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MemoryMode开始/结束操作时发生异常");
                StartButton.IsEnabled = true;
            }
        }

        bool Canchange;

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (Canchange)
                {
                    if (AllFiles.Count == 0)
                    {
                        Log.Warning("没有可用名单文件");
                        AllNames.Clear();
                        StartButton.IsEnabled = false;
                        ChangeButton.IsEnabled = false;
                        DelButton.IsEnabled = false;
                    }
                    else
                    {
                        StartButton.IsEnabled = true;
                        ChangeButton.IsEnabled = true;
                        DelButton.IsEnabled = true;
                        if (ComboBox.SelectedIndex == -1)
                        {
                            ComboBox.SelectedIndex = 0;
                        }

                        string selectedFile = AllFiles[ComboBox.SelectedIndex];
                        Log.Debug("选择名单文件: {Filename}", selectedFile);

                        string path1 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "permanent", selectedFile);
                        string path2 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", selectedFile);
                        string jsonstring;
                        now = 0;

                        if (File.Exists(path1))
                        {
                            jsonstring = File.ReadAllText(path1);
                            ChangeButton.IsEnabled = false;
                            Log.Debug("加载永久名单文件");
                        }
                        else
                        {
                            jsonstring = File.ReadAllText(path2);
                            if (!GlobalVariables.json.MemoryModeSettings.Locked)
                            {
                                ChangeButton.IsEnabled = true;
                            }
                            Log.Debug("加载临时名单文件");
                        }

                        AllNames.Clear();
                        var newNames = JsonConvert.DeserializeObject<ObservableCollection<string>>(jsonstring);
                        foreach (var name in newNames)
                        {
                            AllNames.Add(name);
                        }

                        Log.Information("加载名单文件完成: {Filename}, 姓名数量: {Count}", selectedFile, newNames?.Count ?? 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "切换名单文件时发生异常");
            }
        }

        private async void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("开始修改名单文件名");
                var dialog = new Wpf.Ui.Controls.ContentDialog()
                {
                    CloseButtonText = "取消",
                    PrimaryButtonText = "确定",
                    Title = "输入保存名",
                    Content = new Wpf.Ui.Controls.StackPanel
                    {
                        Children =
                        {
                            new TextBox()
                            {
                                Foreground=Brushes.Black,
                                PlaceholderText="请输入名单名字:",
                                Name="InputTextBox"
                            },
                        }
                    }
                };
                dialog.DialogHost = RootContentDialogPresenter;
                ContentDialogResult contentDialogResult = await dialog.ShowAsync();

                if (dialog.Content is Wpf.Ui.Controls.StackPanel panel)
                {
                    var InputTextBox = panel.Children.OfType<TextBox>().FirstOrDefault();
                    if (contentDialogResult == ContentDialogResult.None)
                    {
                        Log.Debug("取消修改名单文件名");
                        dialog.Hide();
                    }
                    else
                    {
                        if (InputTextBox.Text != null && !string.IsNullOrWhiteSpace(InputTextBox.Text))
                        {
                            try
                            {
                                Canchange = false;
                                ChangeButton.IsEnabled = false;
                                string path1 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "permanent", InputTextBox.Text + ".json");
                                string path2 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", AllFiles[ComboBox.SelectedIndex]);

                                Log.Information("重命名文件: {OldName} -> {NewName}", AllFiles[ComboBox.SelectedIndex], InputTextBox.Text + ".json");

                                File.Move(path2, path1);
                                AllFiles[ComboBox.SelectedIndex] = InputTextBox.Text + ".json";
                                dialog.Hide();
                                Canchange = true;
                                ComboBox.SelectedIndex = 0;
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "修改名单文件名失败");
                                MessageBoxFunction.ShowMessageBoxError(ex.Message);
                            }
                        }
                        else
                        {
                            Log.Warning("输入的文件名为空");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "修改名单文件时发生异常");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBox.SelectedIndex < 0 || ComboBox.SelectedIndex >= AllFiles.Count)
                {
                    Log.Warning("无效的文件选择索引: {Index}", ComboBox.SelectedIndex);
                    return;
                }

                string fileToDelete = AllFiles[ComboBox.SelectedIndex];
                Log.Information("删除名单文件: {Filename}", fileToDelete);

                string path1 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "permanent", fileToDelete);
                string path2 = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", fileToDelete);

                try
                {
                    if (File.Exists(path1))
                    {
                        File.Delete(path1);
                        Log.Debug("删除永久文件: {Path}", path1);
                    }
                    else
                    {
                        File.Delete(path2);
                        Log.Debug("删除临时文件: {Path}", path2);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "删除文件失败");
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                    return;
                }

                AllFiles.RemoveAt(ComboBox.SelectedIndex);
                if (AllFiles.Count > 0)
                {
                    ComboBox.SelectedIndex = 0;
                }
                else
                {
                    ComboBox.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "删除名单文件时发生异常");
                MessageBoxFunction.ShowMessageBoxError(ex.Message);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Information("创建新的临时名单");
                List<string> NameList = new List<string>();
                foreach (string name in GlobalVariables.json.AllSettings.Name)
                {
                    NameList.Add(name);
                }

                string filename = DateTime.Now.ToString("yyyy_M_d_H_m_s ") + ".json";
                string filePath = Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", filename);

                File.WriteAllText(filePath, JsonConvert.SerializeObject(NameList));

                bool canAdd = true;
                foreach (string getFileName in AllFiles)
                {
                    if (getFileName == filename) canAdd = false;
                }

                if (canAdd)
                {
                    AllFiles.Insert(0, filename);
                    ComboBox.SelectedIndex = 0;
                    Log.Information("创建新名单成功: {Filename}", filename);
                }
                else
                {
                    Log.Warning("创建过于频繁，文件名重复: {Filename}", filename);
                    SnackBarFunction.ShowSnackBarInMainWindow("创建过于频繁", ControlAppearance.Caution);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "创建新名单时发生异常");
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Log.Debug("MemoryMode页面加载");

                Directory.CreateDirectory(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "permanent"));
                Directory.CreateDirectory(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary"));

                string filename = DateTime.Now.ToString("yyyy_M_d_H_m_s ") + ".json";
                if (GlobalVariables.json.MemoryModeSettings.AutoAddFile)
                {
                    Log.Debug("自动创建名单文件");
                    foreach (string name in GlobalVariables.json.AllSettings.Name)
                    {
                        AllNames.Add(name);
                    }
                    File.WriteAllText(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary", filename), JsonConvert.SerializeObject(AllNames));
                    AllFiles.Add(filename);
                    Log.Information("自动创建名单文件: {Filename}", filename);
                }

                string[] fileNames1 = Directory.GetFiles(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "permanent"));
                string[] fileNames2 = Directory.GetFiles(Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryMode", "temporary"));

                Log.Debug("发现永久名单文件: {Count} 个", fileNames1.Length);
                Log.Debug("发现临时名单文件: {Count} 个", fileNames2.Length);

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

                AllNames.Clear();
                CanChange = false;
                SpeakCheck.IsChecked = GlobalVariables.json.MemoryModeSettings.Speak;
                CanChange = true;
                Canchange = true;
                timer.Elapsed += Timer_Elapsed;

                if (GlobalVariables.json.MemoryModeSettings.Speed == 0)
                {
                    Log.Debug("重置轮播速度为默认值20");
                    GlobalVariables.json.MemoryModeSettings.Speed = 20;
                }

                if (!GlobalVariables.json.AllSettings.SystemSpeech)
                {
                    _speechSynthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                    _speechSynthesizer.Volume = GlobalVariables.json.AllSettings.Volume;
                    _speechSynthesizer.Rate = GlobalVariables.json.AllSettings.Speed;
                    Log.Debug("配置语音合成器: 性别=Female, 音量={Volume}, 语速={Speed}",
                        GlobalVariables.json.AllSettings.Volume,
                        GlobalVariables.json.AllSettings.Speed);
                }

                if (GlobalVariables.json.MemoryModeSettings.Locked)
                {
                    Log.Debug("页面设置为锁定状态");
                    SpeakCheck.IsEnabled = false;
                    ChangeButton.IsEnabled = false;
                    DelButton.IsEnabled = false;
                }

                ComboBox.SelectedIndex = 0;
                if (AllFiles.Count == 0)
                {
                    Log.Warning("没有找到任何名单文件");
                    StartButton.IsEnabled = false;
                    ChangeButton.IsEnabled = false;
                    DelButton.IsEnabled = false;
                }

                NowNumberText.Foreground = GlobalVariables.json.AllSettings.color;
                FinishText.Foreground = GlobalVariables.json.AllSettings.color;
                NowNumberText.FontFamily = GlobalVariables.json.AllSettings.Font;
                FinishText.FontFamily = GlobalVariables.json.AllSettings.Font;

                if (GlobalVariables.json.MemoryModeSettings.LastName != null)
                {
                    NowNumberText.Text = GlobalVariables.json.MemoryModeSettings.LastName;
                    Log.Debug("设置上次抽取结果: {LastName}", GlobalVariables.json.MemoryModeSettings.LastName);
                }

                Log.Information("MemoryMode页面加载完成，总文件数: {FileCount}", AllFiles.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "MemoryMode页面加载时发生异常");
            }
        }
    }
}