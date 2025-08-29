using NameCube.ToolBox.AutomaticProcessPages.ProcessPages;
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
using System.Windows.Shapes;

namespace NameCube.ToolBox.AutomaticProcessPages
{
    /// <summary>
    /// DebugWindows.xaml 的交互逻辑
    /// </summary>
    public partial class DebugWindows : Window
    {
        public DebugWindows(Page page)
        {
            InitializeComponent();
            MainFrame.Navigate(page);
            if (page is ReadyPage readyPage)
            {
                readyPage.RequestParentAction += HandleChildRequest;
            }
            else if(page is StartFiles startFiles)
            {
                startFiles.RequestParentAction += HandleChildRequest;
            }
            else if (page is AudioPage audioPage)
            {
                audioPage.RequestParentAction += HandleChildRequest;
            }
            else if(page is ReadPage readPage)
            {
                readPage.RequestParentAction += HandleChildRequest;
            }
            else if(page is CmdPage cmdPage)
            {
                cmdPage.RequestParentAction += HandleChildRequest;
            }
            else if(page is WaitPage waitPage)
            {
                waitPage.RequestParentAction += HandleChildRequest;
            }
            else if(page is ClearPage clearPage)
            {
                clearPage.RequestParentAction += HandleChildRequest;
            }
            else if(page is PowerOffPage powerOffPage)
            {
                powerOffPage.RequestParentAction += HandleChildRequest;
            }
        }
        public void Exit(string data = null)
        {
            if (data != null)
            {
                MessageBoxFunction.ShowMessageBoxInfo(data);
            }
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => this.Close());
            }
            else
            {
                this.Close();
            }
        }
        private void HandleChildRequest(string data)
        {
            Exit(data);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (MainFrame.Content is ReadyPage readyPage)
            {
                readyPage.RequestParentAction -= HandleChildRequest;
            }
            base.OnClosed(e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //关闭界面
            MainFrame.Content = null;
        }
    }
}
