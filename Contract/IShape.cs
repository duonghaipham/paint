using System;
using System.Windows;

namespace Contract
{
    public interface IShape
    {
        string Name { get; }
        string Icon { get; }

        void HandleStart(double x, double y);
        void HandleFinish(double x, double y);

        UIElement Draw();
        IShape Clone();
    }
}
