using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct LinkReportRequest  
    {
        public int LinkShapeId;
        public int TopicId;

        public static LinkReportRequest Read(Dictionary<byte, object> par)
        {
            var res = new LinkReportRequest();
            res.LinkShapeId = (int)par[(byte)DiscussionParamKey.ShapeId];
            res.TopicId     = (int)par[(byte)DiscussionParamKey.ChangedTopicId];    

            return res;
        }

        public static Dictionary<byte, object> Write(int linkShapeId, int topicId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte)DiscussionParamKey.ShapeId, linkShapeId);
            res.Add((byte)DiscussionParamKey.ChangedTopicId, topicId);  
            return res;      
        }

        public Dictionary<byte, object> ToDict()
        {
            return Write(LinkShapeId, TopicId);            
        }
    }
}
