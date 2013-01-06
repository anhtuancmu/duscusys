using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DistributedEditor
{
    internal class Utils
    {
        public static System.Windows.Media.Color IntToColor(int iCol)
        {
            return Color.FromArgb((byte) (iCol >> 24),
                                  (byte) (iCol >> 16),
                                  (byte) (iCol >> 8),
                                  (byte) (iCol));
        }
    }
}