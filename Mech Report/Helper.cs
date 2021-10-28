using System;
using System.Collections.Generic;
using System.Text;

namespace Mech_Report
{
    public class Helper
    {
        public static int[] guide_lines = { 500, 1000, 2000, 3000, 4000, 5000, 10000, 20000, 40000, 60000, 100000, 150000 };
        public static double DegreesToRadians(double degrees)
        {
            double radians = (Math.PI / 180.0) * degrees;
            return radians;
        }
    }
}
