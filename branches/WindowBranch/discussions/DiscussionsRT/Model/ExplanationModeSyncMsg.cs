using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public enum SyncMsgType
    {
        SourceView,
        ImageView,
        YoutubeView
    }

    public struct ExplanationModeSyncMsg
    {
        public SyncMsgType syncMsgType;

        //for source view it's id of source; for attachment it's attachment id
        public int viewObjectId;

        public bool doExpand;

        public static ExplanationModeSyncMsg Read(Dictionary<byte, object> par)
        {
            var res = new ExplanationModeSyncMsg();
            res.syncMsgType = (SyncMsgType) (int) par[(byte) DiscussionParamKey.IntParameter1];
            res.viewObjectId = (int) par[(byte) DiscussionParamKey.IntParameter2];
            res.doExpand = (bool) par[(byte) DiscussionParamKey.BoolParameter1];
            return res;
        }

        public static Dictionary<byte, object> Write(SyncMsgType type, int viewObjectId, bool doExpand)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.IntParameter1, (int) type);
            res.Add((byte) DiscussionParamKey.IntParameter2, viewObjectId);
            res.Add((byte) DiscussionParamKey.BoolParameter1, doExpand);
            return res;
        }
    }
}