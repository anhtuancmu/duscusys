using System.Collections.Generic;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public class PdfScrollPosition
    {
        public int ownerId;
        public double X;
        public double Y;
        public float Zoom; 
        public int topicId;

        public static PdfScrollPosition Read(Dictionary<byte, object> par)
        {
            var res = new PdfScrollPosition
            {
                ownerId = (int) par[(byte) DiscussionParamKey.ShapeOwnerId],
                X = (double)par[(byte)DiscussionParamKey.OffsetXKey],
                Y = (double)par[(byte)DiscussionParamKey.OffsetYKey],
                Zoom = (float)par[(byte)DiscussionParamKey.Zoom],
                topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId]
            };
            return res;
        }

        public static Dictionary<byte, object> Write(int ownerId,
                                                     double X,
                                                     double Y,
                                                     float Zoom,                                         
                                                     int topicId)
        {
            var res = new Dictionary<byte, object>
            {
                {(byte) DiscussionParamKey.ShapeOwnerId, ownerId},
                {(byte) DiscussionParamKey.OffsetXKey, X},
                {(byte) DiscussionParamKey.OffsetYKey, Y},
                {(byte) DiscussionParamKey.Zoom, Zoom},
                {(byte) DiscussionParamKey.ChangedTopicId, topicId}
            };
            return res;
        }

        public Dictionary<byte, object> ToDict()
        {
            return Write(ownerId, X, Y, Zoom, topicId);
        }
    }
}