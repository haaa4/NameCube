   M S B A M L   `   `   `  ���� G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:NameCubeNameCube  -clr-namespace:NameCube.ModeNameCube.Mode  [ WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms     NameCube.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     (localclr-namespace:NameCube     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     ,modeclr-namespace:NameCube.Mode     Qtray@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms     M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   -     � Title$  点鸣魔方��$
��1400��$	��650��$
��1400��$	��650��.���  � RowDefinitions �� $��50 ��� $��* �.+�  Wpf.Ui.Controls.TitleBar     Title$ 点鸣魔方��$��0��   Icon &   Wpf.Ui.Markup.ImageIconExtension  *
icon.png+$  Wpf.Ui.Controls.NavigationView  -     Name$ NavigationMenu��$��1��$	��300��$��0q�   IsBackButtonVisible$
 Auto=�   IsPaneToggleVisible .    PaneDisplayMode$
 Left=�   	MenuItems (  "Wpf.Ui.Controls.NavigationViewItem  $��主页��	   TargetPageType   NameCube.Mode.Home"	  
   Icon
 '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ Home24=�  $��单人模式��!   NameCube.Mode.OnePeopleMode"	  
   $ Accessibility16=�  $��因子模式��$   NameCube.Mode.MemoryFactorMode"	  
   $ PeopleSettings24=�  $��批量模式��	   NameCube.Mode.BatchMode"	 	 
   $ PeopleCommunity24=�  $��数字模式��
   NameCube.Mode.NumberMode"	 
 
   $ NumberSymbol24=�  $��预备模式��   NameCube.Mode.PrepareMode"	  
   $ Run24=�  $��记忆模式��   NameCube.Mode.MemoryMode"	  
   $ BrainCircuit24=�   
PaneFooter �� .+�   Wpf.Ui.Controls.CardAction  -   $��55��$	��5,0q�$��应用设置��   Icon   $ 
Settings24=�  -     Name$ ToolBoxCardAction��$��55��$	��5,5q�$��	小工具��   $ 	Toolbox24=�