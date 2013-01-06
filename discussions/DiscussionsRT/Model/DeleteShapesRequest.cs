using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct DeleteShapesRequest
    {
        public int ownerId;
        public int initialOwnerId;
        public int topicId;

        public static DeleteShapesRequest Read(Dictionary<byte, object> par)
        {
            var res = new DeleteShapesRequest();
            res.ownerId = (int) par[(byte) DiscussionParamKey.ShapeOwnerId];
            res.initialOwnerId = (int) par[(byte) DiscussionParamKey.InitialShapeOwnerId];
            res.topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId];
            return res;
        }

        public static Dictionary<byte, object> Write(int ownerId, int initialOwnerId, int topicId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.ShapeOwnerId, ownerId);
            res.Add((byte) DiscussionParamKey.InitialShapeOwnerId, initialOwnerId);
            res.Add((byte) DiscussionParamKey.ChangedTopicId, topicId);
            return res;
        }
    }
}