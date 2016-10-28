using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointVisualizer
{
    public class Point3D
    {
        public double x { get; private set; }
        public double y { get; private set; }
        public double z { get; private set; }
        public Mogre.ColourValue color { get; set; }

        public Point3D(double x, double y, double z, Mogre.ColourValue c)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.color = c;
        }

        public Point3D(double x, double y, double z, Color color)
        {
            this.x = x;
            this.y = y;
            this.z = z;
         
            this.color = new Mogre.ColourValue((float)color.R / 255f,
                                               (float)color.G / 255f,
                                               (float)color.B / 255f);

        }
    }
}
