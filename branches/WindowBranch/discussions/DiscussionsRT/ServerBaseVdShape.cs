using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DistributedEditor;

namespace Discussions.RTModel
{
    public class ServerBaseVdShape : IServerVdShape
    {
        //immutable things
        private int _id;
        private int _initOwner;
        private VdShapeType _shapeType;
        private int _tag;

        private ServerCursor _cursor;
        private Model.ShapeState _state;

        public ServerBaseVdShape(BinaryReader annotation)
        {
            Read(annotation);
        }

        public ServerBaseVdShape(int id, int initOwner, VdShapeType shapeType, int tag)
        {
            _id = id;
            _initOwner = initOwner;
            _shapeType = shapeType;
            _tag = tag;
        }

        public int Id()
        {
            return _id;
        }

        public int Tag()
        {
            return _tag;
        }

        public int InitialOwner()
        {
            return _initOwner;
        }

        public VdShapeType ShapeCode()
        {
            return _shapeType;
        }

        public ServerCursor GetCursor()
        {
            return _cursor;
        }

        public void SetCursor(ServerCursor c)
        {
            _cursor = c;
        }

        public void UnsetCursor()
        {
            _cursor = null;
        }

        public Model.ShapeState GetState()
        {
            return _state;
        }

        public void ApplyState(Model.ShapeState st)
        {
            switch (ShapeCode())
            {
                case VdShapeType.FreeForm:
                    CopyState(st, true);
                    break;
                case VdShapeType.Text:
                    CopyState(st, true);
                    break;
                default:
                    CopyState(st, false);
                    break;
            }
        }

        private void CopyState(Model.ShapeState st, bool ignoreNullBytes)
        {
            if (_state == null)
                _state = new Model.ShapeState();

            if (ignoreNullBytes)
            {
                if (st.bytes != null)
                    _state.bytes = st.bytes;
            }
            else
            {
                _state.bytes = st.bytes;
            }

            _state.doBroadcast = st.doBroadcast;
            _state.doubles = st.doubles;
            _state.initialOwner = st.initialOwner;
            _state.ints = st.ints;
            _state.shapeId = st.shapeId;
            _state.shapeType = st.shapeType;
            _state.topicId = st.topicId;
        }

        public bool Write(BinaryWriter annotation)
        {
            annotation.Write(_id);
            annotation.Write(_initOwner);
            annotation.Write((int) _shapeType);
            annotation.Write(_tag);

            //for all shapes state should be present. if it is not, 
            //we are in transient state when some shape has been created but not state-updated. 
            //we fail its write and thus write of whole document
            if (_state == null)
                return false;

            _state.Write(annotation);
            return true;
        }

        public void Read(BinaryReader annotation)
        {
            _id = annotation.ReadInt32();
            _initOwner = annotation.ReadInt32();
            _shapeType = (VdShapeType) annotation.ReadInt32();
            _tag = annotation.ReadInt32();

            _state = new Model.ShapeState();
            _state.Read(annotation);
        }
    }
}