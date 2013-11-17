using System.Collections.Generic;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct BrowserScrollPositionRequest
    {       
        public int topicId;

        public static BrowserScrollPosition Read(Dictionary<byte, object> par)
        {
            var res = new BrowserScrollPosition
            {
                topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId]
            };
            return res;
        }

        public static Dictionary<byte, object> Write(int topicId)
        {
            var res = new Dictionary<byte, object>
            {               
                {(byte) DiscussionParamKey.ChangedTopicId, topicId}
            };
            return res;
        }

        public Dictionary<byte, object> ToDict()
        {
            return Write(topicId);
        }
    }
}