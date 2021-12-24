using Contract;
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
                    UIElement element = shape.Draw();
                    canvas.Children.Add(element);
                }

                // Vẽ hình preview đè lên
                canvas.Children.Add(_preview.Draw());
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
                var element = shape.Draw();
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
    }
}
