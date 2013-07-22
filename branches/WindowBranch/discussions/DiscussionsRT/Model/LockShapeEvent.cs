using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    //public struct LockShapeEvent
    //{
    //    public int shapeId;
    //    public int owner;    //if -1, shape is free

    //    public static LockShapeEvent Read(Dictionary<byte, object> par)
    //    {
    //        var res = new LockShapeEvent();
    //        res.owner = (int)par[(byte)DiscussionParamKey.ShapeOwnerId];
    //        res.shapeId = (int)par[(byte)DiscussionParamKey.ShapeId];            
    //        return res;
    //    }

    //    public static Dictionary<byte, object> Write(int owner, int shapeId)
    //    {
    //        var res = new Dictionary<byte, object>();
    //        res.Add((byte)DiscussionParamKey.ShapeOwnerId, owner);
    //        res.Add((byte)DiscussionParamKey.ShapeId, shapeId);            
    //        return res;
    //    }        
    //}
}