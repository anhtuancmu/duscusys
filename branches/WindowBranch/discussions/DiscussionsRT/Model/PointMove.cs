using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct PointMove
    {
        public int ownerId;

        public int shapeId;

        //position is defined in coordinates of sender in request 
        //this is position of anchor point, defined for each shape separately.

        //for cluster, anchor is top left point. 
        //when cluster is moved, only cluster move request is sent. server side updates 
        //positions of badges of cluster, but doesn't broadcast. all clients recompute 
        //positions of cluster badges, based on relative positions of badges inside cluster. 
        public double X;
        public double Y;

        public static PointMove Read(Dictionary<byte, object> par)
        {
            var res = new PointMove();
            res.ownerId = (int) par[(byte) DiscussionParamKey.ShapeOwnerId];
            res.shapeId = (int) par[(byte) DiscussionParamKey.ShapeId];
            res.X = (double) par[(byte) DiscussionParamKey.AnchorX];
            res.Y = (double) par[(byte) DiscussionParamKey.AnchorY];

            return res;
        }

        public static Dictionary<byte, object> Write(int ownerId, int shapeId, double newX, double newY)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.ShapeOwnerId, ownerId);
            res.Add((byte) DiscussionParamKey.ShapeId, shapeId);
            res.Add((byte) DiscussionParamKey.AnchorX, newX);
            res.Add((byte) DiscussionParamKey.AnchorY, newY);
            return res;
        }
    }
}