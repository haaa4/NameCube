   M S B A M L   `   `   `  ����G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:NameCubeNameCube  5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R8clr-namespace:System.ComponentModel;assembly=WindowsBaseSystem.ComponentModel 5      O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5	      $    NameCube.Mode.MemoryFactorMode   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      (localclr-namespace:NameCube     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      Hscm8clr-namespace:System.ComponentModel;assembly=WindowsBase     5      6sys&clr-namespace:System;assembly=mscorlib     5	      M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   5
      8vio.http://schemas.lepo.co/wpfui/2022/xaml/violeta  5        N� Title$  MemoryFactorMode��5       � 	Resources 5         DescendingView �� Source �� SortDescriptions+  %System.ComponentModel.SortDescription   	Direction   PropertyName �� LiveSortingProperties �� IsLiveSortingRequested��%�   &	        �� 5   
    66   �� *thisModeJson+ 5        5      $ 
Descending=�6&   $ Factor��6=    5      ��5      
Factor��6%    . 5      5      .��5      �  � ColumnDefinitions 5   
   �� 5      $��3* �6   �� 5      $��1* �6   �� 5       $	��200 �6   	 � RowDefinitions	 5"   
   �� 5#      $	��300 �6   �� 5$      $	��100 �6   �� 5%      $��1* �6   .+�5'   
   �� -   5        
 ��Name$
 NowNumberText��5(      $��Center=�5)      $��Center=�5*      $	��150(�5+      ���#30d7d7$	��XXX��5-      �� 5.   
   -   5        $
 FinishNumberText��5/      $��Center=�50      $��Center=�51      $	��150(�52      ���#30d7d7$	��XXX��54      $y�Hidden=�55      �� 56   
   -   5        $
 LastFactorText��57      $��1��58      $��Center=�59      $��30(�5:      $��......��5;        Wpf.Ui.Controls.Button  5E   
   -   5          Name$ StartButton��5F      $��1��5G      $��2��5H      $	��200��5I      $	��100��5J      $��Center=�5K      $��开始��5M      $��Segoe UI)�5N      $��72(�5O      $��Black"�5P        Wpf.Ui.Controls.DataGrid  5Q   
   $��2��5R      $��1��5S      &   System.Windows.Controls.DataGrid   AutoGenerateColumns .     
IsReadOnly . $ �� IsSynchronizedWithCurrentItem$ FalseY�5V      ��5W      ��  �� Source# [     Columns 5X      0  *System.Windows.Controls.DataGridTextColumn  5Y         FontSize$ 25Q�5[      ,  &System.Windows.Controls.DataGridColumn   Header$ 姓名��5\      1  +System.Windows.Controls.DataGridBoundColumn   Binding 5Z      �� *Name+5\        5]      $ 25Q�5_      $ 因子��5`       5^      �� *Factor+5`      5b      �� 5c   
   $��2��5d      $��1��5e      $��2��5f      .+�5h        -   5        $ ResetButton��5i      $��1��5j      $��1��5k      $��0,0,0,0q�5l      $	��Top=�5m      $��重置概率因子��5n      $��36(�5o      
���  ��#  Wpf.Ui.Controls.FlyoutService   Flyout 5q      	  Wpf.Ui.Controls.Flyout	  5r       	  	Placement$ Bottom=�6    .��5s      �� .+�5t      �� $
��Left=�6(   $����确定重置概率因子吗？（重置概率因子后配置文件不会删除，而是会创建一个备份,但保底数据会被清空）��6C   �� 5u      -   5        $��确定��5u   ?   5v      5w      5y      �� 5z      -   5         ��Name$ SpeechButton��5{      $��启用朗读��5}      5~      �� 5   
   $��3��6   $��2��6&   .+�5�      �� $��Center=�5�      $��30(�5�      $��当前概率最高��5�      �� 5�      -   5        $
 MaxIndexName��5�      $��Center=�5�      $��30(�5�      $	��XXX��5�      �� 5�      -	   5        $
 MaxIndexText��5�      $��Center=�5�      $��30(�5�      $	��XXX��5�      �� 5�      $��Center=�5�      $��20(�5�      $��真实概率��5�      �� 5�      -
   5        $
 MaxIndexRealProbability��5�      $��Center=�5�      $��20(�5�      $	��XXX��5�      �� 5�      -   5        $
 Mininum��5�      $��Center=�5�      $��20(�5�      $��距离进入保底阶段��5�      $y�Visible=�5�      �� 5�      -   5        $
 MinimumText��5�      $��Center=�5�      $��20(�5�      $	��XXX��5�      $y�Visible=�5�      �� 5�      -   5        $
 MininumState��5�      $��5,0,5,0q�5�      $��Center=�5�      $��20(�5�      $��Italic$�5�      $<��6当前进入保底阶段，每抽一次概率增加25%��5�      $
��Wrap=�5�      $y�	Collapsed=�5�      �� 5�      $��3��6   
���   �6#   �� 5�      $��0,10,0,0q�5�      $��Center=�5�      $��30(�5�      $��概率UP��5�      �� 5�      -   5        $
 UpName��5�      $��Center=�5�      $��30(�5�      $	��XXX��5�      �� 5�      -   5        $
 UpFactor��5�      $��Center=�5�      $��30(�5�      $	��XXX��5�      �� 5�      $��Center=�5�      $��20(�5�      $��真实概率��5�      �� 5�      -   5        $
 UpRealProbability��5�      $��Center=�5�      $��20(�5�      $	��XXX��5�      �� 5�      $��3��6   
���   �6#   �� 5�      $��0,10,0,0q�5�      $��Center=�5�      $��25(�5�      $��总概率因子数量��5�      �� 5�      -   5        $
 Count��5�      $��Center=�5�      $��25(�5�      $�� ��5�      �� 5�      $��3��6   
���   �6#   �� 5�      $��5,10,5,0q�5�      $��Center=�5�      $��20(�5�      $��Italic$�5�      $<��6真实概率=概率因子数量÷总概率因子数量��5�      $
��Wrap=�5�      5�      5�      5�      