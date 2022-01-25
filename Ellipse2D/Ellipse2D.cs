using Contract;
using System;
using System.IO;
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

        public Type GetUiElementType()
        {
            return typeof(Ellipse);
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

        //Dãy byte[] được trả về có nội dung:
        //Chiều dài nội dung - Name - [_start] - [_finish] - colorBrush - strokeThickness - strokeDashCap - gapSize - dashSize
        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(m))
                {
                    writer.Write(_start.Serialize());
                    writer.Write(_finish.Serialize());
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
            Ellipse2D result = new Ellipse2D();
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
                    long sizeOfFinish = reader.ReadInt64();
                    name = reader.ReadString(); //read the name "Point"
                    result._finish = result._finish.Deserialize(reader.ReadBytes((int)sizeOfFinish)) as Point2D;

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
