   M S B A M L   `   `   `  ����  点鸣魔方$    NameCube.FirstUse.NameCubeMode   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5        N� Title$  NameCubeMode��5      .��5      �� .+�5   
   	� -   5         	�Name$ RadioButton1��5      $��
20,20,20,0q�5      $��长期运行模式��5       d� 	IsChecked$
 TrueY�5      �� 5   
   $��20,0,0,0q�5      $��20(�5      $u��o此模式下，应用将在后台长期运行，可以更快地通过各种方式（如悬浮球）启动软件��5      $
��Wrap=�5      r� 5   
   -   5         r�Name$ Media1��5      $	��300��5      $��20,0,0,0q�5      $
��Left=�5       r� LoadedBehavior$ Manual=�5       r� Source$( "pack://application:,,,/Video/1.mp4?�5       	� 5!   
   -   5        $��
20,20,20,0q�5"      $��单次运行模式��5$      �� 5%   
   $��20,0,0,0q�5&      $��20(�5'      $N��H此模式下，应用只会显示在前台，关闭窗口即关闭应用��5(      $
��Wrap=�5)      r� 5+   
   -   5        $ Media2��5,      $	��300��5-      $��20,0,0,0q�5.      $
��Left=�5/      $ Manual=�50      $( "pack://application:,,,/Video/2.mp4?�53      54      55      