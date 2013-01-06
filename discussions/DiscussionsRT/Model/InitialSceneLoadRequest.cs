using System.Collections.Generic;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct InitialSceneLoadRequest
    {
        public int topicId;

        public static InitialSceneLoadRequest Read(Dictionary<byte, object> par)
        {
            var res = new InitialSceneLoadRequest();
            res.topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId];
            return res;
        }

        public static Dictionary<byte, object> Write(int topicId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.ChangedTopicId, topicId);
            return res;
        }
    }
}