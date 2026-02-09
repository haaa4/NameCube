using Masuit.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;
using static NameCube.ToolBox.AutomaticProcessPages.Audio;
using Path = System.IO.Path;
using NameCube.GlobalVariables.DataClass;
using Serilog;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessSettingPages
{
    /// <summary>
    /// AudioSettingPag.xaml 的交互逻辑
    /// </summary>
    public partial class AudioSettingPage : Page
    {
        public string url;
        public int waitTime;
        bool canChange = false;
        List<string> AudioFamily = new List<string>();
        string musicPath = Path.Combine(GlobalVariablesData.userDataDir, "Music");
        public AudioSettingPage(ProcessData processData)
        {
            Log.Information("初始化音频设置页面");
            try
            {
                InitializeComponent();
                canChange = false;
                url = processData.stringData1;
                waitTime = (int)processData.doubleData;
                WaitTimeNumberBox.Value = waitTime;

                Log.Debug("音频设置初始化 - URL: {Url}, 等待时间: {WaitTime}", url, waitTime);
                Loaded += AudioSettingPage_Loaded;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化音频设置页面时发生错误");
                throw;
            }
        }

        private void AudioSettingPage_Loaded(object sender, RoutedEventArgs e)
        {
            Log.Debug("音频设置页面加载完成，开始加载音频文件");
            try
            {
                AudioFamily.Clear();
                AudioComboBox.Items.Clear();

                if (!Directory.Exists(musicPath))
                {
                    Log.Warning("音乐目录不存在: {MusicPath}", musicPath);
                    Directory.CreateDirectory(musicPath);
                    Log.Information("已创建音乐目录");
                }

                string[] filenames = Directory.GetFiles(musicPath);
                Log.Debug("找到 {FileCount} 个文件在音乐目录中", filenames.Length);

                int audioCount = 0;
                foreach (string filename in filenames)
                {
                    FileInfo fileInfo = new FileInfo(filename);
                    if (
                        fileInfo.Extension == ".mp3"
                        || fileInfo.Extension == ".wma"
                        || fileInfo.Extension == ".wav"
                    )
                    {
                        AudioFamily.Add(fileInfo.Name);
                        AudioComboBox.Items.Add(fileInfo.Name);
                        audioCount++;
                    }
                }

                Log.Information("成功加载 {AudioCount} 个音频文件到下拉列表", audioCount);

                int find = AudioFamily.IndexOf(url);
                if (find != -1)
                {
                    AudioComboBox.SelectedIndex = find;
                    Log.Debug("已选择音频文件: {FileName}，索引: {Index}", url, find);
                }
                else
                {
                    if (!string.IsNullOrEmpty(url))
                    {
                        Log.Warning("音频文件在列表中未找到: {Url}，将清空选择", url);
                    }
                    url = null;
                }

                canChange = true;
                Log.Debug("音频设置页面加载完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载音频文件列表时发生错误");
            }
        }

        private void WaitTimeNumberBox_ValueChanged(object sender, Wpf.Ui.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (canChange)
            {
                int oldWaitTime = waitTime;
                waitTime = (int)WaitTimeNumberBox.Value;
                WaitTimeNumberBox.Value = waitTime;

                Log.Debug("等待时间从 {OldWaitTime} 秒修改为 {NewWaitTime} 秒",
                    oldWaitTime, waitTime);
            }
        }

        private void WaitTimeNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Log.Debug("等待时间输入框失去焦点");
            int get = WaitTimeNumberBox.Text.ToInt32(-1);
            if (get != -1)
            {
                waitTime = get;
                Log.Debug("等待时间设置为: {WaitTime} 秒", waitTime);
            }
            else
            {
                waitTime = 0;
                Log.Warning("等待时间输入无效，重置为 0 秒");
            }
            WaitTimeNumberBox.Value = waitTime;
        }

        private void AudioComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canChange && AudioComboBox.SelectedIndex >= 0)
            {
                string oldUrl = url;
                url = AudioFamily[AudioComboBox.SelectedIndex];

                Log.Information("音频选择从 {OldUrl} 更改为 {NewUrl}，索引: {Index}",
                    oldUrl ?? "空", url, AudioComboBox.SelectedIndex);
            }
        }
    }
}