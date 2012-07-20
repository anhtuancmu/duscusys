using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;

namespace Reporter
{
    public class TopicReport
    {
        public Topic topic;
        public int numClusters;
        public int numClusteredBadges;
        public int numLinks;
        public int numParticipants;
        public int numSources;
        public int numComments;
        public int cumulativeDuration; //in seconds
        public int[] clusterIds; 

        public TopicReport(Topic topic, int numClusters, int numClusteredBadges,
                          int numLinks, int numParticipants, int numSources, 
                          int numComments, int cumulativeDuration, int[] clusterIds)
        {
            this.topic              = topic;
            this.numClusters        = numClusters;
            this.numClusteredBadges = numClusteredBadges;
            this.numLinks           = numLinks;
            this.numParticipants    = numParticipants;
            this.numSources  = numSources;
            this.numComments = numComments;
            this.cumulativeDuration = cumulativeDuration;
            this.clusterIds         = clusterIds;
        }
    }
}
