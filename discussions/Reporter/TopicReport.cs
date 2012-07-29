﻿using System;
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
        public int numSources;
        public int numComments;
        public int cumulativeDuration; //in seconds
        public int[] clusterIds;
        public int[] linkIds;
        public int numPoints;
        public int numPointsWithDescription;
        public int numMediaAttachments;
        public List<int> accumulatedParticipants;//only used for totals/avg etc, more than single topic

        public TopicReport(Topic topic, int numClusters, int numClusteredBadges,
                          int numLinks, int numSources,
                          int numComments, int cumulativeDuration, int[] clusterIds, int[] linkIds,
                          int numPoints, int numPointsWithDescription, int numMediaAttachments)
        {
            this.topic              = topic;
            this.numClusters        = numClusters;
            this.numClusteredBadges = numClusteredBadges;
            this.numLinks           = numLinks;          
            this.numSources  = numSources;
            this.numComments = numComments;
            this.cumulativeDuration = cumulativeDuration;
            this.clusterIds         = clusterIds;
            this.linkIds            = linkIds;
            this.numPoints = numPoints;
            this.numPointsWithDescription = numPointsWithDescription;
            this.numMediaAttachments = numMediaAttachments;
        }

        public TopicReport()
        {
        }

        public int GetNumParticipantsAmong(List<int> amongUserIds)
        {
            var uniqueParticipants = new List<int>();
            foreach (var pers in topic.Person)
                if (amongUserIds.Contains(pers.Id) && !uniqueParticipants.Contains(pers.Id))
                    uniqueParticipants.Add(pers.Id);
            return uniqueParticipants.Count();
        }

        public int GetNumParticipantsAmong2(List<int> amongUserIds) 
        {
            var res = new List<int>();

            foreach (var usr in accumulatedParticipants)
                if (amongUserIds.Contains(usr) && !res.Contains(usr))
                    res.Add(usr);

            return res.Count();
        }
    }
}
