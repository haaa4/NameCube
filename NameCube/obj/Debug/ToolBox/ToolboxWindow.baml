   M S B A M L   `   `   `  ����G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:NameCube.ToolBoxNameCube.ToolBox  5      #clr-namespace:NameCubeNameCube  5      $    NameCube.ToolBox.ToolboxWindow   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      2Toolboxclr-namespace:NameCube.ToolBox     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      (localclr-namespace:NameCube     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   5	      -   5          � Title$  	小工具��5
      $
��1000��5      $	��450��5      .��5      �  � RowDefinitions 5   
   �� 5      $��50 �6   �� 5      .+�5   
     Wpf.Ui.Controls.TitleBar     Title$ 	小工具��5      $��0��5         Icon 5      &   Wpf.Ui.Markup.ImageIconExtension  *!pack://application:,,,/icon.ico+$  Wpf.Ui.Controls.NavigationView  5   
   -   5          Name$ NavigationMenu��5      $��1��5      $	��300��5      $��0q�5         IsBackButtonVisible$
 Auto=�5         IsPaneToggleVisible .    PaneDisplayMode$
 Left=�5         	MenuItems 5       (  "Wpf.Ui.Controls.NavigationViewItem  5!      $��自定义朗读��5"      	   TargetPageType$   NameCube.ToolBox.SpeechToolbox"	  5$      
   Icon
 5#      '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ PersonVoice24=�5$        5%      $��内存��5&      $   NameCube.ToolBox.MemoryToolbox"	  5(      
 5'        $ Memory16=�5(        5)      $��自动关机��5*         NameCube.ToolBox.ShutDown"	  5,      
 5+        $ Power24=�5,         
PaneHeader 5/      	  Wpf.Ui.Controls.Button	  50       	  Icon 6     $ ArrowClockwise24=�#
  Wpf.Ui.Controls.FlyoutService 
  Flyout 51        Wpf.Ui.Controls.Flyout  52         	Placement$ Bottom=�6$   .��53      �� .+�54   "   �� $
��Left=�6,   $!��你确定重启应用吗？��6G   �� 55   "   -   5        $��确定��55   >   56      57      59      5;      5<      5=      