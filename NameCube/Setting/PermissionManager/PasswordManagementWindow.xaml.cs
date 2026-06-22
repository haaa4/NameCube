using NameCube.Function;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NameCube.Setting.PermissionManager
{
    /// <summary>
    /// PasswordManagementWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PasswordManagementWindow 
    {
        bool canClose = false;
        public PasswordManagementWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var story=FindResource("Start") as Storyboard;
            story?.Begin();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(PasswordBox1.Password.Length < 6)
            {
                OkButton.Content="密码长度不能小于6位!";
                return;
            }  
            if(PasswordBox1.Password != PasswordBox2.Password)
            {
                OkButton.Content = "两次输入的密码不一致!";
                return;
            }
            GlobalVariablesData.creds = new(PasswordBox1.Password);
            CredentialHelper.SaveCredential(Environment.UserName, PasswordBox1.Password);
            MessageBoxFunction.ShowMessageBoxInfo("密码设置成功!可设置U盘凭证（修改密码后需要重新设置）");
            CenterScreen centerScreen = new CenterScreen();
            centerScreen.ShowDialog();
            canClose = true;
            this.Close();
        }

        private void FluentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!canClose)
            {
                //阻止关闭窗口
                e.Cancel = true;
            }

        }
    }
}
