// CircularPanel.cs
using System;
using System.Windows;
using System.Windows.Controls;

namespace NameCube.Setting.Debug
{
    public class CircularPanel : Panel
    {
        /// <summary>
        /// 起始角度（度），默认 -90 表示第一个元素在正上方
        /// </summary>
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(CircularPanel),
                new FrameworkPropertyMetadata(-90.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        protected override Size MeasureOverride(Size availableSize)
        {
            Size constraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(constraint);
            }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int count = InternalChildren.Count;
            if (count == 0) return finalSize;

            double centerX = finalSize.Width / 2;
            double centerY = finalSize.Height / 2;

            // 计算最大子元素尺寸，用于确定布局半径（避免元素重叠到中心）
            double maxChildSize = 0;
            foreach (UIElement child in InternalChildren)
            {
                Size size = child.DesiredSize;
                double childDiagonal = Math.Sqrt(size.Width * size.Width + size.Height * size.Height);
                if (childDiagonal > maxChildSize) maxChildSize = childDiagonal;
            }

            // 半径 = 最小可用空间的一半减去半个子元素对角线
            double radius = Math.Min(finalSize.Width, finalSize.Height) / 2 - maxChildSize / 2 - 10;
            if (radius < 20) radius = 100; // 保底

            double stepAngle = 360.0 / count;
            double startRad = DegreeToRadian(StartAngle);

            for (int i = 0; i < count; i++)
            {
                UIElement child = InternalChildren[i];
                double angle = startRad + i * DegreeToRadian(stepAngle);
                double x = centerX + radius * Math.Cos(angle) - child.DesiredSize.Width / 2;
                double y = centerY + radius * Math.Sin(angle) - child.DesiredSize.Height / 2;

                child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
            }

            return finalSize;
        }

        private double DegreeToRadian(double degree) => degree * Math.PI / 180;
    }
}