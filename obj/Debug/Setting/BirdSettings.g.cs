﻿#pragma checksum "..\..\..\Setting\BirdSettings.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "5B7B67291A2A679FE8ECD3B0D725A1005F8EBC8E7DF00811DA442D044BEF2A1E"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

using NameCube.Setting;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Converters;
using Wpf.Ui.Markup;


namespace NameCube.Setting {
    
    
    /// <summary>
    /// BirdSettings
    /// </summary>
    public partial class BirdSettings : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 36 "..\..\..\Setting\BirdSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.ToggleSwitch BallCheck;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\..\Setting\BirdSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.DropDownButton DropWay;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\..\Setting\BirdSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.ProgressRing Ring;
        
        #line default
        #line hidden
        
        
        #line 88 "..\..\..\Setting\BirdSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.Image ImageIcon;
        
        #line default
        #line hidden
        
        
        #line 110 "..\..\..\Setting\BirdSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider ABSlider;
        
        #line default
        #line hidden
        
        
        #line 135 "..\..\..\Setting\BirdSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Wpf.Ui.Controls.ToggleSwitch AutoAdsorb;
        
        #line default
        #line hidden
        
        
        #line 157 "..\..\..\Setting\BirdSettings.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider Diaphaneity;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/学号魔方;component/setting/birdsettings.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Setting\BirdSettings.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.BallCheck = ((Wpf.Ui.Controls.ToggleSwitch)(target));
            
            #line 37 "..\..\..\Setting\BirdSettings.xaml"
            this.BallCheck.Click += new System.Windows.RoutedEventHandler(this.BallCheck_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.DropWay = ((Wpf.Ui.Controls.DropDownButton)(target));
            return;
            case 3:
            
            #line 59 "..\..\..\Setting\BirdSettings.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItem_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 60 "..\..\..\Setting\BirdSettings.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItem_Click_1);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 61 "..\..\..\Setting\BirdSettings.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItem_Click_2);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 62 "..\..\..\Setting\BirdSettings.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItem_Click_3);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 63 "..\..\..\Setting\BirdSettings.xaml"
            ((System.Windows.Controls.MenuItem)(target)).Click += new System.Windows.RoutedEventHandler(this.MenuItem_Click_4);
            
            #line default
            #line hidden
            return;
            case 8:
            this.Ring = ((Wpf.Ui.Controls.ProgressRing)(target));
            return;
            case 9:
            this.ImageIcon = ((Wpf.Ui.Controls.Image)(target));
            
            #line 88 "..\..\..\Setting\BirdSettings.xaml"
            this.ImageIcon.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.ImageIcon_MouseDown);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 89 "..\..\..\Setting\BirdSettings.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click_1);
            
            #line default
            #line hidden
            return;
            case 11:
            this.ABSlider = ((System.Windows.Controls.Slider)(target));
            
            #line 110 "..\..\..\Setting\BirdSettings.xaml"
            this.ABSlider.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.ABSlider_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 12:
            this.AutoAdsorb = ((Wpf.Ui.Controls.ToggleSwitch)(target));
            
            #line 136 "..\..\..\Setting\BirdSettings.xaml"
            this.AutoAdsorb.Click += new System.Windows.RoutedEventHandler(this.AutoAdsorb_Click);
            
            #line default
            #line hidden
            return;
            case 13:
            this.Diaphaneity = ((System.Windows.Controls.Slider)(target));
            
            #line 157 "..\..\..\Setting\BirdSettings.xaml"
            this.Diaphaneity.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.Diaphaneity_ValueChanged);
            
            #line default
            #line hidden
            return;
            case 14:
            
            #line 162 "..\..\..\Setting\BirdSettings.xaml"
            ((Wpf.Ui.Controls.CardAction)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

