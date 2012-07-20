using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DistributedEditor
{
    public class VdBaseShape
    {
        // resize context 
        protected Rectangle activeMarker = null;
        protected bool TouchManip = false;

        protected Point StartPoint;
        protected Point CurrentPoint;

        protected int _initialOwner;
        protected int _id = -1;

        protected UserCursor _cursorView;

        protected Cursor _cursor = null;

        protected bool _isFocused = false;

        public virtual void SetCursor(Cursor c)
        {
            _cursor = c;
            _cursorView.DataContext = c;
        }

        public virtual void UnsetCursor()
        {
            _cursor = null;
            _cursorView.DataContext = null;
        }

        public Cursor GetCursor()
        {
            return _cursor;
        }

        public virtual void SetFocus()
        {
            _isFocused = true;
        }

        public virtual void RemoveFocus()
        {
            _isFocused = false;
        }

        public bool IsFocused()
        {
            return _isFocused;
        }

        public int InitialOwner()
        {
            return _initialOwner;
        }
     
        public int Id()
        {
            if (_id == -1)
                throw new KeyNotFoundException("shape id not initialized");
            
            return _id;
        }

        public VdBaseShape(int owner, int shapeId)
        {
            _initialOwner = owner;
            _id = shapeId;
            _cursorView = new UserCursor();    
        }

        public virtual void FinishManip()
        {    
            if(activeMarker!=null)
            {
                activeMarker.ReleaseAllTouchCaptures();
                activeMarker.ReleaseMouseCapture();
                activeMarker = null;
            }

            TouchManip = false;
        }

        public delegate void ShapeChanged(IVdShape sh);
        public ShapeChanged shapeChanged;

        protected bool _manipulationGoing = false;
        public virtual void ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _manipulationGoing = false;
        }

        public virtual bool IsManipulated()
        {
            return _manipulationGoing;
        }

        public virtual void ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            _manipulationGoing = true;
        }
    }
}
