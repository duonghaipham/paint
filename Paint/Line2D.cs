using System.IO;
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

        //Dãy byte[] được trả về có nội dung:
        //Chiều dài nội dung - Name - [_start] - [_end] - colorBrush - strokeThickness - strokeDashCap - gapSize - dashSize
        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(_start.Serialize());
                    writer.Write(_end.Serialize());
                    writer.Write(_colorBrush.ToString());
                    writer.Write(_strokeThickness);
                    writer.Write(_strokeDashCap.ToString());
                    writer.Write(_gapSize);
                    writer.Write(_dashSize);

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

        //Dãy byte[] nhận vào có nội dung:
        //[_start] - [_end] - colorBrush - strokeThickness - strokeDashCap - gapSize - dashSize
        public IShape Deserialize(byte[] data)
        {
            Line2D result = new Line2D();
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(m))
                {
                    //these 2 lines for debug purpose only. Comment them out before release
                    //long size = reader.ReadInt64();
                    //string lineName = reader.ReadString();

                    //deserialize Point2D start
                    long sizeOfStart = reader.ReadInt64();
                    string name = reader.ReadString(); //read the name "Point"
                    byte[] read = reader.ReadBytes((int)sizeOfStart);
                    result._start = result._start.Deserialize(read) as Point2D;

                    //deserialize Point2D end
                    long sizeOfEnd = reader.ReadInt64();
                    name = reader.ReadString(); //read the name "Point"
                    result._end = result._end.Deserialize(reader.ReadBytes((int)sizeOfEnd)) as Point2D;

                    //deserialize other attributes
                    BrushConverter brushConverter = new BrushConverter();
                    result._colorBrush = brushConverter.ConvertFromString(reader.ReadString()) as SolidColorBrush;
                    result._strokeThickness = reader.ReadDouble();
                    result._strokeDashCap = parsePenLineCap(reader.ReadString());
                    result._gapSize = reader.ReadInt32();
                    result._dashSize = reader.ReadInt32();
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
