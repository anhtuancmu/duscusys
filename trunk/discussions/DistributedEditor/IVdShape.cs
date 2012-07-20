using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Input;
using Discussions.RTModel.Model;

namespace DistributedEditor
{
    public enum PointApplyResult
    {
        None,
        Move,
        Resize,
        MoveResize
    }
    
    public interface IVdShape
    {
        int  Id();        
       
        //focus and cursor
        Cursor GetCursor();  
        void SetCursor(Cursor c);  
        void UnsetCursor();  
        void SetFocus();
        void RemoveFocus();
        bool IsFocused();

        void AttachToCanvas(Canvas c);
        void DetachFromCanvas(Canvas c);
        
        //manipulations
        void StartManip(Point p, object sender);
        PointApplyResult ApplyCurrentPoint(Point p);
        void FinishManip();
        void ScaleInPlace(bool plus);
        void MoveBy(double dx, double dy);
        double distToFigure(Point from);
        void ManipulationStarting(object sender, ManipulationStartingEventArgs e);
        void ManipulationDelta(object sender, ManipulationDeltaEventArgs e);
        void ManipulationCompleted(object sender, ManipulationCompletedEventArgs e);
        bool IsManipulated();

        //d-editor
        int InitialOwner();
        VdShapeType ShapeCode();
        UIElement UnderlyingControl();
        ShapeState GetState(int topicId);
        void ApplyState(ShapeState st);
        ShapeZ ShapeZLevel();
        void SetZIndex(int z);
    }
}
