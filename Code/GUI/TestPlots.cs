using Mogre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PointVisualizer
{
    public partial class OgreForm
    {
        private void PlotSpiral()
        {
            // Origin
            Point3D sp1 = new Point3D(0, 0, 0, ColourValue.White);
            List<Point3D> points = new List<Point3D>();
            points.Add(sp1);

            for (float r1 = 10; r1 < 400; r1 += 1f)
            {
                for (float t1 = -PI; t1 <= PI; t1 += 0.01f)
                {
                    double x = r1 * System.Math.Sin(t1);
                    double y = r1 * System.Math.Cos(t1);
                    double z = t1 * 100;

                    ColourValue c = new ColourValue(1 - (t1 / PI), 0, 0);

                    points.Add(new Point3D(x, y, z, c));
                }
            }

            UpdateCloudPoints(points, 100000000);
        }

        private void PlotMandelbrot()
        {
            List<Point3D> points = new List<Point3D>();

            points.AddRange(Mandelbrot.DoPlot(1000d));

            // Origin
            Point3D sp1 = new Point3D(0, 0, 0, ColourValue.White);
            points.Add(sp1);

            UpdateCloudPoints(points, 100000000);
        }

        private void PlotLines()
        {
            List<Point3D> points = new List<Point3D>();

            // Origin
            Point3D sp1 = new Point3D(0, 0, 0, ColourValue.Red);
            points.Add(sp1);

            UpdateCloudPoints(points, 100000000);

            try
            {
                manual.Begin("BaseWhiteNoLighting", RenderOperation.OperationTypes.OT_LINE_STRIP);
                manual.Colour(ColourValue.White);
                manual.Position(0, 0, 0);
                manual.Position(300, 200, 100);
                manual.Colour(ColourValue.Blue);
                manual.Position(300, -200, -100);
                manual.Colour(ColourValue.Red);
                manual.Position(100, -500, 600);
                manual.End();
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception when plotting points: " + e.Message);
                // ignore throw e;
            }
        }

        private void PlotRandom()
        {
            List<Point3D> points = new List<Point3D>();

            // Origin
            Point3D sp1 = new Point3D(0, 0, 0, ColourValue.Red);
            points.Add(sp1);

            UpdateCloudPoints(points, 100000000);

            Random rand = new Random();
            int SIZE = 10000;

            try
            {
                manual.Begin("BaseWhiteNoLighting", RenderOperation.OperationTypes.OT_POINT_LIST);
                manual.Colour(ColourValue.White);
                manual.Position(0, 0, 0);

                for (int i = 0; i < 50000; i++)
                {
                    float x = (float)rand.NextDouble() * SIZE - (SIZE / 2);
                    float y = (float)rand.NextDouble() * SIZE - (SIZE / 2);
                    float z = (float)rand.NextDouble() * SIZE - (SIZE / 2);

                    float r = (float)rand.NextDouble();
                    float g = (float)rand.NextDouble();
                    float b = (float)rand.NextDouble();

                    manual.Position(x, y, z);
                    manual.Colour(new ColourValue(r, g, b));
                }
                manual.End();
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception when plotting points: " + e.Message);
                // ignore throw e;
            }
        }

    }
}
