﻿using Contract;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
        private Matrix originalMatrix;  //matrix ban đầu của canvas, giúp phục hồi trạng thái zoom ban đầu

        bool _isDrawing = false;
        string _selectedShapeName = "";
        List<IShape> _shapes = new List<IShape>();  //Danh sách các shape được vẽ trên canvas
        List<IShape> _undidShapes = new List<IShape>(); //Danh sách các shape bị undo
        IShape _preview;
        Dictionary<string, IShape> _shapePrototypes = new Dictionary<string, IShape>();
        private bool _isSelectingShape; //Người dùng có đang chọn một shape trên canvas hay không

        public MainWindow()
        {
            InitializeComponent();

            //Lưu matrix ban đầu của canvas
            var matrixTransform = canvas.RenderTransform as MatrixTransform;
            if (matrixTransform != null) originalMatrix = matrixTransform.Matrix;
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

        #region Canvas Mouse Events

        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isSelectingShape)  //Nếu đang chọn shape trên canvas
            {
                if (_shapes.Count > 0)
                {
                    HitTestResult hitTestResult = VisualTreeHelper.HitTest(canvas, e.GetPosition(canvas));
                    if (hitTestResult != null && hitTestResult.VisualHit is UIElement element)
                    {
                        AdornerLayer myAdornerLayer = ClearAllAdorner();
                        if (myAdornerLayer != null)
                        {
                            //Thêm adorner vào UIElement được chọn trên canvas
                            if (element is Line)
                            {
                                myAdornerLayer.Add(new LineAdorner(element));
                            }
                            else
                            {
                                myAdornerLayer.Add(new RectAdorner(element));
                            }
                        }
                    }
                }
            }
            else  //Nếu đang vẽ shape
            {
                ClearAllAdorner();
                SaveAllUIElementChangesToShape();

                _isDrawing = true;

                Point pos = e.GetPosition(canvas);

                _preview.HandleStart(pos.X, pos.Y);
            }
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
            if (_isDrawing)
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
        }

        //Xử lí zoom tại vị trí con trỏ chuột khi lăn
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var matTrans = canvas.RenderTransform as MatrixTransform;
            var pos1 = e.GetPosition(grid1);

            var scale = e.Delta > 0 ? 1.1 : 1 / 1.1;

            var mat = matTrans.Matrix;
            mat.ScaleAt(scale, scale, pos1.X, pos1.Y);
            matTrans.Matrix = mat;
            e.Handled = true;
        }

        #endregion

        #region QuickAccessItem Buttons on click events

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAllUIElementChangesToShape();

            if (_shapes.Count > 0)
            {
                _undidShapes.Add(_shapes[^1]);
                _shapes.RemoveAt(_shapes.Count - 1);

                RedrawAllShapes();
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            SaveAllUIElementChangesToShape();

            if (_undidShapes.Count > 0)
            {
                _shapes.Add(_undidShapes[^1]);
                _undidShapes.RemoveAt(_undidShapes.Count - 1);

                RedrawAllShapes();
            }
        }

        #endregion

        #region Tab Home-Group File: Buttons on click events

        private void btnSave_Clicked(object sender, RoutedEventArgs e)
        {
            SaveAllUIElementChangesToShape();

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
            SaveAllUIElementChangesToShape();

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
            SaveAllUIElementChangesToShape();

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

        #endregion

        #region Tab Home-Group Shape: Buttons on click events

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

            _isSelectingShape = false;
        }

        //Xử lí khi nhấn nút "chọn shape" trên ribbon
        private void ButtonSelectShape_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (Fluent.Button button in gbShapes.Items)
            {
                button.Background = Brushes.Transparent;
            }

            Fluent.Button btnSelect = sender as Fluent.Button;
            btnSelect.Background = Brushes.LightSkyBlue;

            _isSelectingShape = true;
        }

        #endregion

        #region Tab Home-Group Color: Buttons on click events

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

        #endregion

        #region Tab Home-Group Stroke: Buttons on click events

        private void sldThick_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _strokeThickness = (sender as Slider).Value;
        }

        #endregion

        #region Tab Home-Group Dash: Buttons on click events

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

        #endregion

        #region Tab View-Group Zoom: Buttons on click events


        //Zoom in khi nhấn chọn Zoom in button trên ribbon (mặc định zoom in tại điểm trung tâm)
        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            Point center = canvas.TransformToAncestor(grid1).Transform(new Point(canvas.ActualWidth / 2, canvas.ActualHeight / 2));

            var matTrans = canvas.RenderTransform as MatrixTransform;
            var mat = matTrans.Matrix;
            var scale = 1.1;
            mat.ScaleAt(scale, scale, center.X, center.Y);
            matTrans.Matrix = mat;
            e.Handled = true;
        }

        //Zoom in khi nhấn chọn Zoom in button trên ribbon (mặc định zoom out tại điểm trung tâm)
        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            Point center = canvas.TransformToAncestor(grid1).Transform(new Point(canvas.ActualWidth / 2, canvas.ActualHeight / 2));

            var matTrans = canvas.RenderTransform as MatrixTransform;
            var mat = matTrans.Matrix;
            var scale = 1 / 1.1;
            mat.ScaleAt(scale, scale, center.X, center.Y);
            matTrans.Matrix = mat;
            e.Handled = true;
        }

        //Trả về kích cỡ ban đầu khi nhấn chọn 100% zoom button trên ribbon
        private void NormalZoomButton_Click(object sender, RoutedEventArgs e)
        {
            var matTrans = canvas.RenderTransform as MatrixTransform;
            matTrans.Matrix = originalMatrix;
            e.Handled = true;
        }

        #endregion

        //Clear canvas và vẽ lại tất cả các shapes
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

        //Loại bỏ các adorner hiện có trên từng UIElement trên canvas.
        //Trả về adornderLayer trên canvas đã được xóa sạch adornder.
        private AdornerLayer ClearAllAdorner()
        {
            var myAdornerLayer = AdornerLayer.GetAdornerLayer(canvas);
            if (myAdornerLayer != null)
            {
                foreach (UIElement canvasChild in canvas.Children)
                {
                    var adorners = myAdornerLayer.GetAdorners(canvasChild);
                    if (adorners != null)
                    {
                        foreach (var adorner in adorners)
                        {
                            myAdornerLayer.Remove(adorner);
                        }
                    }
                }

                return myAdornerLayer;
            }

            return null;
        }

        //Lưu lại những thay đổi được thực hiện trên các UIElement trên canvas vào mảng shape
        private void SaveAllUIElementChangesToShape()
        {
            _shapes.Clear();

            foreach (UIElement element in canvas.Children)
            {
                foreach (var prototype in _shapePrototypes)
                {
                    IShape s = prototype.Value;
                    if (element.GetType() == s.GetUIElementType())  //Nếu cùng loại UIElement
                    {
                        _shapes.Add(s.Parse(element));
                        break;
                    }
                }
            }
        }
    }
}
