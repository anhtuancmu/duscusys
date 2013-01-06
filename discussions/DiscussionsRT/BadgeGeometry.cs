using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discussions.RTModel
{
    public class BadgeGeometry
    {
        //keep center of badge as defined by ScatterViewItem
        public double CenterX;
        public double CenterY;

        //keeps orientation of badge as defined by ScatterViewItem
        //http://msdn.microsoft.com/en-us/library/microsoft.surface.presentation.controls.scattercontentcontrolbase.orientation.aspx
        public double Orientation;
    }
}