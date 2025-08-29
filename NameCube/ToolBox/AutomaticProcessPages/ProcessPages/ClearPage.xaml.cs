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

namespace NameCube.ToolBox.AutomaticProcessPages.ProcessPages
{
    /// <summary>
    /// ClearPage.xaml 的交互逻辑
    /// </summary>
    public partial class ClearPage : Page
    {
        private bool debug;
        public event Action<string> RequestParentAction;
        public event Action<int> EndThePageAction;
        private void CallEndThePage(int ret = 0)
        {
            EndThePageAction?.Invoke(ret);
        }
        private void CallParentMethodDebug(string data)
        {
            RequestParentAction?.Invoke(data);
        }
        public ClearPage(bool isDubug = false,bool show=true)
        {
            InitializeComponent();
            debug= isDubug;
            if(!show)
            {
                Page_Loaded(null, null);
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(1000);
            try
            {
                Masuit.Tools.Win32.Windows.ClearMemory();
                if(debug)
                {
                    CallParentMethodDebug("清理完成");
                }
                else
                {
                    CallEndThePage();
                }
            }
            catch(Exception ex)
            {
                MessageBoxFunction.ShowMessageBoxError(ex.Message, true, ex);
                if(debug)
                {
                    CallParentMethodDebug(ex.Message);
                }
                else
                {
                    CallEndThePage();
                }
            }
        }
    }
}
