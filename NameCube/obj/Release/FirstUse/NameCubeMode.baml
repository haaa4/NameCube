   M S B A M L   `   `   `  ����   点鸣魔方$    NameCube.FirstUse.NameCubeMode   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006       N� Title$  NameCubeMode��.���� .+�	� -    	�Name$ RadioButton1��$��
20,20,20,0q�$��长期运行模式�� d� 	IsChecked$
 TrueY��� $��20,0,0,0q�$��20(�$u��o此模式下，应用将在后台长期运行，可以更快地通过各种方式（如悬浮球）启动软件��$
��Wrap=�r� -    r�Name$ Media1��$	��300��$��20,0,0,0q�$
��Left=� r� LoadedBehavior$ Manual=� r� Source$( "pack://application:,,,/Video/1.mp4?�	� -   $��
20,20,20,0q�$��单次运行模式���� $��20,0,0,0q�$��20(�$N��H此模式下，应用只会显示在前台，关闭窗口即关闭应用��$
��Wrap=�r� -   $ Media2��$	��300��$��20,0,0,0q�$
��Left=�$ Manual=�$( "pack://application:,,,/Video/2.mp4?�