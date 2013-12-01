using System;
using System.Windows;
using Microsoft.Surface.Presentation.Input;

namespace AbstractionLayer.Input
{
    public class InputProcessor : IDisposable
    {
        private readonly Window _window;
        private readonly UIElement _inputElement;

        public event EventHandler<PointerEventArgs> PointerDown;
        public event EventHandler<PointerEventArgs> PointerMove;
        public event EventHandler<PointerEventArgs> PointerUp;

        private DateTime _recentTouchMove;

        public InputProcessor(Window window, UIElement inputElement)
        {
            _window = window;
            _inputElement = inputElement;

            window.EnableSurfaceInput();

            inputElement.MouseDown += root_MouseDown;
            inputElement.MouseUp   += root_MouseUp;
            inputElement.MouseMove += root_MouseMove;

            inputElement.TouchDown += root_TouchDown;
            inputElement.TouchMove += root_TouchMove;
            inputElement.TouchUp   += root_TouchUp;
        }
        
        public void Dispose()
        {
            _inputElement.MouseDown -= root_MouseDown;
            _inputElement.MouseUp   -= root_MouseUp;
            _inputElement.MouseMove -= root_MouseMove;

            _inputElement.TouchDown -= root_TouchDown;
            _inputElement.TouchMove -= root_TouchMove;
            _inputElement.TouchUp   -= root_TouchUp;
        }

        void RaisePointerDown(object sender, PointerEventArgs e)
        {
            if (PointerDown != null)
                PointerDown(sender, e);
        }

        void RaisePointerMove(object sender, PointerEventArgs e)
        {
            if (PointerMove != null)
                PointerMove(sender, e);
        }

        void RaisePointerUp(object sender, PointerEventArgs e)
        {
            if (PointerUp != null)
                PointerUp(sender, e);
        }

        void root_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            //prevent proparagation to the mouse events
            e.Handled = true;

            _inputElement.ReleaseTouchCapture(e.TouchDevice);

            RaisePointerUp(sender, new PointerEventArgs(e, null));
        }

        void root_TouchMove(object sender, System.Windows.Input.TouchEventArgs e)
        {
            //prevent proparagation to the mouse events
            e.Handled = true;

            RaisePointerMove(sender, new PointerEventArgs(e, null));

            _recentTouchMove = DateTime.Now;
        }

        void root_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            //prevent proparagation to the mouse events
            e.Handled = true;

            e.TouchDevice.Capture(_inputElement);

            RaisePointerDown(sender, new PointerEventArgs(e, null));
        }

        void root_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DateTime.Now.Subtract(_recentTouchMove).TotalMilliseconds < 10)
                return;

            e.Handled = true;

            RaisePointerMove(sender, new PointerEventArgs(null, e));
        }

        void root_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RaisePointerUp(sender, new PointerEventArgs(null, e));
        }

        void root_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RaisePointerDown(sender, new PointerEventArgs(null, e));
        }
    }
}