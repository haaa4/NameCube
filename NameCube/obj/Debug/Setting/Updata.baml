   M S B A M L   `   `   `  ����G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:NameCube.SettingNameCube.Setting  5          NameCube.Setting.Updata   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      0localclr-namespace:NameCube.Setting     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   5        N� Title$  Updata��5	      .��5      �� .+�5   
   �� $��Center=�5      $��30(�5      $��当前版本��5      �� 5   
   -   5         ��Name$ VersionText��5      $��Center=�5      $��50(�5      $��Unknow��5      � 5   
   -   5         �Name$ NowProgressBar��5      $��10��5      $
��10,0q�5       � Maximum$	 100Q�5       � Minimum$ 0Q�5       � Value$	 100Q�5      �� 5   
   -   5        $ CaseText��5       $
��10,0q�5!      $��20(�5"      $��当前已是最新版本��5#      �� 5$   
   -   5        $ 	CheckText��5%      $
��10,0q�5&      $��20(�5'      $!��上次检查时间：Unknow��5(        Wpf.Ui.Controls.Button  5)   
   -   5          Name$ CheckButton��5*      $��10q�5+         
Appearance$ Primary=�5,      $��检查更新��5.      $��Black"�5/        50   
   $��10,0,0,0q�6   $��从本地安装��6&   #  Wpf.Ui.Controls.FlyoutService   Flyout 51        Wpf.Ui.Controls.Flyout  52      	   	Placement$	 Bottom=�6   .��53      �� .+�54      �� $
��Left=�6$   $K��E请确保更新文件是正确的，否则可能导致软件丢失！��6?   �� 55      -   5        
 ��Name$
 UpdataFromComputerButton��56      $��继续��58      59      5:      5<      !  Wpf.Ui.Controls.CardControl  5=   
   $��4q�5>      $��20,10,20,10q�5?         Icon 5@      '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ DualScreenUpdate24=�   Header 5A      �� 5B      .+�5C        Wpf.Ui.Controls.TextBlock  $��0q�5D         FontTypography$ 
BodyStrong=�5E      $��更新获取��5F      $��WrapWithOverflow=�5G        5H         
Appearance$ 	Secondary=�5I      $<��6测试版本包括未完成的功能，请谨慎选择��5J      $��WrapWithOverflow=�5K      5L      .��5N      �� -   5         ��Name$ UpdataWayComboBox��5N      .@�5O      �� $��仅正式版本��6   �� 5P      $��所有版本��6   5Q      5S        5T   
   $��4q�5U      $��20,10,20,10q�5V       5W        $ Alert20=� 5X      �� 5Y      .+�5Z        $��0q�5[      $ 
BodyStrong=�5\      $��自动检查��5]      $��WrapWithOverflow=�5^        5_      $ 	Secondary=�5`      $6��0应用启动时，将会自动检测最新版本��5a      $��WrapWithOverflow=�5b      5c      .��5e      "  Wpf.Ui.Controls.ToggleSwitch  -   5          Name$ 	AutoStart��5f      $
��Left=�5g      {�.    
OffContent$	 关��5j         	OnContent$	 开��5k      5m      %  Wpf.Ui.Controls.HyperlinkButton  5n   
   $��无法连接？��5o         NavigateUri$/ )https://docs.qq.com/doc/DZmhXS3BvYnZHWnRy��5q         Icon 5p        *Link24+5q      5r      5s      