using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct DeleteSingleShapeRequest
    {
        public int ownerId;
        public int shapeId;
        public int topicId;

        public static DeleteSingleShapeRequest Read(Dictionary<byte, object> par)
        {
            var res = new DeleteSingleShapeRequest(); 
            res.ownerId  = (int)par[(byte)DiscussionParamKey.ShapeOwnerId];
            res.shapeId = (int)par[(byte)DiscussionParamKey.ShapeId];
            res.topicId = (int)par[(byte)DiscussionParamKey.ChangedTopicId]; 
            return res;
        }

        public static Dictionary<byte, object> Write(int ownerId, int shapeId, int topicId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte)DiscussionParamKey.ShapeOwnerId, ownerId);
            res.Add((byte)DiscussionParamKey.ShapeId, shapeId);
            res.Add((byte)DiscussionParamKey.ChangedTopicId, topicId);
            return res;      
        }
    }
}
