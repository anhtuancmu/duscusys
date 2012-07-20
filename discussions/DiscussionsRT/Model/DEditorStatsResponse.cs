using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct DEditorStatsResponse
    {
        public int TopicId;
        public int NumClusters;
        public int NumClusteredBadges;
        public int NumLinks;
        public int[] ListOfClusterIds;

        public static DEditorStatsResponse Read(Dictionary<byte, object> par)
        {
            var res = new DEditorStatsResponse();
            res.TopicId     = (int)par[(byte)DiscussionParamKey.ChangedTopicId];
            res.NumClusters = (int)par[(byte)DiscussionParamKey.NumClusters];
            res.NumClusteredBadges = (int)par[(byte)DiscussionParamKey.NumClusteredBadges];
            res.NumLinks    = (int)par[(byte)DiscussionParamKey.NumLinks];
            res.ListOfClusterIds = (int[])par[(byte)DiscussionParamKey.ArrayOfInts];  
            return res;
        }

        public static Dictionary<byte, object> Write(int TopicId, int NumClusters, int NumClusteredBadges, int NumLinks, int[] ListOfClusterIds)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte)DiscussionParamKey.ChangedTopicId, TopicId);
            res.Add((byte)DiscussionParamKey.NumClusters, NumClusters);
            res.Add((byte)DiscussionParamKey.NumClusteredBadges, NumClusteredBadges);
            res.Add((byte)DiscussionParamKey.NumLinks, NumLinks);
            res.Add((byte)DiscussionParamKey.ArrayOfInts, ListOfClusterIds);
            return res;      
        }

        public Dictionary<byte, object> ToDict()
        {
            return Write(TopicId, NumClusters, NumClusteredBadges, NumLinks, ListOfClusterIds);            
        }
    }
}
