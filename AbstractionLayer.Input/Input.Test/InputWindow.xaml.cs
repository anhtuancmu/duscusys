using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace AbstractionLayer.Input.Input.Test
{
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        private int _i;

        private InputProcessor _proc;

        public InputWindow()
        {
            InitializeComponent();

            _proc = new InputProcessor(this, scene);
            _proc.PointerDown += ProcPointerDown;
            _proc.PointerMove += ProcPointerMove;
            _proc.PointerUp += ProcPointerUp;
        }
        void ProcPointerUp(object sender, PointerEventArgs e)
        {
            Debug.WriteLine("ProcPointerUp" + _i++);
        }

        void ProcPointerMove(object sender, PointerEventArgs pointerEventArgs)
        {
           // Debug.WriteLine("ProcPointerMove" + _i++);

            Debug.WriteLine("ProcPointerMove " + pointerEventArgs.GetPosition(this));
        }

        void ProcPointerDown(object sender, PointerEventArgs e)
        {
            Debug.WriteLine("ProcPointerDown" + _i++);
        }

        private void OnTouchDown(object sender, TouchEventArgs e)
        {
           // Debug.WriteLine("OnTouchDown" + _i++);
        }

        private void OnTouchUp(object sender, TouchEventArgs e)
        {
            //Debug.WriteLine("OnTouchUp" + _i++);
        }

        private void OnTouchMove(object sender, TouchEventArgs e)
        {
            //Debug.WriteLine("OnTouchMove" + _i++);
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("OnMouseDown" + _i++);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("OnMouseUp" + _i++);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine("OnMouseMove" + _i++);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ButtonBase_OnClick");
        }
    }
}
