   M S B A M L   `   `   `  ����G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:NameCube.SettingNameCube.Setting  5      %    NameCube.Setting.SettingsWindow   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      0localclr-namespace:NameCube.Setting     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   5        � Title$  应用设置��5	      $
��1000��5
      $	��450��5      .��5      �  � RowDefinitions 5   
   �� 5      $��50 �6   �� 5      $��* �6   .+�5   
     Wpf.Ui.Controls.TitleBar     Title$ 应用设置��5      $��0��5         Icon 5      &   Wpf.Ui.Markup.ImageIconExtension  *!pack://application:,,,/icon.ico+$  Wpf.Ui.Controls.NavigationView  5   
   -   5          Name$ NavigationMenu��5      $��1��5      $	��300��5      $��0q�5         IsBackButtonVisible$
 Auto=�5         IsPaneToggleVisible .    PaneDisplayMode$ LeftMinimal=�5         	MenuItems 5      (  "Wpf.Ui.Controls.NavigationViewItem  5       $��档案编辑��5!      	   TargetPageType   NameCube.Setting.Archives"	  5#      
   Icon
 5"      '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ BookContacts24=�5#        5$      $��模式设置��5%      #   NameCube.Setting.ModeSettings"	  5'      
 5&        $ 
Settings24=�5'        5(      $��外观��5)      !   NameCube.Setting.Appearance"	  5+      
 5*        $ Collections24=�5+        5,      $��	讲述人��5-      	   NameCube.Setting.Speech"	 	 5/      
 5.        $ PersonVoice24=�5/        50      -   5          Name$ BallSetting��51      $��	悬浮球��52      #
   NameCube.Setting.BirdSettings"	 
 54      
 53        $ 	Balloon12=�54        55      $��	快捷键��56      )   #NameCube.Setting.ShortcutKeySetting"	  58      
 57        $ 
Keyboard24=�58        59      $��配置文件��5:         NameCube.Setting.Document"	  5<      
 5;        $ 
Document24=�5<        5=      $��更新��5>         NameCube.Setting.Updata"	  5@      
 5?        $ ArrowUpload24=�5@        5A      $��其他��5B         NameCube.Setting.Other"	  5D      
 5C        $ MoreCircle24=�5D        5E      $��关于��5F         NameCube.Setting.About"	  5H      
 5G        $ Info24=�5H         
PaneHeader 5J        Wpf.Ui.Controls.Button  5K         Icon 6     $ ArrowClockwise24=�#  Wpf.Ui.Controls.FlyoutService   Flyout 5L        Wpf.Ui.Controls.Flyout  5M         	Placement$ Bottom=�6$   .��5N      �� .+�5O   "   �� $
��Left=�6,   $!��你确定重启应用吗？��6G   �� 5P   "   -   5        $��确定��5P   >   5Q      5R      5T      5V      '  !Wpf.Ui.Controls.SnackbarPresenter  5W   
   -   5          Name$ SnackbarPresenterHost��5W      $��1��6>   5X      5Y      