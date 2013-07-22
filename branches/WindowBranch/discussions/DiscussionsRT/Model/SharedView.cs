using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discussions.RTModel
{
    //keeps shared views of arg.points and group badges
    public class SharedView
    {
        public bool viewType = true; //if true, it's point view
        public int ViewId; //Id of the point or group in DB
        public bool badgeGeometryInitialized = false;
        public BadgeGeometry badgeGeometry = new BadgeGeometry();

        public SharedView(int ViewId, bool pointView)
        {
            this.ViewId = ViewId;
            this.viewType = pointView;
        }
    }
}