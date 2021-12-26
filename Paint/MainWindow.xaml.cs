﻿using Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        private SolidColorBrush _color1 = Brushes.Black;
        private SolidColorBrush _color2 = Brushes.White;
        private double _strokeThickness = 1;
        private PenLineCap _strokeDashCap = PenLineCap.Flat;
        public int _gapSize = 0;
        public int _dashSize = 1;

        public MainWindow()
        {
            InitializeComponent();
        }

        bool _isDrawing = false;
        string _selectedShapeName = "";
        List<IShape> _shapes = new List<IShape>();
        IShape _preview;
        Dictionary<string, IShape> _shapePrototypes = new Dictionary<string, IShape>();

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = true;

            Point pos = e.GetPosition(canvas);

            _preview.HandleStart(pos.X, pos.Y);
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                Point pos = e.GetPosition(canvas);
                _preview.HandleFinish(pos.X, pos.Y);

                // Xoá hết các hình vẽ cũ
                canvas.Children.Clear();

                // Vẽ lại các hình trước đó
                foreach (var shape in _shapes)
                {
                    UIElement element = shape.ReDraw();
                    canvas.Children.Add(element);
                }

                // Vẽ hình preview đè lên
                canvas.Children.Add(_preview.Draw(_color1, _strokeThickness, _strokeDashCap, _gapSize, _dashSize));
            }
        }

        private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;

            // Thêm đối tượng cuối cùng vào mảng quản lí
            Point pos = e.GetPosition(canvas);
            _preview.HandleFinish(pos.X, pos.Y);
            _shapes.Add(_preview);

            // Sinh ra đối tượng mẫu kế
            _preview = _shapePrototypes[_selectedShapeName].Clone();

            // Ve lai Xoa toan bo
            canvas.Children.Clear();

            // Ve lai tat ca cac hinh
            foreach (var shape in _shapes)
            {
                var element = shape.ReDraw();
                canvas.Children.Add(element);
            }
        }

        private void winMain_Loaded(object sender, RoutedEventArgs e)
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string folder = System.IO.Path.GetDirectoryName(exePath);
            FileInfo[] fis = new DirectoryInfo(folder).GetFiles("*.dll");

            foreach (FileInfo fileInfo in fis)
            {
                var domain = AppDomain.CurrentDomain;
                Assembly assembly = Assembly.Load(AssemblyName.GetAssemblyName(fileInfo.FullName));

                Type[] types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass)
                    {
                        if (typeof(IShape).IsAssignableFrom(type) && type != typeof(Point2D))
                        {
                            var shape = Activator.CreateInstance(type) as IShape;
                            _shapePrototypes.Add(shape.Name, shape);
                        }
                    }
                }
            }

            // Tạo ra các nút bấm hàng mẫu
            foreach (var item in _shapePrototypes)
            {
                var shape = item.Value;

                var btnShape = new Fluent.Button()
                {
                    Tag = shape.Name,
                    LargeIcon = shape.Icon,
                    SizeDefinition = "Middle"
                };

                btnShape.Click += btnShape_Click;
                gbShapes.Items.Add(btnShape);
            }

            _selectedShapeName = _shapePrototypes.First().Value.Name;
            _preview = _shapePrototypes[_selectedShapeName].Clone();

            btnColor1Chooser.Background = _color1;
            btnColor2Chooser.Background = _color2;

            DataContext = this;
        }

        private void btnShape_Click(object sender, RoutedEventArgs e)
        {
            Fluent.Button btnShape = sender as Fluent.Button;

            foreach (Fluent.Button button in gbShapes.Items)
            {
                button.Background = Brushes.Transparent;
            }

            btnShape.Background = Brushes.LightSkyBlue;

            _selectedShapeName = btnShape.Tag as string;

            _preview = _shapePrototypes[_selectedShapeName].Clone();
        }

        private void btnColor1Chooser_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _color1 = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                btnColor1Chooser.Background = _color1;
            }
        }

        private void btnColor2Chooser_Click(object sender, RoutedEventArgs e)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _color2 = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
                btnColor2Chooser.Background = _color2;
            }
        }

        private void sldThick_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _strokeThickness = (sender as Slider).Value;
        }

        private void btnDashNone_Checked(object sender, RoutedEventArgs e)
        {
            _strokeDashCap = PenLineCap.Flat;
        }

        private void btnDashFlat_Checked(object sender, RoutedEventArgs e)
        {
            _strokeDashCap = PenLineCap.Flat;
        }

        private void btnDashSquare_Checked(object sender, RoutedEventArgs e)
        {
            _strokeDashCap = PenLineCap.Square;
        }

        private void btnDashTriangle_Checked(object sender, RoutedEventArgs e)
        {
            _strokeDashCap = PenLineCap.Triangle;
        }

        private void btnDashRound_Checked(object sender, RoutedEventArgs e)
        {
            _strokeDashCap = PenLineCap.Round;
        }

        private void iudGapSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _gapSize = (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.GetValueOrDefault();
        }

        private void iudDashSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _dashSize = (sender as Xceed.Wpf.Toolkit.IntegerUpDown).Value.GetValueOrDefault();
        }
    }
}
