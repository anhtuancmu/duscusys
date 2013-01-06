using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct ClusterStatsResponse
    {
        public int clusterId;
        public string clusterTextTitle;
        public int[] points;
        public int topicId;
        public int initialOwnerId;
        public int clusterShId;

        public static ClusterStatsResponse Read(Dictionary<byte, object> par)
        {
            var res = new ClusterStatsResponse();
            res.clusterId = (int) par[(byte) DiscussionParamKey.ClusterId];
            res.clusterTextTitle = (string) par[(byte) DiscussionParamKey.ClusterCaption];
            res.points = (int[]) par[(byte) DiscussionParamKey.ArrayOfInts];
            res.topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId];
            res.initialOwnerId = (int) par[(byte) DiscussionParamKey.InitialShapeOwnerId];
            res.clusterShId = (int) par[(byte) DiscussionParamKey.ShapeId];
            return res;
        }

        public static Dictionary<byte, object> Write(int clusterId, string clusterTextTitle,
                                                     int[] points, int topicId, int initialOwnerId, int shId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.ClusterId, clusterId);
            res.Add((byte) DiscussionParamKey.ClusterCaption, clusterTextTitle);
            res.Add((byte) DiscussionParamKey.ArrayOfInts, points);
            res.Add((byte) DiscussionParamKey.ChangedTopicId, topicId);
            res.Add((byte) DiscussionParamKey.InitialShapeOwnerId, initialOwnerId);
            res.Add((byte) DiscussionParamKey.ShapeId, shId);
            return res;
        }

        public Dictionary<byte, object> ToDict()
        {
            return Write(this.clusterId, this.clusterTextTitle, this.points, this.topicId, this.initialOwnerId,
                         this.clusterShId);
        }
    }
}