using System.IO;
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
        private PenLineCap _strokeDashCap = PenLineCap.Flat;

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

        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(Name);
                    writer.Write(X);
                    writer.Write(Y);
                    writer.Write(_colorBrush);
                    writer.Write(_strokeThickness);
                    writer.Write(_strokeDashCap);
                }
                return m.ToArray();
            }
        }

        public IShape Deserialize(byte[] data)
        {
            throw new System.NotImplementedException();
        }
    }
}
