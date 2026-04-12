using Masuit.Tools;
using Serilog;  // 添加Serilog命名空间
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace NameCube.FirstUse
{
    /// <summary>
    /// BirdSettings.xaml 的交互逻辑
    /// </summary>
    public partial class BirdSettings : Page
    {
        private bool CanChange;

        public BirdSettings()
        {
            InitializeComponent();
            CanChange = false;
            Log.Information("初始化BirdSettings页面");
            Initialize();
            CanChange = true;
            Log.Information("BirdSettings页面初始化完成");
        }

        private void BallCheck_Click(object sender, RoutedEventArgs e)
        {
            if (CanChange)
            {
                GlobalVariablesData.config.StartToDo.Ball = BallCheck.IsChecked.Value;
                Log.Information("切换悬浮球启动状态: {BallStatus}", BallCheck.IsChecked.Value);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                Log.Information("开始重启应用操作");
                File.WriteAllText(Path.Combine(GlobalVariablesData.configDir, "START"), "The cake is a lie");
                Log.Information("重启标记文件已写入");
                Log.Information("程序退出，准备重启");
                System.Windows.Application.Current.Shutdown();
                Process.Start(System.Windows.Application.ResourceAssembly.Location, string.Join(" ", args.Skip(1)));
                Log.Information("新进程已启动");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "重启应用时发生错误");
            }
        }

        

       

     


        private void Initialize()
        {
            try
            {
                Log.Information("开始初始化BirdSettings控件");
                BallCheck.IsChecked = GlobalVariablesData.config.StartToDo.Ball;
                StartWayComboBox.SelectedIndex = GlobalVariablesData.config.BirdSettings.StartWay;

                
                if (GlobalVariablesData.config.BirdSettings.diaphaneity == 0)
                {
                    GlobalVariablesData.config.BirdSettings.diaphaneity = 100;
                    Log.Debug("修复透明度为默认值100%");
                }


                Log.Information("BirdSettings控件初始化完成");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化BirdSettings控件时发生错误");
            }
        }

       
       

        private void StartWayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CanChange)
            {
                int selectedIndex = StartWayComboBox.SelectedIndex;
                GlobalVariablesData.config.BirdSettings.StartWay = selectedIndex;
                Log.Information("启动方式更改为索引: {StartWay}", selectedIndex);
            }
        }


    }
}