   M S B A M L   `   `   `  ����G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:NameCubeNameCube  5      -clr-namespace:NameCube.ModeNameCube.Mode  5      [ WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms 5	          NameCube.MainWindow   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      (localclr-namespace:NameCube     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      ,modeclr-namespace:NameCube.Mode     5      Qtray@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms     5	      M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   5
      -   5          � Title$  点鸣魔方��5      $
��1400��5      $	��650��5      $
��1400��5      $	��650��5      .��5      �  � RowDefinitions 5   
   �� 5      $��50 �6   �� 5      $��* �6   .+�5   
     Wpf.Ui.Controls.TitleBar     Title$ 点鸣魔方��5      $��0��5         Icon 5      &   Wpf.Ui.Markup.ImageIconExtension  *
icon.png+$  Wpf.Ui.Controls.NavigationView  5   
   -   5          Name$ NavigationMenu��5      $��1��5      $	��300��5      $��0q�5         IsBackButtonVisible$
 Auto=�5          IsPaneToggleVisible .    PaneDisplayMode$
 Left=�5"         	MenuItems 5#      (  "Wpf.Ui.Controls.NavigationViewItem  5$      $��主页��5%      	   TargetPageType   NameCube.Mode.Home"	  5'      
   Icon
 5&      '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ Home24=�5'        5(      $��单人模式��5)      !   NameCube.Mode.OnePeopleMode"	  5+      
 5*        $ Accessibility16=�5+        5,      $��因子模式��5-      $   NameCube.Mode.MemoryFactorMode"	  5/      
 5.        $ PeopleSettings24=�5/        50      $��批量模式��51      	   NameCube.Mode.BatchMode"	 	 53      
 52        $ PeopleCommunity24=�53        54      $��数字模式��55      
   NameCube.Mode.NumberMode"	 
 57      
 56        $ NumberSymbol24=�57        58      $��预备模式��59         NameCube.Mode.PrepareMode"	  5;      
 5:        $ Run24=�5;        5<      $��记忆模式��5=         NameCube.Mode.MemoryMode"	  5?      
 5>        $ BrainCircuit24=�5?         
PaneFooter 5B      �� 5C      .+�5D         Wpf.Ui.Controls.CardAction  -   5        $��55��5E      $	��5,0q�5F      $��应用设置��5H         Icon 5I        $ 
Settings24=�  5J      -   5          Name$ ToolBoxCardAction��5K      $��55��5L      $	��5,5q�5M      $��	小工具��5O       5P        $ 	Toolbox24=�5Q      5S      5T      5U      