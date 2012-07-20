using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct ClusterBadgeMessage
    {
        public int badgeId;
        public int clusterId;
        public bool playImmidiately;
        public int ownerId;  
        public int topicId;
        public int callToken;

        public static ClusterBadgeMessage Read(Dictionary<byte, object> par)
        {
            var res = new ClusterBadgeMessage();
            res.ownerId = (int)par[(byte)DiscussionParamKey.ShapeOwnerId];
            res.badgeId = (int)par[(byte)DiscussionParamKey.ShapeId];
            res.clusterId = (int)par[(byte)DiscussionParamKey.ClusterId];
            res.playImmidiately = (bool)par[(byte)DiscussionParamKey.BoolParameter1];
            res.topicId = (int)par[(byte)DiscussionParamKey.ChangedTopicId];
            res.callToken = (int)par[(byte)DiscussionParamKey.CallToken];
            return res;
        }

        public static Dictionary<byte, object> Write(int badgeId,
                                                     int ownerId,
                                                     bool playImmidiately, 
                                                     int clusterId,
                                                     int topicId,
                                                     int callToken)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte)DiscussionParamKey.ShapeOwnerId, ownerId);
            res.Add((byte)DiscussionParamKey.ShapeId, badgeId);
            res.Add((byte)DiscussionParamKey.ClusterId, clusterId);
            res.Add((byte)DiscussionParamKey.BoolParameter1, playImmidiately);
            res.Add((byte)DiscussionParamKey.ChangedTopicId, topicId);
            res.Add((byte)DiscussionParamKey.CallToken, callToken);
            return res;      
        }
    }
}
