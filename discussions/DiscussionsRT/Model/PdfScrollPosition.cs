using System.Collections.Generic;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public class PdfScrollPosition
    {
        public int ownerId;
        public int Y;        
        public int topicId;

        public static PdfScrollPosition Read(Dictionary<byte, object> par)
        {
            var res = new PdfScrollPosition
            {
                ownerId = (int) par[(byte) DiscussionParamKey.ShapeOwnerId],
                Y = (int)par[(byte) DiscussionParamKey.OffsetYKey],
                topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId]
            };
            return res;
        }

        public static Dictionary<byte, object> Write(int ownerId,
                                                     int Y,                                                     
                                                     int topicId)
        {
            var res = new Dictionary<byte, object>
            {
                {(byte) DiscussionParamKey.ShapeOwnerId, ownerId},
                {(byte) DiscussionParamKey.OffsetYKey, Y},
                {(byte) DiscussionParamKey.ChangedTopicId, topicId}
            };
            return res;
        }

        public Dictionary<byte, object> ToDict()
        {
            return Write(ownerId, Y, topicId);
        }
    }
}