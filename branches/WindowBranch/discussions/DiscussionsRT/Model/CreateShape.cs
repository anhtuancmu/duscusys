using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;
using DistributedEditor;

namespace Discussions.RTModel.Model
{
    public struct CreateShape
    {
        public int ownerId;
        public int shapeId;
        public VdShapeType shapeType;
        public double startX;
        public double startY;
        public bool takeCursor;
        public int tag; // only for badges, DB Id of ArgPoint
        public int topicId;

        public static CreateShape Read(Dictionary<byte, object> par)
        {
            var res = new CreateShape();
            res.ownerId = (int) par[(byte) DiscussionParamKey.ShapeOwnerId];
            res.shapeId = (int) par[(byte) DiscussionParamKey.ShapeId];
            res.shapeType = (VdShapeType) par[(byte) DiscussionParamKey.ShapeType];
            res.startX = (double) par[(byte) DiscussionParamKey.AnchorX];
            res.startY = (double) par[(byte) DiscussionParamKey.AnchorY];
            res.takeCursor = (bool) par[(byte) DiscussionParamKey.AutoTakeCursor];
            res.tag = (int) par[(byte) DiscussionParamKey.Tag];
            res.topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId];
            return res;
        }

        public static Dictionary<byte, object> Write(int ownerId, int shapeId, VdShapeType shapeType,
                                                     double startX, double startY,
                                                     bool takeCursor, int tag,
                                                     int topicId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.ShapeOwnerId, ownerId);
            res.Add((byte) DiscussionParamKey.ShapeId, shapeId);
            res.Add((byte) DiscussionParamKey.ShapeType, shapeType);
            res.Add((byte) DiscussionParamKey.AnchorX, startX);
            res.Add((byte) DiscussionParamKey.AnchorY, startY);
            res.Add((byte) DiscussionParamKey.AutoTakeCursor, takeCursor);
            res.Add((byte) DiscussionParamKey.Tag, tag);
            res.Add((byte) DiscussionParamKey.ChangedTopicId, topicId);
            return res;
        }
    }
}