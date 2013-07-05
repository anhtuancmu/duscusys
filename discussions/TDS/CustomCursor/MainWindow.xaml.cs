using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Surface.Presentation.Controls;


namespace CustomCursor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SurfaceWindow
    {
        private const int PointerId = 1;
        private readonly LaserCursorManager _cursorManager;

        private Color _current = Colors.Red;

        public MainWindow()
        {
            InitializeComponent();

            _cursorManager = new LaserCursorManager(visCanv);
        }

        private void MainWindow_OnMouseEnter(object sender, MouseEventArgs e)
        {
            _cursorManager.DisableStandardCursor(this); 
           _cursorManager.AttachLaserPointer(PointerId,_current);
        }

        private void MainWindow_OnMouseLeave(object sender, MouseEventArgs e)
        {
            _cursorManager.EnableStandardCursor(this);
            _cursorManager.DetachLaserPointer(PointerId);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = (Button) sender;
            _current = ((SolidColorBrush)btn.Background).Color;

            _cursorManager.SetTouchVisualizationColors(this, _current);
            _cursorManager.SetLaserColor(PointerId, _current);
        }

        private void MainWindow_OnMouseMove(object sender, MouseEventArgs e)
        {
            _cursorManager.UpdatePointerLocation(PointerId, e.GetPosition(visCanv));
        }
    }
}
