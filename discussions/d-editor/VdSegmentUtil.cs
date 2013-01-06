using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows;

namespace DistributedEditor
{
    public class VdSegmentUtil
    {
        public enum SegmentMarker
        {
            Side1,
            Side2,
            Center
        };

        public static void ShowMarkers(Shape selMarker1, Shape selMarker2)
        {
            selMarker1.Visibility = Visibility.Visible;
            selMarker2.Visibility = Visibility.Visible;
        }

        public static void HideMarkers(Shape selMarker1, Shape selMarker2)
        {
            selMarker1.Visibility = Visibility.Hidden;
            selMarker2.Visibility = Visibility.Hidden;
        }
    }
}