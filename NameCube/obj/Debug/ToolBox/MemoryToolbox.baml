   M S B A M L   `   `   `  ����G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:NameCube.ToolBoxNameCube.ToolBox  5      $    NameCube.ToolBox.MemoryToolbox   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      0localclr-namespace:NameCube.ToolBox     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   5        N� Title$  MemoryToolbox��5	      .��5      �� .+�5   
   !  Wpf.Ui.Controls.CardControl  $��4q�5      $��20,10,20,10q�5         Icon 5      '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ Broom24=�   Header 5      �� 5      .+�5        Wpf.Ui.Controls.TextBlock  $��0q�5         FontTypography$ 
BodyStrong=�5      $��自动内存清理��5      $��WrapWithOverflow=�5        5         
Appearance$ 	Secondary=�5      $L��F开启后，将在后台每隔十分钟清理内存(重启后生效）��5      $��WrapWithOverflow=�5      5      .��5       "  Wpf.Ui.Controls.ToggleSwitch  -   5          Name$ MemoryCheck��5!      $
��Left=�5"      {�.    
OffContent$	 关��5%         	OnContent$	 开��5&      5'        Wpf.Ui.Controls.Button  5(   
   -   5        $��5,0,0,0q�5)      $��现在进行内存清理��5+      	   Icon*  $Wpf.Ui.Controls.IconElementConverter$"	 ui:SymbolIcon Symbol=Broom24 5,      '  !Wpf.Ui.Controls.SnackbarPresenter  5-   
   -   5        
  Name$
 SnackbarPresenterHost��5-        5.   
   -   5        $
 SnackbarPresenterHost1��5.        5/   
   -   5        $
 SnackbarPresenterHost2��5/      50      51      