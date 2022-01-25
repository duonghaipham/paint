using System;
using System.Windows;
using System.Windows.Media;

namespace Contract
{
    public interface IShape
    {
        string Name { get; }
        string Icon { get; }
        
        void HandleStart(double x, double y);
        void HandleFinish(double x, double y);

        //Trả về Loại UIElement có thể được Parse
        Type GetUIElementType();

        UIElement Draw(
            SolidColorBrush colorBrush,
            double strokeThickness,
            PenLineCap strokeDashCap,
            int gapSize,
            int dashSize
        );
        UIElement ReDraw();
        IShape Clone();

        //Nhận vào UIElement và trả về IShape có các thuộc tính tương ứng
        IShape Parse(UIElement element);

        byte[] Serialize();
        IShape Deserialize(byte[] data);
    }
}
