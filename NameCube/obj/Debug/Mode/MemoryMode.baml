   M S B A M L   `   `   `  ����G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:NameCube.ModeNameCube.Mode  5          NameCube.Mode.MemoryMode   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      -localclr-namespace:NameCube.Mode     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5	      M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   5
        N� Title$  
MemoryMode��5      .��5      �  � RowDefinitions 5   
   �� 5      $��3* �6   �� 5      $��3* �6   �� 5      $��1* �6    � ColumnDefinitions 5   
   �� 5      $��4* �6   �� 5      $��1* �6   .+�5   
   �� -   5         ��Name$ NowNumberText��5      $��Center=�5      $��Center=�5      $	��200(�5      ���#30d7d7$	��XXX��5!      �� 5"   
   -   5        $ 
FinishText��5#      $��Center=�5$      $��Center=�5%      $	��200(�5&      ���#30d7d7$	��XXX��5(      $y�Hidden=�5)        Wpf.Ui.Controls.Button  5*   
   -   5          Name$ StartButton��5+      $��1��5,      $	��200��5-      $	��100��5.      $��Center=�5/      $��开始��51      $��Segoe UI)�52      $��72(�53      $��Black"�54      �� 55   
   $��2��6    �� Orientation$ 
Horizontal=�6"   .+�56      �� -   5         ��Name$ 
SpeakCheck��57      $��启用语音��59      $��Black"�5:      �� 5;      -   5         ��Name$ ComboBox��5<       �� SelectedItem$ 1��5>      ��5=      �� *
AllFiles+5?        5@      -   5        $ ChangeButton��5A      $��12,0,0,0q�5B      $��转为常驻名单��5D        5E      -   5        $��12,0,0,0q�5F      $��添加临时名单��5H        5I      -   5        $ 	DelButton��5J      $��12,0,0,0q�5K      $��删除此名单��5M      
���  ��5N      5O        Wpf.Ui.Controls.DataGrid  5P   
   -	   5        	  Name$	 DataGrid��5Q      $��2��5R      $��1��5S      $	��400��5T      &   System.Windows.Controls.DataGrid
   AutoGenerateColumns
 .     CanUserAddRows .     CanUserDeleteRows .     CanUserResizeColumns .     CanUserResizeRows .     
IsReadOnly . ��5[      �� *
AllNames+   Columns 5\      0  *System.Windows.Controls.DataGridTextColumn  5]      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 auto 5^         Header$ 姓名��5`      1  +System.Windows.Controls.DataGridBoundColumn   Binding 5_      �� 5`      5b      #  Wpf.Ui.Controls.ContentDialog  5c   
   D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5d      :x,http://schemas.microsoft.com/winfx/2006/xaml     5e      -
   5          Name$ ContentDialog1��5f         Title$ 转为常驻名单��5g      $��3��5h      $��2��5i         CloseButtonText$ 取消��5j         PrimaryButtonText$ 确定��5l      $y�	Collapsed=�5m      .��5n      �� .+�5o      �� -   5        $ WriteNameText��5p      
���   �$��请输入名单名字:��5r      �� 5s      -   5         ��Name$ InputTextBox��5s      5t      5u      5v      5x      