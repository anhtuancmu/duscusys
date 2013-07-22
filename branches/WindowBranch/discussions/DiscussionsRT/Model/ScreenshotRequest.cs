using System.Collections.Generic;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct ScreenshotRequest
    {
        public int topicId;
        public int discussionId;

        public static ScreenshotRequest Read(Dictionary<byte, object> par)
        {
            var res = new ScreenshotRequest();
            res.topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId];
            res.discussionId = (int) par[(byte) DiscussionParamKey.DiscussionId];
            return res;
        }

        public static Dictionary<byte, object> Write(int topicId, int discussionId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.ChangedTopicId, topicId);
            res.Add((byte) DiscussionParamKey.DiscussionId, discussionId);
            return res;
        }
    }
}