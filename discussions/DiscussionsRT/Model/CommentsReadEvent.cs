using System.Collections.Generic;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct CommentsReadEvent
    {
        public int PersonId;
        public int TopicId;
        public int ArgPointId;
        public int CommentId;

        public static CommentsReadEvent Read(Dictionary<byte, object> par)
        {
            var res = new CommentsReadEvent();
            res.PersonId = (int) par[(byte) DiscussionParamKey.PersonId];
            res.TopicId = (int)par[(byte)DiscussionParamKey.ChangedTopicId];
            res.ArgPointId = (int)par[(byte)DiscussionParamKey.ArgPointId];
            res.CommentId = (int)par[(byte)DiscussionParamKey.CommentId];  
            return res;
        }

        public static Dictionary<byte, object> Write(int PersonId, int TopicId, int ArgPointId, int CommentId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte)DiscussionParamKey.PersonId, PersonId);
            res.Add((byte)DiscussionParamKey.ChangedTopicId, TopicId);
            res.Add((byte)DiscussionParamKey.ArgPointId, ArgPointId);
            res.Add((byte)DiscussionParamKey.CommentId, CommentId);     
            return res;
        }
    }
}
