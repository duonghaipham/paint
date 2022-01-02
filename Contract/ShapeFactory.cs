using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Shapes;

namespace Contract
{
    public class ShapeFactory
    {
        private static ShapeFactory _instance = null;
        private Dictionary<string, IShape> _prototypes = new Dictionary<string, IShape>();

        public static ShapeFactory GetInstance()
        {
            if (_instance == null)
            {
                _instance = new ShapeFactory();
            }

            return _instance;
        }

        private ShapeFactory()
        {
            //Load các dlls vào _prototypes
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
                            _prototypes.Add(shape.Name, shape);
                        }
                    }
                }
            }
        }

        public Dictionary<string, IShape> GetPrototype()
        {
            return _prototypes;
        }
    }
}
