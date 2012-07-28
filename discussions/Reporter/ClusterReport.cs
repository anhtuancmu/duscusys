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
        public Person initialOwner;

        public ClusterReport(Topic topic, int clusterId, string clusterTitle,
                             ArgPoint[] points, Person initialOwner)
        {
            this.topic           = topic;
            this.clusterId       = clusterId;
            this.clusterTitle    = clusterTitle;
            this.points          = points;
            this.initialOwner    = initialOwner; 
        }
    }
}
