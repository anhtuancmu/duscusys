using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;
using DistributedEditor;

namespace Discussions.RTModel.Model
{
    public class ShapeState
    {
        public VdShapeType shapeType;
        public int shapeId;
        public int initialOwner;
        public byte[] bytes;
        public int[] ints;
        public double[] doubles;
        public int topicId;
        public bool doBroadcast;

        public ShapeState()
        {
        }

        public ShapeState(VdShapeType shapeType, int initialOwner,
                          int shapeId,
                          byte[] bytes, int[] ints, double[] doubles,
                          int topicId)
        {
            this.shapeType = shapeType;
            this.initialOwner = initialOwner;
            this.shapeId = shapeId;
            this.bytes = bytes;
            this.ints = ints;
            this.doubles = doubles;
            this.topicId = topicId;
            this.doBroadcast = true;
        }

        public static ShapeState Read(Dictionary<byte, object> par)
        {
            var res = new ShapeState();
            res.shapeType = (VdShapeType) par[(byte) DiscussionParamKey.ShapeType];
            res.initialOwner = (int) par[(byte) DiscussionParamKey.InitialShapeOwnerId];
            res.shapeId = (int) par[(byte) DiscussionParamKey.ShapeId];
            res.bytes = (byte[]) par[(byte) DiscussionParamKey.ArrayOfBytes];
            res.ints = (int[]) par[(byte) DiscussionParamKey.ArrayOfInts];
            res.doubles = (double[]) par[(byte) DiscussionParamKey.ArrayOfDoubles];
            res.topicId = (int) par[(byte) DiscussionParamKey.ChangedTopicId];
            res.doBroadcast = (bool) par[(byte) DiscussionParamKey.DoBroadcast];
            return res;
        }

        public static Dictionary<byte, object> Write(VdShapeType shapeType, int initialOwner,
                                                     int shapeId, byte[] bytes,
                                                     int[] ints, double[] doubles,
                                                     int topicId, bool doBroadcast)
        {
            var res = new Dictionary<byte, object>();
            res.Add((byte) DiscussionParamKey.ShapeType, shapeType);
            res.Add((byte) DiscussionParamKey.InitialShapeOwnerId, initialOwner);
            res.Add((byte) DiscussionParamKey.ShapeId, shapeId);
            res.Add((byte) DiscussionParamKey.ArrayOfBytes, bytes);
            res.Add((byte) DiscussionParamKey.ArrayOfInts, ints);
            res.Add((byte) DiscussionParamKey.ArrayOfDoubles, doubles);
            res.Add((byte) DiscussionParamKey.ChangedTopicId, topicId);
            res.Add((byte) DiscussionParamKey.DoBroadcast, doBroadcast);
            return res;
        }

        public Dictionary<byte, object> ToDict()
        {
            return Write(shapeType, initialOwner, shapeId, bytes, ints, doubles, topicId, doBroadcast);
        }

        public void Write(BinaryWriter annotation)
        {
            if (bytes != null)
            {
                annotation.Write(bytes.Length);
                annotation.Write(bytes);
            }
            else
            {
                annotation.Write(0);
            }

            if (doubles != null)
            {
                annotation.Write(doubles.Length);
                foreach (var d in doubles)
                    annotation.Write(d);
            }
            else
            {
                annotation.Write(0);
            }

            if (ints != null)
            {
                annotation.Write(ints.Length);
                foreach (var i in ints)
                    annotation.Write(i);
            }
            else
            {
                annotation.Write(0);
            }

            annotation.Write((int) shapeType);
            annotation.Write(shapeId);
            annotation.Write(initialOwner);
            annotation.Write(topicId);
            annotation.Write(true); //doBroadcast
        }

        public void Read(BinaryReader annotation)
        {
            var cb = annotation.ReadInt32();
            bytes = cb > 0 ? annotation.ReadBytes(cb) : null;

            var cDoubles = annotation.ReadInt32();
            if (cDoubles == 0)
            {
                doubles = null;
            }
            else
            {
                doubles = new double[cDoubles];
                for (var i = 0; i < cDoubles; i++)
                    doubles[i] = annotation.ReadDouble();
            }

            var cInts = annotation.ReadInt32();
            if (cInts == 0)
            {
                ints = null;
            }
            else
            {
                ints = new int[cInts];
                for (var i = 0; i < cInts; i++)
                    ints[i] = annotation.ReadInt32();
            }

            shapeType = (VdShapeType) annotation.ReadInt32();
            shapeId = annotation.ReadInt32();
            initialOwner = annotation.ReadInt32();
            topicId = annotation.ReadInt32();
            doBroadcast = annotation.ReadBoolean(); //doBroadcast
        }
    }
}