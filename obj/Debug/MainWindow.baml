   M S B A M L   `   `   `  ����G  C学号魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:NameCubeNameCube  5   	   -clr-namespace:NameCube.ModeNameCube.Mode  5   	       NameCube.MainWindow   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:NameCube     5   	   ,modeclr-namespace:NameCube.Mode     5   	   M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6/ui&http://schemas.lepo.co/wpfui/2022/xaml  5   	   -   5          � Title$  学号魔方��5
   	   $	��600��6   $
��1400��6#   .��5      �  � RowDefinitions 5   
   �� 5      $��50 �6   �� 5      $��* �6   .+�5   
     Wpf.Ui.Controls.TitleBar  $��0��6      Title$ 学号魔方��6#      Icon 60   &   Wpf.Ui.Markup.ImageIconExtension  *
icon.png+$  Wpf.Ui.Controls.NavigationView  5   
   -   5          Name$ NavigationMenu��5      $	��300��5      $��0q�5         IsBackButtonVisible$
 Auto=�5         IsPaneToggleVisible .    PaneDisplayMode$
 Left=�5      $��1��5         	MenuItems 5      (  "Wpf.Ui.Controls.NavigationViewItem  5      $��单人模式��5      	   TargetPageType!   NameCube.Mode.OnePeopleMode"	  5      
   Icon
 5      '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ Accessibility16=�5        5      $��概率因子模式��5      $   NameCube.Mode.MemoryFactorMode"	  5!      
 5         $ PeopleSettings24=�5!        5"      $��批量模式��5#         NameCube.Mode.BatchMode"	  5%      
 5$        $ PeopleCommunity24=�5%      5(      5*      5+      