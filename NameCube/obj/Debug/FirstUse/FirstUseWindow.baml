   M S B A M L   `   `   `  ����G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:NameCube.FirstUseNameCube.FirstUse  5      -clr-namespace:NameCube.ModeNameCube.Mode  5      &     NameCube.FirstUse.FirstUseWindow   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      1localclr-namespace:NameCube.FirstUse     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      ,modeclr-namespace:NameCube.Mode     5      M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   5	      -   5          � Title$  点鸣魔方��5
      $
��1000��5      $	��450��5       � Icon$% pack://application:,,,/icon.ico��5      .��5      �  � RowDefinitions 5   
   �� 5      $��50 �6   �� 5      $��1* �6    � ColumnDefinitions 5   
   �� 5      $��1* �6   �� 5      $��1* �6   .+�5   
     Wpf.Ui.Controls.TitleBar     Title$ 点鸣魔方��5      $��0��5      $��2��5         Icon 5      &   Wpf.Ui.Markup.ImageIconExtension  *!pack://application:,,,/icon.png+$  Wpf.Ui.Controls.NavigationView  5   
   -   5          Name$ NavigationMenu��5      $��1��5       $��1��5!      $	��300��5"      $��0q�5#         IsBackButtonVisible$
 Auto=�5$         IsPaneToggleVisible . 	   PaneDisplayMode$	 Bottom=�5&      �� 5'   
   -   5        
 ��Name$	
 rec��5(      $��1��5)      $��1��5*      $��30��5+      $��Bottom=�5,      
�������5-      �� 5.   
   $��1��6   .+�5/      �� -   5         ��Name$ TitleGet��50      $��20,0,0,0q�51      $��40(�52      $��Black"�53      $��	欢迎！��54      �� 55      -   5        $
 Text��56      $��10,0,0,0q�57      $��20(�58      $T��N欢迎来到点鸣魔方，在使用软件前，我们需要进行一些设置��59      $
��Wrap=�5:        Wpf.Ui.Controls.Button  5;      -   5        $��	10,30,0,0q�5<      $
��Left=�5=      $��Bottom=�5>         
Appearance$ Primary=�5?      $��
  继续  ��5A        5B      -   5        $��10,5,0,0q�5C      $
��Left=�5D      $��Bottom=�5E      $$��从已有的配置文件开始��5G      5H      5I      5J      