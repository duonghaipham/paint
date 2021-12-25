using Contract;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Rectangle2D
{
    public class Rectangle2D : IShape
    {
        private Point2D _start = new Point2D();
        private Point2D _finish = new Point2D();
        private SolidColorBrush _colorBrush = Brushes.Black;

        public string Name => "Rectangle";
        public string Icon => "Images/rectangle.png";

        public UIElement Draw(SolidColorBrush colorBrush)
        {
            _colorBrush = colorBrush;

            return ReDraw();
        }

        public UIElement ReDraw()
        {
            var rectangle = new Rectangle()
            {
                Width = Math.Abs(_start.X - _finish.X),
                Height = Math.Abs(_start.Y - _finish.Y),
                Stroke = _colorBrush,
                StrokeThickness = 1
            };

            Canvas.SetLeft(rectangle, Math.Min(_start.X, _finish.X));
            Canvas.SetTop(rectangle, Math.Min(_start.Y, _finish.Y));

            return rectangle;
        }

        public void HandleStart(double x, double y)
        {
            _start = new Point2D() { X = x, Y = y };
        }

        public void HandleFinish(double x, double y)
        {
            _finish = new Point2D() { X = x, Y = y };
        }

        public IShape Clone()
        {
            return new Rectangle2D();
        }
    }
}
