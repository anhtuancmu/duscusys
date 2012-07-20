using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct ClusterCreateMessage
    {
        public int clusterId;
        public int ownerId;  
        public int topicId;

        public static ClusterCreateMessage Read(Dictionary<byte, object> par)
        {
            var res = new ClusterCreateMessage();
            res.clusterId = (int)par[(byte)DiscussionParamKey.ClusterId];
            res.ownerId = (int)par[(byte)DiscussionParamKey.InitialShapeOwnerId];           
            res.topicId = (int)par[(byte)DiscussionParamKey.ChangedTopicId];
            return res;
        }

        public static Dictionary<byte, object> Write(int clusterId,
                                                     int ownerId,
                                                     int topicId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte)DiscussionParamKey.ClusterId, clusterId);
            res.Add((byte)DiscussionParamKey.InitialShapeOwnerId, ownerId);
            res.Add((byte)DiscussionParamKey.ChangedTopicId, topicId);
            return res;      
        }
    }
}
