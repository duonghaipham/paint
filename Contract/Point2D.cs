using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Contract
{
    public class Point2D : IShape
    {
        public double X { get; set; }
        public double Y { get; set; }
        private SolidColorBrush _colorBrush = Brushes.Black;
        private double _strokeThickness = 1;

        public string Name => "Point";
        public string Icon => "";

        public void HandleStart(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void HandleFinish(double x, double y)
        {
            X = x;
            Y = y;
        }

        public UIElement Draw(SolidColorBrush colorBrush, double strokeThickness)
        {
            _colorBrush = colorBrush;
            _strokeThickness = strokeThickness;

            return ReDraw();
        }

        public UIElement ReDraw()
        {
            Line line = new Line()
            {
                X1 = X,
                Y1 = Y,
                X2 = X,
                Y2 = Y,
                StrokeThickness = _strokeThickness,
                Stroke = _colorBrush,
            };

            return line;
        }

        public IShape Clone()
        {
            return new Point2D();
        }
    }
}
