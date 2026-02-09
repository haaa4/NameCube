using System;
using System.Collections.Generic;
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
using Serilog;
using NameCube.GlobalVariables.DataClass;

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessSettingPages
{
    /// <summary>
    /// ShutDownSettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class ShutDownSettingPage : Page
    {
        bool canChange = false;
        public int shutDownWay;

        public ShutDownSettingPage(ProcessData processData)
        {
            Log.Information("初始化关机设置页面");
            try
            {
                InitializeComponent();

                // 验证并设置关机方式
                if (processData.doubleData != double.NaN && processData.doubleData >= 0 && processData.doubleData <= 3)
                {
                    shutDownWay = (int)processData.doubleData;
                    Log.Debug("从流程数据获取关机方式: {ShutDownWay}", shutDownWay);
                }
                else
                {
                    shutDownWay = 0;
                    Log.Warning("流程数据中的关机方式无效，使用默认值 0");
                }

                ShutDownWayComboBox.SelectedIndex = shutDownWay;
                canChange = true;

                Log.Information("关机设置页面初始化完成，关机方式: {ShutDownWay}", shutDownWay);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "初始化关机设置页面时发生错误");
                throw;
            }
        }

        private void ShutDownWayComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (canChange && ShutDownWayComboBox.SelectedIndex >= 0)
            {
                int oldShutDownWay = shutDownWay;
                shutDownWay = ShutDownWayComboBox.SelectedIndex;

                Log.Information("关机方式从 {OldWay} 修改为 {NewWay}",
                    GetShutDownWayName(oldShutDownWay), GetShutDownWayName(shutDownWay));
            }
        }

        private string GetShutDownWayName(int way)
        {
            switch (way)
            {
                case 0: return "立即关机";
                case 1: return "一般关机";
                case 2: return "强制关机";
                case 3: return "重启";
                default: return $"未知({way})";
            }
        }
    }
}