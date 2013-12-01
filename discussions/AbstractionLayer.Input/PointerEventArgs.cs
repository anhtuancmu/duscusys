using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace AbstractionLayer.Input
{
    public class PointerEventArgs : EventArgs 
    {
        private MouseEventArgs _mouseEventArgs;
        private TouchEventArgs _touchEventArgs;

        public PointerEventArgs(TouchEventArgs touchEventArgs, MouseEventArgs mouseEventArgs)
        {
            if (touchEventArgs == null && mouseEventArgs==null)
                throw new ArgumentNullException();

            _touchEventArgs = touchEventArgs;
            _mouseEventArgs = mouseEventArgs;
        }

        public Point GetPosition(IInputElement relativeTo)
        {
            if (_touchEventArgs != null)
            {
                Debug.WriteLine("GetPosition.touch");
                TouchPoint touchPoint = _touchEventArgs.GetTouchPoint(relativeTo);
                return touchPoint.Position;
            }

            Debug.WriteLine("GetPosition.mouse");
            return _mouseEventArgs.GetPosition(relativeTo); 
        }

        public TouchDevice TouchDevice
        {
            get
            {
                if (_touchEventArgs != null)
                   return _touchEventArgs.TouchDevice;
                return null;
            }
        }
    }
}
