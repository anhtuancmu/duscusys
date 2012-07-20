using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct UnclusterBadgeMessage
    {
        public int clusterId;
        public int badgeId;
        public int topicId;
        public int usrId;
        public bool playImmidiately;
        public int callToken;

        public static UnclusterBadgeMessage Read(Dictionary<byte, object> par)
        {
            var res = new UnclusterBadgeMessage();
            res.badgeId = (int)par[(byte)DiscussionParamKey.ShapeId];
            res.clusterId = (int)par[(byte)DiscussionParamKey.ClusterId];
            res.playImmidiately = (bool)par[(byte)DiscussionParamKey.BoolParameter1];
            res.topicId = (int)par[(byte)DiscussionParamKey.ChangedTopicId];
            res.usrId = (int)par[(byte)DiscussionParamKey.UserId];
            res.callToken = (int)par[(byte)DiscussionParamKey.CallToken];
            return res;
        }

        public static Dictionary<byte, object> Write(int badgeId,
                                                     int clusterId,
                                                     bool playImmidiately,                                                  
                                                     int topicId,
                                                     int usrId,
                                                     int callToken)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte)DiscussionParamKey.ShapeId, badgeId);
            res.Add((byte)DiscussionParamKey.ClusterId, clusterId);
            res.Add((byte)DiscussionParamKey.BoolParameter1, playImmidiately);
            res.Add((byte)DiscussionParamKey.ChangedTopicId, topicId);
            res.Add((byte)DiscussionParamKey.UserId, usrId);
            res.Add((byte)DiscussionParamKey.CallToken, callToken);
            return res;      
        }
    }
}
