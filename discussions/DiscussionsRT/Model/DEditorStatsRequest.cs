using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct DEditorStatsRequest
    {
        public int topicId;

        public static DEditorStatsRequest Read(Dictionary<byte, object> par)
        {
            var res = new DEditorStatsRequest();
            res.topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId];
            return res;
        }

        public static Dictionary<byte, object> Write(int topicId)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.ChangedTopicId, topicId);
            return res;
        }
    }
}