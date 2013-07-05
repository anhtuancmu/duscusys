using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discussions.RTModel.Model;
using DistributedEditor;

namespace Discussions.RTModel
{
    public class ServerVdDoc
    {
        //maps shape id to shape
        private readonly Dictionary<int, IServerVdShape> _shapeIdToShape = new Dictionary<int, IServerVdShape>();

        private readonly Dictionary<int, IServerVdShape> _userIdToCursor = new Dictionary<int, IServerVdShape>();

        private readonly List<LaserPointer> _laserPointers = new List<LaserPointer>();
        public List<LaserPointer> LaserPointers
        {
            get { return _laserPointers; }
        }

        public byte[] inkData;

        private BadgeShapeIdGenerator _badgeIdGen;

        public BadgeShapeIdGenerator BadgeIdGen
        {
            get { return _badgeIdGen; }
        }

        public ServerVdDoc(BinaryReader annotation)
        {
            Read(annotation);
        }

        #region persistence

        public void Read(BinaryReader annotation)
        {
            if (annotation != null)
                _badgeIdGen = new BadgeShapeIdGenerator(annotation.ReadInt32());
            else
                _badgeIdGen = new BadgeShapeIdGenerator();

            if (annotation == null)
                return;

            readInk(annotation);

            var numShapes = annotation.ReadInt32();
            for (var i = 0; i < numShapes; i++)
            {
                var sh = new ServerBaseVdShape(annotation);
                _shapeIdToShape.Add(sh.Id(), sh);
            }
        }

        public bool Write(BinaryWriter annotation)
        {
            annotation.Write(_badgeIdGen.CurrentId());

            writeInk(annotation);

            var shapes = GetShapes();
            annotation.Write(shapes.Count());
            foreach (var sh in shapes)
                if (!sh.Write(annotation))
                    return false;

            return true;
        }

        private void readInk(BinaryReader annotation)
        {
            inkData = null;
            if (annotation != null)
            {
                var inkSize = annotation.ReadInt32();
                if (inkSize > 0)
                    inkData = annotation.ReadBytes(inkSize);
            }
        }

        private void writeInk(BinaryWriter annotation)
        {
            if (inkData != null)
            {
                annotation.Write(inkData.Length);
                annotation.Write(inkData);
            }
            else
                annotation.Write(0);
        }

        #endregion persistence

        public IEnumerable<IServerVdShape> GetShapes()
        {
            return _shapeIdToShape.Values;
        }

        //returns previously locked shape, caller should broadcast cursor free event if result != null
        public void LockShape(IServerVdShape sh, int owner)
        {
            if (sh.GetCursor() != null)
                throw new InvalidOperationException("cannot lock locked shape");

            var cursor = new ServerCursor(owner);
            sh.SetCursor(cursor);

            _userIdToCursor.Add(owner, sh);
        }

        public void UnlockShape(IServerVdShape sh, int owner)
        {
            sh.UnsetCursor();
            _userIdToCursor.Remove(owner);
        }

        public bool UserHasCursor(int owner)
        {
            return _userIdToCursor.ContainsKey(owner);
        }

        public IServerVdShape TryGetShape(int shapeId)
        {
            if (!_shapeIdToShape.ContainsKey(shapeId))
                return null; //no such shape

            return _shapeIdToShape[shapeId];
        }

        public IServerVdShape TryGetBadgeShapeByArgPt(int argPointId)
        {
            return
                _shapeIdToShape.Values.FirstOrDefault(sh => sh.ShapeCode() == VdShapeType.Badge && sh.Tag() == argPointId);
        }

        public void AddShape(IServerVdShape sh)
        {
            _shapeIdToShape.Add(sh.Id(), sh);
        }

        public void AddShapeAndLock(IServerVdShape sh)
        {
            _shapeIdToShape.Add(sh.Id(), sh);
            LockShape(sh, sh.InitialOwner());
        }

        public void RemoveShape(IServerVdShape sh)
        {
            if (sh.GetCursor() != null)
                throw new InvalidOperationException("cannot remove shape locked by cursor!");

            _shapeIdToShape.Remove(sh.Id());
        }

        public void UnlockAndRemoveShape(IServerVdShape sh)
        {
            var curs = sh.GetCursor();
            if (curs != null)
                UnlockShape(sh, curs.OwnerId);
            RemoveShape(sh);
        }

        public bool editingPermission(IServerVdShape sh, int owner)
        {
            if (sh.GetCursor() == null)
                return true;
            else if (sh.GetCursor().OwnerId == owner)
                return true;
            else
                return false;
        }

        public bool AttachLaserPointer(LaserPointer ptr)
        {
            if (LaserPointers.FirstOrDefault(l0 => l0.UserId == ptr.UserId && l0.TopicId == ptr.TopicId) != null)
                return false;
            LaserPointers.Add(ptr);
            return true;
        }

        public bool DetachLaserPointer(LaserPointer ptr)
        {
            var p = LaserPointers.FirstOrDefault(l0 => l0.UserId == ptr.UserId && l0.TopicId == ptr.TopicId);
            if (p == null)
                return false;

            return LaserPointers.Remove(p);
        }

        public bool MoveLaserPointer(LaserPointer ptr)
        {
            var p = LaserPointers.FirstOrDefault(l0 => l0.UserId == ptr.UserId && l0.TopicId == ptr.TopicId);            
            if (p != null)
            {
                p.X = ptr.X;
                p.Y = ptr.Y;
                return true;
            }
            return false;
        }
    }
}