   M S B A M L   `   `   `  ���� G  C学号魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:NameCube.SettingNameCube.Setting  %    NameCube.Setting.SettingsWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     0localclr-namespace:NameCube.Setting     M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6/ui&http://schemas.lepo.co/wpfui/2022/xaml    � Title$  应用设置��$	��450��$	��800��.���  � RowDefinitions �� $��50 ��� $��* �.+�  Wpf.Ui.Controls.TitleBar  $��0��   Title$ 应用设置��   Icon &   Wpf.Ui.Markup.ImageIconExtension  *!pack://application:,,,/icon.ico+$  Wpf.Ui.Controls.NavigationView  -     Name$ NavigationMenu��$	��300��$��0q�   IsBackButtonVisible$
 Auto=�   IsPaneToggleVisible .    PaneDisplayMode$	 Top=�$��1��   	MenuItems (  "Wpf.Ui.Controls.NavigationViewItem  $��档案编辑��	   TargetPageType   NameCube.Setting.Archives"	  
   Icon
 '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ BookContacts24=�  $��模式设置��#   NameCube.Setting.ModeSettings"	  
   $ 
Settings24=�  $��外观��!   NameCube.Setting.Appearance"	  
   $ Collections24=�  $��	讲述人��	   NameCube.Setting.Speech"	 	 
   $ PersonVoice24=�  $��	悬浮球��#
   NameCube.Setting.BirdSettings"	 
 
   $ 	Balloon12=�  $��更新��   NameCube.Setting.Update"	  
   $ ArrowUpload24=�  $��其他��   NameCube.Setting.Other"	  
   $ MoreCircle24=�  $��关于��   NameCube.Setting.About"	  
   $ Info24=�