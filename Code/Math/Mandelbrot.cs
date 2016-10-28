using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Numerics;

namespace PointVisualizer
{
    class Mandelbrot
    {
        private const int MAX = 5000;
        private const double EPSILON = 0.5d;
        private static Color[] colors = new Color[MAX + 1];

        // Static Constructor
        static Mandelbrot()
        {
            for (int i = 0; i < MAX; i++)
            {
                double d = (double)i;

                double r = d / (double)MAX;
                double g = r;
                double b = d / (d + 8d);

                int ri = (int)(r * 255);
                int gi = (int)(g * 255);
                int bi = (int)(b * 255);

                colors[i] = Color.FromArgb(bi, gi, ri);
            }
        }


        // return number of iterations to check if c = a + ib is in Mandelbrot set
        public static int mand(Complex z0)
        {
            Complex z = z0;

            for (int t = 0; t < MAX; t++)
            {
                // if (z.abs() > 2.0) return t;
                // z = z.times(z).plus(z0);

                if (z.Magnitude > 2.0)
                {
                    return t;
                }
                z = z * z + z0;
            }
            return MAX;
        }

        public static List<Point3D> DoPlot(double size)  
        {
            List<Point3D> points = new List<Point3D>();

            double xc = size / 2d;
            double yc = size / 2d;
            

            for (double i = 0; i < size; i += EPSILON)
            {
                for (double j = 0; j < size; j += EPSILON)
                {
                    double x0 = (i - xc) / 200d;
                    double y0 = (j - yc) / 200d;

                    Complex z0 = new Complex(x0, y0);

                    int index = mand(z0);

                    // Only plot values that aren't MAX  (ie, in the set)
                    if (index < MAX)
                    {
                        Color color = colors[index];
                        Point3D sp = new Point3D(i - xc, j - yc, index/10d, color);
                        points.Add(sp);
                    }
                }
            }

            return points;
        }
    }
}