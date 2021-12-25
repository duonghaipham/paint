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
        private SolidColorBrush _colorBrush;

        public string Name => "Ellipse";
        public string Icon => "Images/ellipse.png";
        

        public UIElement Draw(SolidColorBrush colorBrush)
        {
            _colorBrush = colorBrush;

            return ReDraw();
        }

        public UIElement ReDraw()
        {
            var ellipse = new Ellipse()
            {
                Width = Math.Abs(_start.X - _finish.X),
                Height = Math.Abs(_start.Y - _finish.Y),
                Stroke = _colorBrush,
                StrokeThickness = 1
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
