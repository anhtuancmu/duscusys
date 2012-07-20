using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discussions.RTModel.Model
{
    public class TestHelper
    {
        static Random rnd = new Random();

        //public
        static SharedView[] CreateBadges(int numBadges)
        {
            SharedView[] result = new SharedView[numBadges];
            for (int i = 0; i < numBadges; ++i)
            {
                var argPointView = new SharedView(i,true);
                argPointView.badgeGeometry.CenterX = rnd.NextDouble();
                argPointView.badgeGeometry.CenterY = rnd.NextDouble();
                argPointView.badgeGeometry.Orientation = 360.0 * rnd.NextDouble();
                result[i] = argPointView;
            }
            return result;
        }
    }
}
