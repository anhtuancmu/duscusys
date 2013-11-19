using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Surface.Presentation.Controls;

namespace AbstractionLayer.Input.Input.Test
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputSurfaceWindow : SurfaceWindow
    {
        private int _i;

        private InputProcessor _proc;

        public InputSurfaceWindow()
        {
            InitializeComponent();

            _proc = new InputProcessor(this, this);
            _proc.PointerDown += ProcPointerDown;
            _proc.PointerMove += ProcPointerMove;
            _proc.PointerUp += ProcPointerUp;
        }

        void ProcPointerUp(object sender, PointerEventArgs e)
        {
            Debug.WriteLine("ProcPointerUp");
        }

        void ProcPointerMove(object sender, PointerEventArgs pointerEventArgs)
        {
           // Debug.WriteLine("ProcPointerMove");
        }

        void ProcPointerDown(object sender, PointerEventArgs e)
        {
            Debug.WriteLine("ProcPointerDown");
        }

        private void InputSurfaceWindow_OnTouchDown(object sender, TouchEventArgs e)
        {
           // Debug.WriteLine("OnTouchDown" + _i++);
        }

        private void InputSurfaceWindow_OnTouchUp(object sender, TouchEventArgs e)
        {
          //  Debug.WriteLine("OnTouchUp" + _i++); 
        }

        private void InputSurfaceWindow_OnTouchMove(object sender, TouchEventArgs e)
        {
          //  Debug.WriteLine("OnTouchMove" + _i++); 
        }

        private void InputSurfaceWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
           // Debug.WriteLine("OnMouseDown" + _i++); 
        }

        private void InputSurfaceWindow_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
           // Debug.WriteLine("OnMouseUp" + _i++); 
        }

        private void InputSurfaceWindow_OnMouseMove(object sender, MouseEventArgs e)
        {
           // Debug.WriteLine("OnMouseMove" + _i++);
        }
    }
}
