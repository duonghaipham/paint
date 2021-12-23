using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        bool _isDrawing = false;
        double _lastX = -1;
        double _lastY = -1;
        int _choice = Shapes.Line;
        List<IShape> _shapes = new List<IShape>();
        IShape _preview = new Line2D();


        interface IShape
        {
            string Name { get; }
            void HandleStart(double x, double y);
            void HandleEnd(double x, double y);

            UIElement Draw();
        }

        class Point2D : IShape
        {
            public double X { get; set; }
            public double Y { get; set; }

            public string Name => "Point";

            public void HandleStart(double x, double y)
            {
                X = x;
                Y = y;
            }

            public void HandleEnd(double x, double y)
            {
                X = x;
                Y = y;
            }

            public UIElement Draw()
            {
                Line l = new Line()
                {
                    X1 = X,
                    Y1 = Y,
                    X2 = X,
                    Y2 = Y,
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(Colors.Red),
                };

                return l;
            }
        }

        class Line2D : IShape
        {
            private Point2D _start = new Point2D();
            private Point2D _end = new Point2D();

            public string Name => "Line";

            public void HandleStart(double x, double y)
            {
                _start = new Point2D() { X = x, Y = y };
            }

            public void HandleEnd(double x, double y)
            {
                _end = new Point2D() { X = x, Y = y };
            }

            public UIElement Draw()
            {
                Line l = new Line()
                {
                    X1 = _start.X,
                    Y1 = _start.Y,
                    X2 = _end.X,
                    Y2 = _end.Y,
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(Colors.Red),
                };

                return l;
            }
        }

        class Rectangle2D : IShape
        {
            private Point2D _leftTop = new Point2D();
            private Point2D _rightBottom = new Point2D();

            public string Name => "Rectangle";

            public UIElement Draw()
            {
                var rect = new Rectangle()
                {
                    Width = _rightBottom.X - _leftTop.X,
                    Height = _rightBottom.Y - _leftTop.Y,
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 1
                };

                Canvas.SetLeft(rect, _leftTop.X);
                Canvas.SetTop(rect, _leftTop.Y);

                return rect;
            }

            public void HandleStart(double x, double y)
            {
                _leftTop = new Point2D() { X = x, Y = y };
            }

            public void HandleEnd(double x, double y)
            {
                _rightBottom = new Point2D() { X = x, Y = y };
            }
        }

        class Ellipse2D : IShape
        {
            private Point2D _leftTop = new Point2D();
            private Point2D _rightBottom = new Point2D();

            public string Name => "Ellipse";

            public UIElement Draw()
            {
                var ellipse = new Ellipse()
                {
                    Width = _rightBottom.X - _leftTop.X,
                    Height = _rightBottom.Y - _leftTop.Y,
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(ellipse, _leftTop.X);
                Canvas.SetTop(ellipse, _leftTop.Y);

                return ellipse;
            }

            public void HandleStart(double x, double y)
            {
                _leftTop.X = x;
                _leftTop.Y = y;
            }

            public void HandleEnd(double x, double y)
            {
                _rightBottom.X = x;
                _rightBottom.Y = y;
            }
        }

        class Shapes
        {
            public const int Line = 1;
            public const int Rectangle = 2;
            public const int Ellipse = 3;
            public const int Square = 4;
            public const int Circle = 5;
        }

        private void lineButton_Click(object sender, RoutedEventArgs e)
        {
            _choice = Shapes.Line;
            _preview = new Line2D();
        }

        private void rectangleButton_Click(object sender, RoutedEventArgs e)
        {
            _choice = Shapes.Rectangle;
            _preview = new Rectangle2D();
        }

        private void ellipseButton_Click(object sender, RoutedEventArgs e)
        {
            _choice = Shapes.Ellipse;
            _preview = new Ellipse2D();
        }

        private void canvas_MouseDown(object sender,
            MouseButtonEventArgs e)
        {
            _isDrawing = true;

            Point pos = e.GetPosition(canvas);
            _lastX = pos.X;
            _lastY = pos.Y;

            _preview.HandleStart(pos.X, pos.Y);

            Title = "Mouse down";
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                Point pos = e.GetPosition(canvas);
                _preview.HandleEnd(pos.X, pos.Y);

                // Xoá hết các hình vẽ cũ
                canvas.Children.Clear();

                // Vẽ lại các hình trước đó
                foreach (var shape in _shapes)
                {
                    UIElement element = shape.Draw();
                    canvas.Children.Add(element);
                }

                // Vẽ hình preview đè lên
                canvas.Children.Add(_preview.Draw());

                Title = $"{pos.X} {pos.Y}";
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;

            // Thêm đối tượng cuối cùng vào mảng quản lí
            Point pos = e.GetPosition(canvas);
            _preview.HandleEnd(pos.X, pos.Y);
            _shapes.Add(_preview);

            if (_choice == Shapes.Line)
            {
                _preview = new Line2D(); // TODO
            }
            else if (_choice == Shapes.Rectangle)
            {
                _preview = new Rectangle2D();
            }
            else if (_choice == Shapes.Ellipse)
            {
                _preview = new Ellipse2D();
            }

            // Ve lai Xoa toan bo
            canvas.Children.Clear();

            // Ve lai tat ca cac hinh
            foreach (var shape in _shapes)
            {
                var element = shape.Draw();
                canvas.Children.Add(element);
            }

        }

        private void squareButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void circleButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
