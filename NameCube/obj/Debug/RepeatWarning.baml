   M S B A M L   `   `   `  ����G  C点鸣魔方, Version=0.2.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:NameCubeNameCube  5          NameCube.RepeatWarning   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      (localclr-namespace:NameCube     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      M IWpf.Ui, Version=4.0.0.0, Culture=neutral, PublicKeyToken=11f9f5cc97b3ffd6I EWpf.Ui.Violeta, Version=4.0.2.3, Culture=neutral, PublicKeyToken=null1ui&http://schemas.lepo.co/wpfui/2022/xaml   5      -   5          � Title$  点名魔方��5	      $	��680��5
      $	��380��5      .��5      �� .+�5   
     Wpf.Ui.Controls.TitleBar     Title$ 点鸣魔方��5      $��0��5         Icon 5      &   Wpf.Ui.Markup.ImageIconExtension  *
icon.png+�� 5   
   -   5         ��Name$
 Text��5      $��10,0,0,0q�5      $����点鸣魔方已经启动，应用暂不支持多开，如果你是长期运行模式，请检查您的用户托盘（如下图）。如果未发现，请尝试任务管理器关闭应用��5      $
��Wrap=�5        Wpf.Ui.Controls.Image  5   
   -   5          Name$ Image��5      $	��200��5      $��20q�5         Source$ Image/RepeatWarning.png��5        Wpf.Ui.Controls.Button  5   
   -   5        $��20,0,0,0q�5      $
��Left=�5      $��Center=�5          
Appearance$ Primary=�5!      $ ��       我知道了       ��5#      $��30(�5$      5%      5&      