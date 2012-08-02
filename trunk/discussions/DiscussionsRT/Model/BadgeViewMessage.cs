using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct BadgeViewMessage
    {
        public int argPointId;
        public bool doExpand;

        public static BadgeViewMessage Read(Dictionary<byte, object> par)
        {
            var res = new BadgeViewMessage();
            res.argPointId = (int)par[(byte)DiscussionParamKey.ArgPointId];
            res.doExpand   = (bool)par[(byte)DiscussionParamKey.BoolParameter1];
            return res;
        }

        public static Dictionary<byte, object> Write(int argPointId, bool doExpand)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte)DiscussionParamKey.ArgPointId, argPointId);
            res.Add((byte)DiscussionParamKey.BoolParameter1, doExpand); 
            return res;      
        }
    }
}
