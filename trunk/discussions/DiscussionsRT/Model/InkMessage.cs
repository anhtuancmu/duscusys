using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct InkMessage
    {
        public int ownerId;
        public int topicId;
        public byte[] inkData;

        public static InkMessage Read(Dictionary<byte, object> par)
        {
            var res = new InkMessage();
            res.ownerId = (int) par[(byte) DiscussionParamKey.ShapeOwnerId];
            res.topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId];
            res.inkData = (byte[]) par[(byte) DiscussionParamKey.InkData];
            return res;
        }

        public static Dictionary<byte, object> Write(int ownerId,
                                                     int topicId,
                                                     byte[] inkData)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.ShapeOwnerId, ownerId);
            res.Add((byte) DiscussionParamKey.ChangedTopicId, topicId);
            res.Add((byte) DiscussionParamKey.InkData, inkData);
            return res;
        }
    }
}