using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NameCube.Setting.Debug
{
    /// <summary>
    /// Whell.xaml 的交互逻辑
    /// </summary>
    public partial class Whell : Window
    {
        // 字符串列表 - 在此处修改抽取项
        private List<string> _items = new List<string>
        {
            "C#", "Python", "Java", "TypeScript", "Rust", "Go", "Swift", "Kotlin",
            "F#", "Ruby", "PHP", "C++", "JavaScript", "Dart", "Scala", "Lua"
        };

        private Random _random = new Random();
        private bool _isSpinning = false;

        public Whell()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 绑定数据源
            WheelItemsControl.ItemsSource = _items;
        }

        // 开始旋转 (匀速无限旋转)
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // 停止所有进行中的动画
            WheelRotate.BeginAnimation(RotateTransform.AngleProperty, null);

            double currentAngle = WheelRotate.Angle % 360;

            // 创建无限循环匀速动画 (每2秒转一圈)
            DoubleAnimation spinAnimation = new DoubleAnimation
            {
                From = currentAngle,
                To = currentAngle + 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = null // 匀速
            };

            WheelRotate.BeginAnimation(RotateTransform.AngleProperty, spinAnimation);
            _isSpinning = true;

            // 清空上次结果
            ResultTextBlock.Text = "旋转中...";
        }

        // 停止旋转 (减速停止，每局速度/停止位置不同)
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isSpinning) return;

            // 停止当前无限动画
            WheelRotate.BeginAnimation(RotateTransform.AngleProperty, null);

            double currentAngle = WheelRotate.Angle;

            // 随机生成减速参数：额外转3~7圈，并在最后一圈随机位置停下
            int extraFullRotations = _random.Next(1, 4);
            double extraRandomDegrees = _random.NextDouble() * 360;
            double totalExtraDegrees = extraFullRotations * 360 + extraRandomDegrees;

            // 计算基础时间（保持平均速度180度/秒）
            double baseDuration = totalExtraDegrees / 180.0;
            // 随机势能 0.8~1.2 让每局速度略有不同
            double randomFactor = 0.8 + _random.NextDouble() * 0.4; // 0.8 ~ 1.2
            double durationSeconds = baseDuration * randomFactor;

            // 随机选择缓动函数
            IEasingFunction easing = GetRandomEasing();

            DoubleAnimation stopAnimation = new DoubleAnimation
            {
                From = currentAngle,
                To = currentAngle + totalExtraDegrees,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };

            stopAnimation.Completed += (s, _) =>
            {
                // 动画完成后计算最终选中的项
                double finalAngle = WheelRotate.Angle % 360;
                if (finalAngle < 0) finalAngle += 360;

                string selectedItem = DetermineSelectedItem(finalAngle);
                ResultTextBlock.Text = selectedItem;
                _isSpinning = false;
            };

            WheelRotate.BeginAnimation(RotateTransform.AngleProperty, stopAnimation);
        }

        // 随机选择一种缓动函数，模拟不同的减速感觉
        private IEasingFunction GetRandomEasing()
        {
            int choice = _random.Next(4);
            switch (choice)
            {
                case 0: return new QuadraticEase() { EasingMode = EasingMode.EaseOut };
                case 1: return new CubicEase() { EasingMode = EasingMode.EaseOut };
                case 2: return new QuarticEase() { EasingMode = EasingMode.EaseOut };
                case 3: return new SineEase() { EasingMode = EasingMode.EaseOut };
                default: return new QuinticEase() { EasingMode = EasingMode.EaseOut };
            }
        }

        // 根据最终角度确定选中的字符串
        private string DetermineSelectedItem(double finalAngle)
        {
            int count = _items.Count;
            if (count == 0) return "无数据";

            // 每个项所占角度 (圆形面板起始角为 -90°)
            double step = 360.0 / count;
            double pointerAngle = 270; // 指针方向固定向上

            // 计算每个项相对于指针的偏差，取最小偏差的项
            int bestIndex = 0;
            double minDiff = double.MaxValue;

            for (int i = 0; i < count; i++)
            {
                // 项的实际显示角度 = 项固有角度 + 转盘旋转角度
                double itemBaseAngle = -90 + i * step; // 与面板 StartAngle 保持一致
                double itemActualAngle = (itemBaseAngle + finalAngle) % 360;
                if (itemActualAngle < 0) itemActualAngle += 360;

                // 计算与指针的角度差 (取锐角)
                double diff = Math.Abs(itemActualAngle - pointerAngle);
                if (diff > 180) diff = 360 - diff;

                if (diff < minDiff)
                {
                    minDiff = diff;
                    bestIndex = i;
                }
            }

            return _items[bestIndex];
        }

        // 可选：对外提供更新列表的方法（例如按钮点击动态修改）
        public void UpdateItems(List<string> newItems)
        {
            _items = newItems;
            WheelItemsControl.ItemsSource = null;
            WheelItemsControl.ItemsSource = _items;
        }
    }

    public class InverseAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double angle)
                return -angle;
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}