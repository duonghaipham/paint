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
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Paint
{
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
            ZoomViewbox.Width = 100;
            ZoomViewbox.Height = 100;
            canvas.Width = ZoomViewbox.Width;
            canvas.Height = ZoomViewbox.Height;
        }

        bool _isDrawing = false;
        string _selectedShapeName = "";
        List<IShape> _shapes = new List<IShape>();  //Danh sách các shape được vẽ trên canvas
        List<IShape> _undidShapes = new List<IShape>(); //Danh sách các shape bị undo
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

                // Vẽ lại các hình trước đó
                RedrawAllShapes();

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

            // Ve lai tat ca cac hinh
            RedrawAllShapes();
        }

        private void winMain_Loaded(object sender, RoutedEventArgs e)
        {
            //Lấy các prototypes từ factory
            _shapePrototypes = ShapeFactory.GetInstance().GetPrototype();

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

        private void btnSave_Clicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG (*.png)|*.png";

            if (saveFileDialog.ShowDialog() == true)
            {
                Rect rect = new Rect(canvas.RenderSize);
                RenderTargetBitmap renderTargetBitmap = 
                    new RenderTargetBitmap((int)rect.Right, (int)rect.Bottom, 96d, 96d, PixelFormats.Default);
                renderTargetBitmap.Render(canvas);

                BitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

                MemoryStream memoryStream = new MemoryStream();

                pngEncoder.Save(memoryStream);
                memoryStream.Close();

                File.WriteAllBytes(saveFileDialog.FileName, memoryStream.ToArray());
            }
        }

        private void btnSaveCanvas_Clicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PPF (*.ppf)|*.ppf";
            saveFileDialog.FileName = "ppfCanvas";

            if (saveFileDialog.ShowDialog() == true)
            {
                //mở file .ppf đã lưu để ghi
                string fileName = saveFileDialog.FileName;
                using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(fileName, FileMode.Create)))
                {
                    foreach (IShape shape in _shapes)
                    {
                        binaryWriter.Write(shape.Serialize());
                    }
                }
            }
        }

        private void btnOpenCanvas_Clicked(object sender, RoutedEventArgs e)
        {
            //Hỏi người dùng có muốn lưu hình vẽ hiện tại trước khi chọn mở file.
            if (_shapes.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show(
                    "This canvas is not empty.\nDo you want to save it before open new one? (Yes/No)",
                    "Waring", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    btnSaveCanvas_Clicked(null, null);
                    return;
                }
            }

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "PPF (*.ppf)|*.ppf";

            if (openFile.ShowDialog() == true)
            {
                //Clear danh sách shapes và canvas
                _shapes.Clear();
                canvas.Children.Clear();

                FileStream file = File.Open(openFile.FileName, FileMode.Open);
                using (BinaryReader binaryReader = new BinaryReader(file))
                {
                    //Đọc đến khi hết file. Mỗi lần đọc thì parse ra shape và thêm vào list _shapes
                    while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                    {
                        long size = binaryReader.ReadInt64();
                        string name = binaryReader.ReadString();
                        byte[] data = binaryReader.ReadBytes((int)size);

                        IShape shape = _shapePrototypes[name].Deserialize(data);
                        _shapes.Add(shape);
                    }
                }
            }

            RedrawAllShapes();
        }

        private void RedrawAllShapes()
        {
            canvas.Children.Clear();

            // Ve lai tat ca cac hinh
            foreach (var shape in _shapes)
            {
                var element = shape.ReDraw();
                canvas.Children.Add(element);
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            if (_shapes.Count > 0)
            {
                _undidShapes.Add(_shapes[^1]);
                _shapes.RemoveAt(_shapes.Count - 1);

                RedrawAllShapes();
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            if (_undidShapes.Count > 0)
            {
                _shapes.Add(_undidShapes[^1]);
                _undidShapes.RemoveAt(_undidShapes.Count - 1);
                
                RedrawAllShapes();
            }
        }

        private void MainWindow_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UpdateViewBox((e.Delta > 0) ? 10 : -10);
        }

        private double minWidth = 10;
        private double maxWidth = 400;
        private void UpdateViewBox(int newValue)
        {
            double newWidth = ZoomViewbox.Width + newValue;
            double newHeight = ZoomViewbox.Width + newValue;

            if (newWidth >= minWidth && newWidth <= maxWidth)
            {
                if (newWidth >= 0 && newHeight >= 0)
                {
                    ZoomViewbox.Width = newWidth;
                    ZoomViewbox.Height = newHeight;
                }
            }
        }
    }
}
