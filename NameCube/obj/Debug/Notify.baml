   M S B A M L   `   `   `  ����G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:NameCubeNameCube  5          NameCube.Notify   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      (localclr-namespace:NameCube     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   5      -   5          � Title$  Notify��5	      $	��170��5
      $	��350��5      
������� � 
ResizeMode$ NoResize=�5       � ShowInTaskbar .   � Topmost .  � WindowStyle$
 None=�5      .��5      � $��0,0,0,5q�6    � RowDefinitions 5   
   �� 5      $��2* �6   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��1* �6   .+�5   
   �  � ColumnDefinitions 5      �� 5      $��1* �6#   �� 5      $��1* �6#    5!      �� 5"      $��1* �6    �� 5#      $��1* �6    �� 5$      $��1* �6    .+�5&        Wpf.Ui.Controls.Image  $��2��5'       �� BitmapScalingMode$ HighQuality=�5(         Source$ Icon.png��5)      �� 5*      $��1��5+      $��Center=�5,      $��Center=�5-      $��18(�5.      $��点鸣魔方��5/      �� 50      $��1��51      $��1��52      $��13(�53      $��Made by haaa4��54      $
��Wrap=�55      �� 56      -   5        	 ��Name$	 
VersonText��57      $��2��58      $��2��59      $��Center=�5:      $��Center=�5;      $��20(�5<      $��Unkown��5=      5>        Wpf.Ui.Controls.Button  5?   
   -   5        $��1��5@      $	��150��5A      $��48��5B      $��Center=�5C      $��打开主界面��5E      
   Icon
 5F      '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ Open24=�  5G   
   -   5        $��2��5H      $	��150��5I      $��48��5J      $��Center=�5K      $��应用设置��5M      
 5N        $ 
Settings24=�  5O   
   -   5        $��3��5P      $	��150��5Q      $��48��5R      $��Center=�5S      $��	小工具��5U      
 5V        $ 	Toolbox24=�  5W   
   -   5        $��4��5X      $	��150��5Y      $��48��5Z      $��Center=�5[      $��重启应用��5]      
 5^        $ ArrowClockwise24=�  5_   
   -   5        $��5��5`      $	��150��5a      $��48��5b      $��Center=�5c      $��退出应用��5e      
 5f        $ ArrowExit20=�5g      5i      