using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct SourceViewMessage
    {
        public string url; 
        public bool doExpand;
        
        public static SourceViewMessage Read(Dictionary<byte, object> par)
        {
            var res = new SourceViewMessage();
            res.url        = (string)par[(byte)DiscussionParamKey.StringKey];
            res.doExpand   = (bool)par[(byte)DiscussionParamKey.BoolParameter1];
            return res;
        }

        public static Dictionary<byte, object> Write(string url, bool doExpand)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte)DiscussionParamKey.StringKey, url);
            res.Add((byte)DiscussionParamKey.BoolParameter1, doExpand); 
            return res;      
        }
    }
}
