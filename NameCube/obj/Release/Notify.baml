   M S B A M L   `   `   `  ���� G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:NameCubeNameCube      NameCube.Notify   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     (localclr-namespace:NameCube     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   -     � Title$  Notify��$	��170��$	��350��
������� � 
ResizeMode$ NoResize=� � ShowInTaskbar .   � Topmost .  � WindowStyle$
 None=�.��� $��0,0,0,5q� � RowDefinitions �� $��2* ��� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* �.+��  � ColumnDefinitions �� $��1* ��� $��1* � �� $��1* ��� $��1* ��� $��1* �.+�  Wpf.Ui.Controls.Image  $��2�� �� BitmapScalingMode$ HighQuality=�   Source$ Icon.png���� $��1��$��Center=�$��Center=�$��18(�$��点鸣魔方���� $��1��$��1��$��13(�$��Made by haaa4��$
��Wrap=��� -   	 ��Name$	 
VersonText��$��2��$��2��$��Center=�$��Center=�$��20(�$��Unkown��  Wpf.Ui.Controls.Button  -   $��1��$	��150��$��48��$��Center=�$��打开主界面��
   Icon
 '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ Open24=�  -   $��2��$	��150��$��48��$��Center=�$��应用设置��
   $ 
Settings24=�  -   $��3��$	��150��$��48��$��Center=�$��	小工具��
   $ 	Toolbox24=�  -   $��4��$	��150��$��48��$��Center=�$��重启应用��
   $ ArrowClockwise24=�  -   $��5��$	��150��$��48��$��Center=�$��退出应用��
   $ ArrowExit20=�