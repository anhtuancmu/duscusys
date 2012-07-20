using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discussions.RTModel
{
    public class Normalizer
    {
        public static double NormX(double w, double x)
        {
            return x / w;
        }

        public static double NormY(double h, double y)
        {
            return y / h;
        }       
    }
}
