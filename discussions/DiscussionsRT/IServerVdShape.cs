using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Discussions.RTModel.Model;
using DistributedEditor;

namespace Discussions.RTModel
{
    public interface IServerVdShape
    {
        int Id();
        int InitialOwner();
        VdShapeType ShapeCode();

        //focus and cursor
        ServerCursor GetCursor();
        void SetCursor(ServerCursor c);
        void UnsetCursor();

        ShapeState GetState();
        void ApplyState(ShapeState st);

        int Tag();

        bool Write(BinaryWriter annotation);
        void Read(BinaryReader annotation);
    }
}