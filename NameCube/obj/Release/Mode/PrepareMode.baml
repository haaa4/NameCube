   M S B A M L   `   `   `  ���� G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:NameCube.ModeNameCube.Mode      NameCube.Mode.PrepareMode   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     -localclr-namespace:NameCube.Mode     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml     N� Title$  PrepareMode��.���  � RowDefinitions �� $��9* ��� $��1* �.+��  � ColumnDefinitions �� $��1* ��� $��1* �.+��� -    ��Name$ NowNumberText��$��2��$��Center=�$	��Top=�$	��120(����#30d7d7$	��XXX��$��0 30 0 0q��� -   $ 
FinishText��$��2��$��Center=�$	��Top=�$	��120(����#30d7d7$��XX��$y�Hidden=�$��0 30 0 0q�  Wpf.Ui.Controls.Button  -     Name$ StartButton��$��1��$��2��$	��200��$	��100��$
��0,40q�$��Center=�$��Bottom=�$��开始��$��Segoe UI)�$��72(�$��Black"��� $��30(�$"��请以下同学做好准备:���� $��0,40,0,0q�.+��� -   $ Ready1��$
��Left=�$��55(����#30d7d7$	��XXX���� -   $ Ready2��$
��Left=�$��55(����#30d7d7$	��XXX���� -   $ Ready3��$
��Left=�$��55(����#30d7d7$	��XXX���� -   $ Ready4��$
��Left=�$��55(����#30d7d7$	��XXX���� -   $ Ready5��$
��Left=�$��55(����#30d7d7$	��XXX���� $��0,40,0,0q�.+��� -	   $ FinishReady1��$
��Left=�$��55(����#30d7d7$	��XXX��$y�Hidden=��� -
   $ FinishReady2��$
��Left=�$��55(����#30d7d7$	��XXX��$y�Hidden=��� -   $ FinishReady3��$
��Left=�$��55(����#30d7d7$	��XXX��$y�Hidden=��� -   $ FinishReady4��$
��Left=�$��55(����#30d7d7$	��XXX��$y�Hidden=��� -   $ FinishReady5��$
��Left=�$��55(����#30d7d7$	��XXX��$y�Hidden=��� $��0,40,0,0q�.+��� -   $ BeforeReady1��$��Right=�$��55(�
���   �$	��XXX��$y�Hidden=��� -   $ BeforeReady2��$��Right=�$��55(�
���   �$	��XXX��$y�Hidden=��� -   $ BeforeReady3��$��Right=�$��55(�
���   �$	��XXX��$y�Hidden=��� -   $ BeforeReady4��$��Right=�$��55(�
���   �$	��XXX��$y�Hidden=��� -   $ BeforeReady5��$��Right=�$��55(�
���   �$	��XXX��$y�Hidden=��� $��2�� �� Orientation$ 
Horizontal=�.+��� -    ��Name$ 
SpeakCheck��$��启用语音��$��Black"�