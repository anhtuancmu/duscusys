using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct CursorEvent
    {
        public bool doSet;
        public int ownerId;
        public int shapeId; // undefined for unset
        public int topicId;

        public static CursorEvent Read(Dictionary<byte, object> par)
        {
            var res = new CursorEvent();
            res.doSet = (bool) par[(byte) DiscussionParamKey.BoolParameter1];
            res.ownerId = (int) par[(byte) DiscussionParamKey.UserCursorUsrId];
            res.shapeId = (int) par[(byte) DiscussionParamKey.ShapeId];
            res.topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId];
            return res;
        }

        public static Dictionary<byte, object> Write(bool doSet, int ownerId, int shapeId, int topicId,
                                                     int targetClient = -1)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.BoolParameter1, doSet);
            res.Add((byte) DiscussionParamKey.UserCursorUsrId, ownerId);
            res.Add((byte) DiscussionParamKey.ShapeId, shapeId);
            res.Add((byte) DiscussionParamKey.ChangedTopicId, topicId);
            return res;
        }
    }
}