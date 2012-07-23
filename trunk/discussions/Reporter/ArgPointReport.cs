using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;

namespace Reporter
{
    public class ArgPointReport
    {
        public int numPoints;
        public int numPointsWithDescriptions;               
        public int numMediaAttachments;
        public int numSources;
        public int numComments;
        public Person user;
        public Topic topic;

        public ArgPointReport()
        {
        }

        public ArgPointReport(int numPoints, int numPointsWithDescriptions,
                              int numMediaAttachments, int numSources, int numComments,
                              Person user, Topic topic)
        {
            this.numPoints = numPoints;
            this.numPointsWithDescriptions = numPointsWithDescriptions;
            this.numMediaAttachments = numMediaAttachments;
            this.numSources = numSources;
            this.numComments = numComments;
            this.user = user;
            this.topic = topic;
        }
    }
}
