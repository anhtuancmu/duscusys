using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    //endpoint for reporting needs only
    public struct LinkReportResponse
    {
        //if endpoint is cluster and it has text caption
        public string ClusterCaption1;

        public int IdOfCluster1;

        //id of argpoint if endpoint is arg.point.
        public int ArgPointId1;

        //true if it's arg.point 
        public bool EndpointArgPoint1;


        //end 2

        public string ClusterCaption2;
        public int IdOfCluster2;
        public int ArgPointId2;
        public bool EndpointArgPoint2;

        public int topicId;

        public int initialOwner;

        // only used on client-side 
        public ArgPoint ArgPoint1;
        public ArgPoint ArgPoint2;
        public Topic Topic;
        public Person initOwner;

        public void Write(Dictionary<byte, object> dto)
        {
            dto.Add((byte)DiscussionParamKey.BoolParameter1, EndpointArgPoint1);
            
            if (EndpointArgPoint1)
                dto.Add((byte)DiscussionParamKey.ArgPointId, ArgPointId1);
            else if(ClusterCaption1!=null && ClusterCaption1!="")
                dto.Add((byte)DiscussionParamKey.ClusterCaption, ClusterCaption1);
            dto.Add((byte)DiscussionParamKey.ClusterId, IdOfCluster1);

            //second end
            dto.Add((byte)DiscussionParamKey.BoolParameter2, EndpointArgPoint2);

            if (EndpointArgPoint2)
                dto.Add((byte)DiscussionParamKey.ArgPointId2, ArgPointId2);
            else if (ClusterCaption2 != null && ClusterCaption2 != "")
                dto.Add((byte)DiscussionParamKey.ClusterCaption2, ClusterCaption2);
            dto.Add((byte)DiscussionParamKey.ClusterId2, IdOfCluster2);

            dto.Add((byte)DiscussionParamKey.ChangedTopicId, topicId);
            dto.Add((byte)DiscussionParamKey.InitialShapeOwnerId, initialOwner);
        }

        public static LinkReportResponse Read(Dictionary<byte, object> dto)
        {
            var res = default(LinkReportResponse);

            res.EndpointArgPoint1 = (bool)dto[(byte)DiscussionParamKey.BoolParameter1];

            if (res.EndpointArgPoint1)
                res.ArgPointId1 = (int)dto[(byte)DiscussionParamKey.ArgPointId];
            else if (dto.ContainsKey((byte)DiscussionParamKey.ClusterCaption))
                res.ClusterCaption1 = (string)dto[(byte)DiscussionParamKey.ClusterCaption];
            res.IdOfCluster1 = (int)dto[(byte)DiscussionParamKey.ClusterId];

            //2nd end

            res.EndpointArgPoint2 = (bool)dto[(byte)DiscussionParamKey.BoolParameter2];

            if (res.EndpointArgPoint2)
                res.ArgPointId2 = (int)dto[(byte)DiscussionParamKey.ArgPointId2];
            else if (dto.ContainsKey((byte)DiscussionParamKey.ClusterCaption2))
                res.ClusterCaption2 = (string)dto[(byte)DiscussionParamKey.ClusterCaption2];
            res.IdOfCluster2 = (int)dto[(byte)DiscussionParamKey.ClusterId2];

            res.topicId = (int)dto[(byte)DiscussionParamKey.ChangedTopicId];
            res.initialOwner = (int)dto[(byte)DiscussionParamKey.InitialShapeOwnerId];

            return res;              
        }

        public Dictionary<byte,object> ToDict()
        {
            var res = new Dictionary<byte, object>();
            Write(res);
            return res;
        }
    }
}
