using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct UnselectAllRequest
    {
        public int ownerId;

        public static UnselectAllRequest Read(Dictionary<byte, object> par)
        {
            var res = new UnselectAllRequest();
            res.ownerId = (int) par[(byte) DiscussionParamKey.ShapeOwnerId];
            return res;
        }

        public static Dictionary<byte, object> Write(int ownerId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.ShapeOwnerId, ownerId);
            return res;
        }
    }
}