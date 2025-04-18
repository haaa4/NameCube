﻿using Masuit.Tools.Files;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NameCube.Setting
{
    /// <summary>
    /// Document.xaml 的交互逻辑
    /// </summary>
    public partial class Document : Page
    {
        public Document()
        {
            InitializeComponent();
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(GlobalVariables.configDir);
        }

        private void CardAction_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult result = System.Windows.Forms.MessageBox.Show("确定删除日志及备份文件吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes) 
            {
                Directory.Delete(System.IO.Path.Combine(GlobalVariables.configDir, "logs"),true);
                Directory.Delete(System.IO.Path.Combine(GlobalVariables.configDir, "Mode_data", "MemoryFactoryMode", "Backups"),true);
                System.Windows.MessageBox.Show("删除成功");
            }
        }

        private void CardAction_Click_2(object sender, RoutedEventArgs e)
        {
            DialogResult result = System.Windows.Forms.MessageBox.Show("你确定继续执行操作吗？这将从你的硬盘上彻底删除配置文件", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes) { 
                Directory.Delete(GlobalVariables.configDir, true);
                System.Windows.MessageBox.Show("删除成功，请自行启动软件");
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void CardAction_Click_3(object sender, RoutedEventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = "c:\\"; 
                saveFileDialog.Filter = "压缩包(*.zip)|*.zip|所有文件 (*.*)|*.*"; 
                saveFileDialog.Title = "保存配置文件"; // 对话框标题
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        SevenZipCompressor.Zip(new List<string>() { GlobalVariables.configDir }, saveFileDialog.FileName);
                        System.Windows.MessageBox.Show("保存成功");
                    }
                    catch (Exception ex) 
                    {
                        System.Windows.MessageBox.Show("保存失败\n"+ex.Message,"错误",MessageBoxButton.OK,MessageBoxImage.Error);
                    }

                }
            }
        }

        private void CardAction_Click_4(object sender, RoutedEventArgs e)
        {
            DialogResult result = System.Windows.Forms.MessageBox.Show("你确定继续执行操作吗？这将从你的硬盘上彻底删除原有的配置文件", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "从压缩包导入";
                openFileDialog.Filter = "压缩包 (*.zip)|*.zip|所有文件 (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Directory.Delete(GlobalVariables.configDir, true);
                    Directory.CreateDirectory(GlobalVariables.configDir);
                    SevenZipCompressor.Decompress(openFileDialog.FileName, GlobalVariables.configDir);
                    System.Windows.MessageBox.Show("覆盖成功，请自行启动软件");
                    System.Windows.Application.Current.Shutdown();
                }
                
            }
        }
    }
}
