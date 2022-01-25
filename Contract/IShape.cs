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

        //Trả về Loại UIElement được sử dụng trong phương thức ReDraw để vẽ
        Type GetUiElementType();

        UIElement Draw(
            SolidColorBrush colorBrush,
            double strokeThickness,
            PenLineCap strokeDashCap,
            int gapSize,
            int dashSize
        );
        UIElement ReDraw();
        IShape Clone();
        //IShape Parse(UIElement element);
        byte[] Serialize();
        IShape Deserialize(byte[] data);
    }
}
