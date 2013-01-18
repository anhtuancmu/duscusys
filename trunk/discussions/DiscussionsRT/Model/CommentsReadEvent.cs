using System.Collections.Generic;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct CommentsReadEvent
    {
        public int PersonId;
        public int TopicId;
        public int ArgPointId;

        public static CommentsReadEvent Read(Dictionary<byte, object> par)
        {
            var res = new CommentsReadEvent();
            res.PersonId = (int) par[(byte) DiscussionParamKey.PersonId];
            res.TopicId = (int)par[(byte)DiscussionParamKey.ChangedTopicId];
            res.ArgPointId = (int)par[(byte)DiscussionParamKey.ArgPointId];  
            return res;
        }

        public static Dictionary<byte, object> Write(int PersonId, int TopicId, int ArgPointId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte)DiscussionParamKey.PersonId, PersonId);
            res.Add((byte)DiscussionParamKey.ChangedTopicId, TopicId);
            res.Add((byte)DiscussionParamKey.ArgPointId, ArgPointId);     
            return res;
        }
    }
}
