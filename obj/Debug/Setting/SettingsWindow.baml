   M S B A M L   `   `   `  ����G  C学号魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:NameCube.SettingNameCube.Setting  5   	   %    NameCube.Setting.SettingsWindow   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:NameCube.Setting     5   	   M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6/ui&http://schemas.lepo.co/wpfui/2022/xaml  5   	     � Title$  应用设置��5	   	   $	��450��6   $	��800��6#   .��5
      �  � RowDefinitions 5   
   �� 5      $��50 �6   �� 5      $��* �6   .+�5   
     Wpf.Ui.Controls.TitleBar  $��0��6      Title$ 应用设置��6#      Icon 60   &   Wpf.Ui.Markup.ImageIconExtension  *!pack://application:,,,/icon.ico+$  Wpf.Ui.Controls.NavigationView  5   
   -   5          Name$ NavigationMenu��5   	   $	��300��5   	   $��0q�5   	      IsBackButtonVisible$
 Auto=�5   	      IsPaneToggleVisible .    PaneDisplayMode$	 Top=�5   	   $��1��5   	      	MenuItems 5      (  "Wpf.Ui.Controls.NavigationViewItem  5      $��档案编辑��5      	   TargetPageType   NameCube.Setting.Archives"	  5      
   Icon
 5      '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ BookContacts24=�5        5      $��模式设置��5      #   NameCube.Setting.ModeSettings"	  5       
 5        $ 
Settings24=�5         5!      $��外观��5"      !   NameCube.Setting.Appearance"	  5$      
 5#        $ Collections24=�5$        5%      $��	讲述人��5&      	   NameCube.Setting.Speech"	 	 5(      
 5'        $ PersonVoice24=�5(        5)      $��	悬浮球��5*      #
   NameCube.Setting.BirdSettings"	 
 5,      
 5+        $ 	Balloon12=�5,        5-      $��更新��5.         NameCube.Setting.Update"	  50      
 5/        $ ArrowUpload24=�50        51      $��其他��52         NameCube.Setting.Other"	  54      
 53        $ MoreCircle24=�54        55      $��关于��56         NameCube.Setting.About"	  58      
 57        $ Info24=�58      5:      5;      5<      