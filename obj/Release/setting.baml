   M S B A M L   `   `   `  ���� G  C学号魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:NameCubeNameCube      NameCube.setting   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     (localclr-namespace:NameCube     M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6/ui&http://schemas.lepo.co/wpfui/2022/xaml  -     N� Title$  setting��.���� .+�  Wpf.Ui.Controls.TextBlock  $��主题��$��20(�$��5q�!  Wpf.Ui.Controls.CardControl  $��4q�$��20,10,20,10q�   Icon '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ Lightbulb20=�   Header �� .+�  $��0q�   FontTypography$ 
BodyStrong=�$��关灯��$��WrapWithOverflow=�     
Appearance$ 	Secondary=�$ ��改变ui整体黑白主题��$��WrapWithOverflow=�.��"  Wpf.Ui.Controls.ToggleSwitch  -     Name$ 	DarkLight��$
��Left=�{�.    
OffContent$	 关��   	OnContent$	 开��  $��朗读��$��20(�$��5q�  $��4q�$��20,10,20,10q�   $ PersonVoice20=� �� .+�  $��0q�$ 
BodyStrong=�$��音量��$��WrapWithOverflow=�  $ 	Secondary=�$��改变朗读音量��$��WrapWithOverflow=�.���� -   	 ��Name$	 VolumeSlider��$��1��$��Center=�
 � Maximum$	
 100Q� � Value$	 100Q�  $��4q�$��20,10,20,10q�   $ 
TopSpeed20=� �� .+�  $��0q�$ 
BodyStrong=�$��速度��$��WrapWithOverflow=�  $ 	Secondary=�$��改变朗读速度��$��WrapWithOverflow=�.���� -   $	 SpeedSlider��$��1��$��1��$��Center=�$
 20Q�$ 10Q�  $��	悬浮球��$��20(�$��5q�  $��4q�$��20,10,20,10q�   $ SportBaseball24=� �� .+�  $��0q�$ 
BodyStrong=�$��	悬浮球��$��WrapWithOverflow=�  $ 	Secondary=�$@��:开启后，将在桌面显示悬浮球(重启后应用）��$��WrapWithOverflow=�.��  -   $ 	BallCheck��$
��Left=�{�. $	 关��$	 开��  $��其他��$��20(�$��5q�  $��4q�$��20,10,20,10q�   $ CalendarSettings20=� �� .+�  $��0q�$ 
BodyStrong=�$��开机自启��$��WrapWithOverflow=�  $ 	Secondary=�$��开机时将自动启动��$��WrapWithOverflow=�.��  -   $ 
StartCheck��$
��Left=�{�. $	 关��$	 开��  Wpf.Ui.Controls.Button  -     Name$ RestartButton��$"��重启软件(敬请期待）��$��5 0 0 0q�{�.     Icon   $ ArrowRepeat116=�