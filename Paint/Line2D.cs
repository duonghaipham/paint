using Contract;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Paint
{
    class Line2D : IShape
    {
        private Point2D _start = new Point2D();
        private Point2D _end = new Point2D();
        private SolidColorBrush _colorBrush = Brushes.Black;

        public string Name => "Line";
        public string Icon => "Images/line.png";

        public void HandleStart(double x, double y)
        {
            _start = new Point2D() { X = x, Y = y };
        }

        public void HandleFinish(double x, double y)
        {
            _end = new Point2D() { X = x, Y = y };
        }

        public UIElement Draw(SolidColorBrush colorBrush)
        {
            _colorBrush = colorBrush;

            return ReDraw();
        }

        public UIElement ReDraw()
        {
            Line line = new Line()
            {
                X1 = _start.X,
                Y1 = _start.Y,
                X2 = _end.X,
                Y2 = _end.Y,
                StrokeThickness = 1,
                Stroke = _colorBrush,
            };

            return line;
        }

        public IShape Clone()
        {
            return new Line2D();
        }
    }
}
