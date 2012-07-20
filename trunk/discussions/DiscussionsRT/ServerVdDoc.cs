using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DistributedEditor;

namespace Discussions.RTModel
{
    public class ServerVdDoc
    {
        //maps shape id to shape
        Dictionary<int, IServerVdShape> shapeIdToShape = new Dictionary<int, IServerVdShape>();

        Dictionary<int, IServerVdShape> userIdToCursor = new Dictionary<int, IServerVdShape>();

        public byte[] inkData = null;

        BadgeShapeIdGenerator _badgeIdGen;
        public BadgeShapeIdGenerator BadgeIdGen
        {
            get
            {
                return _badgeIdGen;
            }
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
                shapeIdToShape.Add(sh.Id(),sh);
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

        void readInk(BinaryReader annotation)
        {
            inkData = null; 
            if(annotation!=null)
            {
                var inkSize = annotation.ReadInt32();
                if (inkSize>0)
                    inkData = annotation.ReadBytes(inkSize); 
            } 
        }
        void writeInk(BinaryWriter annotation)
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
            return shapeIdToShape.Values; 
        }

        //returns previously locked shape, caller should broadcast cursor free event if result != null
        public void LockShape(IServerVdShape sh, int owner)
        {
            if (sh.GetCursor() != null)
                throw new InvalidOperationException("cannot lock locked shape");

            var cursor = new ServerCursor(owner);
            sh.SetCursor(cursor);

            userIdToCursor.Add(owner, sh);           
        }

        public void UnlockShape(IServerVdShape sh, int owner)
        {
            sh.UnsetCursor();
            userIdToCursor.Remove(owner);
        }

        public bool UserHasCursor(int owner)
        {
            return userIdToCursor.ContainsKey(owner);
        }

        public IServerVdShape TryGetShape(int shapeId)
        {
            if (!shapeIdToShape.ContainsKey(shapeId))
                return null; //no such shape

            return shapeIdToShape[shapeId];
        }

        public IServerVdShape TryGetBadgeShapeByArgPt(int argPointId)
        {
            return shapeIdToShape.Values.FirstOrDefault(sh => sh.ShapeCode() == VdShapeType.Badge && sh.Tag() == argPointId);
        }

        public void AddShape(IServerVdShape sh)
        {
            shapeIdToShape.Add(sh.Id(), sh);
        }

        public void AddShapeAndLock(IServerVdShape sh)
        {
            shapeIdToShape.Add(sh.Id(), sh);
            LockShape(sh, sh.InitialOwner());
        }

        public void RemoveShape(IServerVdShape sh)
        {
            if (sh.GetCursor() != null)
                throw new InvalidOperationException("cannot remove shape locked by cursor!");

            shapeIdToShape.Remove(sh.Id());
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
    }
}
