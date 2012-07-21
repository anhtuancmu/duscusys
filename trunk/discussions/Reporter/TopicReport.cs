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
        public List<Person> participants;
        public int numSources;
        public int numComments;
        public int cumulativeDuration; //in seconds
        public int[] clusterIds;
        public int[] linkIds; 

        public TopicReport(Topic topic, int numClusters, int numClusteredBadges,
                          int numLinks, IEnumerable<Person> participants, int numSources,
                          int numComments, int cumulativeDuration, int[] clusterIds, int[] linkIds)
        {
            this.topic              = topic;
            this.numClusters        = numClusters;
            this.numClusteredBadges = numClusteredBadges;
            this.numLinks           = numLinks;
            this.participants = new List<Person>(participants);
            this.numSources  = numSources;
            this.numComments = numComments;
            this.cumulativeDuration = cumulativeDuration;
            this.clusterIds         = clusterIds;
            this.linkIds            = linkIds;
        }

        public int numParticipants
        {
            get
            {
                var uniqueParticipants = new List<int>(); 
                foreach(var pers in participants)
                    if(!uniqueParticipants.Contains(pers.Id))  
                        uniqueParticipants.Add(pers.Id);
                return uniqueParticipants.Count();
            }
        }
    }
}
