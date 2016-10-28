using Mogre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointVisualizer
{
    class Conversions
    {
        public static Vector3 FromSphericalRadians(float r, float theta, float phi)
        {
            float snt = (float)System.Math.Sin(theta);
            float cnt = (float)System.Math.Cos(theta);
            float snp = (float)System.Math.Sin(phi);
            float cnp = (float)System.Math.Cos(phi);

            float x = r * snt * cnp;
            float y = r * snp * snt;
            float z = r * snt;

            return new Vector3(x, y, -z);
        }

        public static Vector3 CamFromSphericalRadians(float r, float az, float el)
        {
            float x = r * (float)System.Math.Sin(az);
            float y = r * (float)System.Math.Sin(el);
            float w = (float)System.Math.Sqrt(x * x + y * y);
            float z = (float)System.Math.Sqrt(r * r - w * w);

            return new Vector3(x, -y, -z);  // Negate Y for azimuth convention, negate Z for Ogre convention
        }

        public static Vector3 FromCylindricalRadians(float r, float theta, float zz)
        {
            float snt = (float)System.Math.Sin(theta);
            float cnt = (float)System.Math.Cos(theta);

            float x = r * cnt;
            float y = r * snt;

            return new Vector3(x, zz, -y);
        }
    }
}
