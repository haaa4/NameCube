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
using NameCube.Setting.PermissionManager;
using Serilog;
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

namespace NameCube.Setting
{
    /// <summary>
    /// PermissionManagerSettings.xaml 的交互逻辑
    /// </summary>
    public partial class PermissionManagerSettings : Page
    {
        bool canChange = false;

        public PermissionManagerSettings()
        {
            InitializeComponent();
            if(GlobalVariablesData.creds != null)
            {
                StartCheck.IsChecked = true;
            }
            CheckBox1.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["Restart"];
            CheckBox2.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["MainWindow"];
            CheckBox3.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["Archives"];
            CheckBox4.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["ModeSetting"];
            CheckBox5.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["Appearance"];
            CheckBox6.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["Speech"];
            CheckBox7.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["BirdSetting"];
            CheckBox8.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["ShortCutKeySetting"];
            CheckBox9.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["LogSetting"];
            CheckBox10.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["UpData"];
            CheckBox16.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["WindowsAndRendering"];
            CheckBox11.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["Other"];
            CheckBox12.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["About"];
            CheckBox14.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["MemoryToolbox"];
            CheckBox15.IsChecked = GlobalVariablesData.config.PermissionManager.needAdminUser["AutomaticProcess"]; 
            canChange = true;
        }

        private void StartCheck_Click(object sender, RoutedEventArgs e)
        {
           if(StartCheck.IsChecked == true)
            {
                PasswordManagementWindow passwordManagementWindow = new();
                passwordManagementWindow.ShowDialog();
                if(GlobalVariablesData.creds==null)
                {
                    StartCheck.IsChecked = false;
                }
            }
            else
            {
                CredentialHelper.DeleteCredential();
                GlobalVariablesData.creds = null;
                GlobalVariablesData.SaveConfig();
            }
            
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
                PasswordManagementWindow passwordManagementWindow = new();
                passwordManagementWindow.ShowDialog();
        }

        private void ManageUSB_Click(object sender, RoutedEventArgs e)
        {
            CenterScreen centerScreen = new();
            centerScreen.ShowDialog();
        }


        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(canChange)
            {
                GlobalVariablesData.config.PermissionManager.needAdminUser["Restart"] = CheckBox1.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["MainWindow"] = CheckBox2.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["Archives"] = CheckBox3.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["ModeSetting"] = CheckBox4.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["Appearance"] = CheckBox5.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["Speech"] = CheckBox6.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["BirdSetting"] = CheckBox7.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["ShortCutKeySetting"] = CheckBox8.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["LogSetting"] = CheckBox9.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["UpData"] = CheckBox10.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["WindowsAndRendering"] = CheckBox16.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["Other"] = CheckBox11.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["About"] = CheckBox12.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["MemoryToolbox"] = CheckBox14.IsChecked == true;
                GlobalVariablesData.config.PermissionManager.needAdminUser["AutomaticProcess"] = CheckBox15.IsChecked == true;
                GlobalVariablesData.SaveConfig();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string jsonString = LastConfig.Text;
                jsonString = jsonString.Substring(1);
                jsonString = AesHelper.AesBestPracticeHelper.Decrypt(jsonString, LastPassword.Password);
                if (jsonString.StartsWith("解"))
                {
                    jsonString = jsonString.Substring(1);
                    Log.Debug("配置文件解密成功");
                    Clipboard.SetText(jsonString);
                    MessageBoxFunction.ShowMessageBoxInfo("恭喜！配置文件解密成功，已复制到剪贴板了。覆盖现在的config.json就可以了（记得关软件哦）");
                }
                else
                {
                    var errorMsg = "配置文件解密失败，可能是由于密码错误或文件损坏导致的。实在没办法也只能放弃了......";
                    Log.Error(errorMsg);
                    MessageBoxFunction.ShowMessageBoxError(errorMsg);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = "配置文件解密失败，可能是由于密码错误或文件损坏导致的。实在没办法也只能放弃了......";
                Log.Error(ex, errorMsg);
                MessageBoxFunction.ShowMessageBoxError(errorMsg, true, ex);
            }
        }
    }
}
