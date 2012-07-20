using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct LinkCreateMessage
    {
        public int end1Id;
        public int end2Id;
        public int ownerId;  
        public int shapeId;
        public int topicId;
        public bool takeCursor;
       
        public static LinkCreateMessage Read(Dictionary<byte, object> par)
        {
            var res = new LinkCreateMessage();
            res.end1Id = (int)par[(byte)DiscussionParamKey.LinkEnd1Id];
            res.end2Id = (int)par[(byte)DiscussionParamKey.LinkEnd2Id];
            res.ownerId = (int)par[(byte)DiscussionParamKey.InitialShapeOwnerId];
            res.shapeId = (int)par[(byte)DiscussionParamKey.ShapeId];
            res.topicId = (int)par[(byte)DiscussionParamKey.ChangedTopicId];
            res.takeCursor = (bool)par[(byte)DiscussionParamKey.AutoTakeCursor];
            return res;
        }

        public static Dictionary<byte, object> Write(int end1Id, int end2Id, int ownerId,
                                                     int shapeId, int topicId, bool takeCursor)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte)DiscussionParamKey.LinkEnd1Id, end1Id);
            res.Add((byte)DiscussionParamKey.LinkEnd2Id, end2Id);
            res.Add((byte)DiscussionParamKey.InitialShapeOwnerId, ownerId);
            res.Add((byte)DiscussionParamKey.ShapeId, shapeId);
            res.Add((byte)DiscussionParamKey.ChangedTopicId, topicId);
            res.Add((byte)DiscussionParamKey.AutoTakeCursor, takeCursor);   
            return res;      
        }
    }
}
