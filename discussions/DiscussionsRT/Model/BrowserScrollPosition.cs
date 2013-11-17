using System.Collections.Generic;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public class BrowserScrollPosition
    {
        public int ownerId;
        public int X;
        public int Y;        
        public int topicId;

        public static BrowserScrollPosition Read(Dictionary<byte, object> par)
        {
            var res = new BrowserScrollPosition
            {
                ownerId = (int) par[(byte) DiscussionParamKey.ShapeOwnerId],
                X = (int)par[(byte)DiscussionParamKey.OffsetXKey],
                Y = (int)par[(byte)DiscussionParamKey.OffsetYKey],
                topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId]
            };
            return res;
        }

        public static Dictionary<byte, object> Write(int ownerId,
                                                     int X, int Y,                                                     
                                                     int topicId)
        {
            var res = new Dictionary<byte, object>
            {
                {(byte) DiscussionParamKey.ShapeOwnerId, ownerId},
                {(byte) DiscussionParamKey.OffsetXKey, X},
                {(byte) DiscussionParamKey.OffsetYKey, Y},
                {(byte) DiscussionParamKey.ChangedTopicId, topicId}
            };
            return res;
        }

        public Dictionary<byte, object> ToDict()
        {
            return Write(ownerId, X, Y, topicId);
        }
    }
}