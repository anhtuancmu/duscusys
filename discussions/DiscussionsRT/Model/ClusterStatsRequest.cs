using System.Collections.Generic;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct ClusterStatsRequest
    {
        public int clusterId;
        public int topicId;

        public static ClusterStatsRequest Read(Dictionary<byte, object> par)
        {
            var res = new ClusterStatsRequest();
            res.clusterId = (int) par[(byte) DiscussionParamKey.ClusterId];
            res.topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId];
            return res;
        }

        public static Dictionary<byte, object> Write(int clusterId, int topicId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.ClusterId, clusterId);
            res.Add((byte) DiscussionParamKey.ChangedTopicId, topicId);
            return res;
        }
    }
}