   M S B A M L   `   `   `  ����G  C学号魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:NameCubeNameCube  5          NameCube.setting   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      (localclr-namespace:NameCube     5      M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6/ui&http://schemas.lepo.co/wpfui/2022/xaml  5      -   5          N� Title$  setting��5
      .��5      �� .+�5   
     Wpf.Ui.Controls.TextBlock  $��主题��6   $��20(�6!   $��5q�6/   !  Wpf.Ui.Controls.CardControl  5   
   $��4q�5      $��20,10,20,10q�5         Icon 5      '  !Wpf.Ui.Markup.SymbolIconExtension     Symbol$ Lightbulb20=�   Header 5      �� 5      .+�5        $��0q�5   !      FontTypography$ 
BodyStrong=�5   !   $��关灯��5   !   $��WrapWithOverflow=�5   !     5         
Appearance$ 	Secondary=�5   !   $ ��改变ui整体黑白主题��5   !   $��WrapWithOverflow=�5   !   5      .��5      "  Wpf.Ui.Controls.ToggleSwitch  -   5          Name$ 	DarkLight��5$      $
��Left=�5       {�.    
OffContent$	 关��5"         	OnContent$	 开��5#      5%      5&        5'   
   $��朗读��6   $��20(�6!   $��5q�6/     5(   
   $��4q�5)      $��20,10,20,10q�5*       5+        $ PersonVoice20=� 5,      �� 5-      .+�5.        $��0q�5/      $ 
BodyStrong=�50      $��音量��51      $��WrapWithOverflow=�52        53      $ 	Secondary=�54      $��改变朗读音量��55      $��WrapWithOverflow=�56      57      .��59      �� -   5        	 ��Name$	 VolumeSlider��59   [   $��1��6   $��Center=�6%   
 � Maximum$	
 100Q�6@    � Value$	 100Q�6N   6q   5:        5;   
   $��4q�5<      $��20,10,20,10q�5=       5>        $ 
TopSpeed20=� 5?      �� 5@      .+�5A        $��0q�5B      $ 
BodyStrong=�5C      $��速度��5D      $��WrapWithOverflow=�5E        5F      $ 	Secondary=�5G      $��改变朗读速度��5H      $��WrapWithOverflow=�5I      5J      .��5L      �� -   5        $	 SpeedSlider��5L   f   $��1��6   $��1��6"   $��Center=�62   $
 20Q�6N   $ 10Q�6[   6{   5M        5N   
   $��	悬浮球��6   $��20(�6"   $��5q�60     5O   
   $��4q�5P      $��20,10,20,10q�5Q       5R        $ SportBaseball24=� 5S      �� 5T      .+�5U        $��0q�5V      $ 
BodyStrong=�5W      $��	悬浮球��5X      $��WrapWithOverflow=�5Y        5Z      $ 	Secondary=�5[      $@��:开启后，将在桌面显示悬浮球(重启后应用）��5\      $��WrapWithOverflow=�5]      5^      .��5`        -   5        $ 	BallCheck��5e      $
��Left=�5a      {�. $	 关��5c      $	 开��5d      5f      5g        5h   
   $��其他��6   $��20(�6!   $��5q�6/     5i   
   $��4q�5j      $��20,10,20,10q�5k       5l        $ CalendarSettings20=� 5m      �� 5n      .+�5o        $��0q�5p      $ 
BodyStrong=�5q      $��开机自启��5r      $��WrapWithOverflow=�5s        5t      $ 	Secondary=�5u      $��开机时将自动启动��5v      $��WrapWithOverflow=�5w      5x      .��5z        -   5        $ 
StartCheck��5      $
��Left=�5{      {�. $	 关��5}      $	 开��5~      5�      5�        Wpf.Ui.Controls.Button  5�   
   -   5          Name$ RestartButton��5�   g   $"��重启软件(敬请期待）��6A   $��5 0 0 0q�6V   {�.     Icon 6     $ ArrowRepeat116=�6�   5�      5�      