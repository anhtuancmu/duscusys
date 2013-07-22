using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Discussions.DbModel;
using LoginEngine;

namespace DistributedEditor
{
    class DaoUtils
    {
        //used for coloring shapes in graphics editor after users
        public static Color UserIdToColor(int id)
        {
            if (id==-1)
                return Colors.AliceBlue;
            Person p = DbCtx.Get().Person.FirstOrDefault(p0 => p0.Id == id);
            if (p == null)
                return Colors.AliceBlue;
            else
                return Utils.IntToColor(p.Color);
        }

        public static bool ArgPointInTopic(int apId, int topicId)
        {
            var ap = DbCtx.Get().ArgPoint.FirstOrDefault(p0 => p0.Id == apId);
            if (ap == null)
                return false;

            if (ap.Topic == null)
                return false;

            return ap.Topic.Id == topicId;
        }
    }
}
