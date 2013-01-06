using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct DeleteSingleShapeEvent
    {
        public int shapeId;
        public int topicId;
        public int indirectOwner;

        public static DeleteSingleShapeEvent Read(Dictionary<byte, object> par)
        {
            var res = new DeleteSingleShapeEvent();
            res.shapeId = (int) par[(byte) DiscussionParamKey.ShapeId];
            res.topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId];
            res.indirectOwner = (int) par[(byte) DiscussionParamKey.ShapeOwnerId];
            return res;
        }

        public static Dictionary<byte, object> Write(int shapeId, int topicId, int indirectOwner)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.ShapeId, shapeId);
            res.Add((byte) DiscussionParamKey.ChangedTopicId, topicId);
            res.Add((byte) DiscussionParamKey.ShapeOwnerId, indirectOwner);
            return res;
        }
    }
}