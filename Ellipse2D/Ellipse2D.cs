using Contract;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ellipse2D
{
    public class Ellipse2D : IShape
    {
        private Point2D _start = new Point2D();
        private Point2D _finish = new Point2D();
        private SolidColorBrush _colorBrush = Brushes.Black;
        private double _strokeThickness = 1;
        private PenLineCap _strokeDashCap = PenLineCap.Flat;
        private int _gapSize = 0;
        private int _dashSize = 0;

        public string Name => "Ellipse";
        public string Icon => "Images/ellipse.png";
        
        public UIElement Draw(
            SolidColorBrush colorBrush,
            double strokeThickness,
            PenLineCap strokeDashCap,
            int gapSize,
            int dashSize
        )
        {
            _colorBrush = colorBrush;
            _strokeThickness = strokeThickness;
            _strokeDashCap = strokeDashCap;
            _gapSize = gapSize;
            _dashSize = dashSize;

            return ReDraw();
        }

        public UIElement ReDraw()
        {
            var ellipse = new Ellipse()
            {
                Width = Math.Abs(_start.X - _finish.X),
                Height = Math.Abs(_start.Y - _finish.Y),
                Stroke = _colorBrush,
                StrokeThickness = _strokeThickness,
                StrokeDashCap = _strokeDashCap,
                StrokeDashArray = new DoubleCollection() { _dashSize, _gapSize }
            };

            Canvas.SetLeft(ellipse, Math.Min(_start.X, _finish.X));
            Canvas.SetTop(ellipse, Math.Min(_start.Y, _finish.Y));

            return ellipse;
        }

        public void HandleStart(double x, double y)
        {
            _start.X = x;
            _start.Y = y;
        }

        public void HandleFinish(double x, double y)
        {
            _finish.X = x;
            _finish.Y = y;
        }

        public IShape Clone()
        {
            return new Ellipse2D();
        }
    }
}
