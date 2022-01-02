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
            Line line = new Line
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

        //Dãy byte[] được trả về có nội dung:
        //Chiều dài nội dung - Name - X - Y - colorBrush - strokeThickness - strokeDashCap
        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(X);
                    writer.Write(Y);
                    writer.Write(_colorBrush.ToString());
                    writer.Write(_strokeThickness);
                    writer.Write(_strokeDashCap.ToString());

                    //Thêm chiều dài nội dung đã ghi & Name vào đầu memory stream
                    using (MemoryStream m1 = new MemoryStream())
                    {
                        using (BinaryWriter writer1 = new BinaryWriter(m1))
                        {
                            writer1.Write(m.Length);
                            writer1.Write(Name);
                            writer1.Write(m.ToArray());
                            return m1.ToArray();
                        }
                    }
                }
            }
        }

        //Dãy byte[] nhận vào cần có nội dung:
        //X - Y - colorBrush - strokeThickness - strokeDashCap
        public IShape Deserialize(byte[] data)
        {
            Point2D result = new Point2D();
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    double x = reader.ReadDouble();
                    double y = reader.ReadDouble();
                    result.HandleStart(x, y);

                    BrushConverter brushConverter = new BrushConverter();
                    result._colorBrush = brushConverter.ConvertFromString(reader.ReadString()) as SolidColorBrush;
                    result._strokeThickness = reader.ReadDouble();
                    result._strokeDashCap = parsePenLineCap(reader.ReadString());
                }

                return result;
            }
        }

        private PenLineCap parsePenLineCap(string choice)
        {
            switch (choice)
            {
                case "Round":
                    return PenLineCap.Round;
                case "Square":
                    return PenLineCap.Square;
                case "Triangle":
                    return PenLineCap.Triangle;
                default:
                    return PenLineCap.Flat;
            }
        }
    }
}
