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
        private double _strokeThickness = 1;
        private PenLineCap _strokeDashCap = PenLineCap.Flat;
        private int _gapSize = 0;
        private int _dashSize = 0;

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
            Line line = new Line()
            {
                X1 = _start.X,
                Y1 = _start.Y,
                X2 = _end.X,
                Y2 = _end.Y,
                Stroke = _colorBrush,
                StrokeThickness = _strokeThickness,
                StrokeDashCap = _strokeDashCap,
                StrokeDashArray = new DoubleCollection() { _dashSize, _gapSize }
            };

            return line;
        }

        public IShape Clone()
        {
            return new Line2D();
        }
    }
}
