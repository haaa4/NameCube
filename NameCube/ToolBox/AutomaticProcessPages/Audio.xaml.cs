using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace NameCube.ToolBox.AutomaticProcessPages
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Windows.Interop;
    using Wpf.Ui.Input;

    public class IconToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Icon icon)
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    icon.Handle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
            }
            return null;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture
        )
        {
            throw new NotImplementedException();
        }
    }

    public class FileIconHelper
    {
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbSizeFileInfo,
            uint uFlags
        );

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        /// <summary>
        /// 获取文件类型的图标
        /// </summary>
        /// <param name="fileExtension">文件扩展名（如 ".txt"）</param>
        /// <param name="isLargeIcon">是否获取大图标</param>
        /// <returns>文件类型对应的图标</returns>
        public static Icon GetFileIcon(string fileExtension, bool isLargeIcon = true)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;

            if (isLargeIcon)
                flags |= SHGFI_LARGEICON;
            else
                flags |= SHGFI_SMALLICON;

            SHGetFileInfo(fileExtension, 0x80, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);

            if (shinfo.hIcon == IntPtr.Zero)
                return null;

            Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
            DestroyIcon(shinfo.hIcon);
            return icon;
        }
    }

    /// <summary>
    /// Audio.xaml 的交互逻辑
    /// </summary>
    public partial class Audio : Page
    {
        public Audio()
        {
            InitializeComponent();
            Directory.CreateDirectory(musicPath);
            DataContext = this;
            GetAudio(musicPath);
        }

        private void GetAudio(string path)
        {
            AudioFamliy.Clear();
            string[] filenames = Directory.GetFiles(path);
            foreach (string filename in filenames)
            {
                FileInfo fileInfo = new FileInfo(filename);
                if (
                    fileInfo.Extension == ".mp3"
                    || fileInfo.Extension == ".wma"
                    || fileInfo.Extension == ".wav"
                )
                {
                    AudioSave audioSave = new AudioSave()
                    {
                        icon = FileIconHelper.GetFileIcon(fileInfo.Extension),
                        name = fileInfo.Name,
                    };
                    AudioFamliy.Add(audioSave);
                }
            }
        }

        string musicPath = Path.Combine(GlobalVariables.configDir, "Music");

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(musicPath);
        }

        public class AudioSave
        {
            public Icon icon { get; set; }
            public string name { get; set; }
        }
        //词写错了，请谅解
        public ObservableCollection<AudioSave> AudioFamliy { get; } =
            new ObservableCollection<AudioSave>();

        public ICommand DeleteCommand =>
            new RelayCommand<AudioSave>(audio =>
            {
                try
                {
                    string filePath = Path.Combine(musicPath, audio.name);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        AudioFamliy.Remove(audio);
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxFunction.ShowMessageBoxError($"删除失败: {ex.Message}");
                }
            });

        public ICommand GetDataCommand =>
            new RelayCommand<AudioSave>(audio =>
            {
                string filePath = Path.Combine(musicPath, audio.name);
                if (File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
            });

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "导入音频";
            openFileDialog.Filter =
                "mp3音频 (*.mp3)|*.mp3|wma音频 (*.wma)|*.wma|wav音频 (*.wav)|*.wav|所有文件 (*.*)|*.*";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    foreach (string file in openFileDialog.FileNames)
                    {
                        string sourceFile = file;
                        string fileName = Path.GetFileName(sourceFile);
                        string destPath = Path.Combine(musicPath, fileName);
                        File.Copy(sourceFile, destPath, true);
                    }
                }
                catch (Exception ex)
                {
                    MessageBoxFunction.ShowMessageBoxError(ex.Message);
                }
                GetAudio(musicPath);
            }
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke((T)parameter) ?? true;

        public void Execute(object parameter) => _execute((T)parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
