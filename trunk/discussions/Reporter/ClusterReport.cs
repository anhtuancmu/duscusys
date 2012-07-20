using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;

namespace Reporter
{
    public class ClusterReport
    {
        public Topic topic;
        public int clusterId;
        public string clusterTitle;
        public ArgPoint[] points;

        public ClusterReport(Topic topic, int clusterId, string clusterTitle, ArgPoint[] points)
        {
            this.topic        = topic;
            this.clusterId    = clusterId;
            this.clusterTitle = clusterTitle;
            this.points       = points;  
        }
    }
}
