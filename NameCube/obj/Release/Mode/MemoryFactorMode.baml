   M S B A M L   `   `   `  ���� G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:NameCubeNameCube  R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R8clr-namespace:System.ComponentModel;assembly=WindowsBaseSystem.ComponentModel O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem $    NameCube.Mode.MemoryFactorMode   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     (localclr-namespace:NameCube     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     Hscm8clr-namespace:System.ComponentModel;assembly=WindowsBase     6sys&clr-namespace:System;assembly=mscorlib     M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   8vio.http://schemas.lepo.co/wpfui/2022/xaml/violeta    N� Title$  MemoryFactorMode�� � 	Resources    DescendingView �� Source �� SortDescriptions+  %System.ComponentModel.SortDescription   	Direction   PropertyName �� LiveSortingProperties �� IsLiveSortingRequested��%k   &	        ��  �� *thisModeJson+   $ 
Descending=�$ Factor�� ��
Factor�� . .���  � ColumnDefinitions �� $��3* ��� $��1* ��� $	��200 �	 � RowDefinitions	 �� $	��300 ��� $	��100 ��� $��1* �.+��� -   
 ��Name$
 NowNumberText��$��Center=�$��Center=�$	��150(����#30d7d7$	��XXX���� -   $
 FinishNumberText��$��Center=�$��Center=�$	��150(����#30d7d7$	��XXX��$y�Hidden=��� -   $
 LastFactorText��$��1��$��Center=�$��30(�$��......��  Wpf.Ui.Controls.Button  -     Name$ StartButton��$��1��$��2��$	��200��$	��100��$��Center=�$��开始��$��Segoe UI)�$��72(�$��Black"�  Wpf.Ui.Controls.DataGrid  $��2��$��1��&   System.Windows.Controls.DataGrid   AutoGenerateColumns .     
IsReadOnly . $ �� IsSynchronizedWithCurrentItem$ FalseY�����  �� Source# [     Columns 0  *System.Windows.Controls.DataGridTextColumn     FontSize$ 25Q�,  &System.Windows.Controls.DataGridColumn   Header$ 姓名��1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Name+  $ 25Q�$ 因子�� �� *Factor+�� $��2��$��1��$��2��.+�  -   $ ResetButton��$��1��$��1��$��0,0,0,0q�$	��Top=�$��重置概率因子��$��36(�
���  ��#  Wpf.Ui.Controls.FlyoutService   Flyout 	  Wpf.Ui.Controls.Flyout	   	  	Placement$ Bottom=�.���� .+��� $
��Left=�$����确定重置概率因子吗？（重置概率因子后配置文件不会删除，而是会创建一个备份,但保底数据会被清空）���� -   $��确定���� -    ��Name$ SpeechButton��$��启用朗读���� $��3��$��2��.+��� $��Center=�$��30(�$��当前概率最高���� -   $
 MaxIndexName��$��Center=�$��30(�$	��XXX���� -	   $
 MaxIndexText��$��Center=�$��30(�$	��XXX���� $��Center=�$��20(�$��真实概率���� -
   $
 MaxIndexRealProbability��$��Center=�$��20(�$	��XXX���� -   $
 Mininum��$��Center=�$��20(�$��距离进入保底阶段��$y�Visible=��� -   $
 MinimumText��$��Center=�$��20(�$	��XXX��$y�Visible=��� -   $
 MininumState��$��5,0,5,0q�$��Center=�$��20(�$��Italic$�$<��6当前进入保底阶段，每抽一次概率增加25%��$
��Wrap=�$y�	Collapsed=��� $��3��
���   ��� $��0,10,0,0q�$��Center=�$��30(�$��概率UP���� -   $
 UpName��$��Center=�$��30(�$	��XXX���� -   $
 UpFactor��$��Center=�$��30(�$	��XXX���� $��Center=�$��20(�$��真实概率���� -   $
 UpRealProbability��$��Center=�$��20(�$	��XXX���� $��3��
���   ��� $��0,10,0,0q�$��Center=�$��25(�$��总概率因子数量���� -   $
 Count��$��Center=�$��25(�$�� ���� $��3��
���   ��� $��5,10,5,0q�$��Center=�$��20(�$��Italic$�$<��6真实概率=概率因子数量÷总概率因子数量��$
��Wrap=�