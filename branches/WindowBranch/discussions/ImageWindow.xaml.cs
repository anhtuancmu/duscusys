using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Discussions
{
    public partial class ImageWindow : Window
    {
        private const double MIN_ZOOM = 0.5;
        private const double MAX_ZOOM = 5;

        private double _zoomFactor = 1.0;

        public double ZoomFactor
        {
            get { return _zoomFactor; }
            set
            {
                if (value != _zoomFactor)
                {
                    var zoomIn = value > _zoomFactor;
                    var zoomOut = value < _zoomFactor;

                    _zoomFactor = value;

                    zoomBySlider(_zoomFactor, zoomIn, zoomOut);
                }
            }
        }

        private void zoomBySlider(double finalFactor, bool zoomIn, bool zoomOut)
        {
            var matrix = GetTransform();

            var stepFactor = finalFactor/matrix.M11;
            matrix.ScaleAt(stepFactor,
                           stepFactor,
                           0.5*this.ActualWidth,
                           0.5*this.ActualHeight);

            SetTransform(matrix);
        }

        private Matrix GetTransform()
        {
            var transformation = img.RenderTransform as MatrixTransform;
            var matrix = transformation == null
                             ? Matrix.Identity
                             : transformation.Matrix;
            return matrix;
        }

        private void SetTransform(Matrix m)
        {
            var mt = new MatrixTransform(m);
            img.RenderTransform = mt;
        }

        public ImageWindow(int attachId)
        {
            InitializeComponent();

            DataContext = this;

            Width = 0.8*System.Windows.SystemParameters.PrimaryScreenWidth;
            Height = 0.8*System.Windows.SystemParameters.PrimaryScreenHeight;
            this.WindowState = WindowState.Normal;

            ExplanationModeMediator.Inst.OnWndOpened(this, attachId);
        }

        private void btnZoom_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void img_ManipulationStarting_1(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Handled = true;
        }

        private void img_ManipulationDelta_1(object sender, ManipulationDeltaEventArgs e)
        {
            var matrix = GetTransform();

            ///var xScale = (e.DeltaManipulation.Scale.X + 2) / 3;
            var xScale = e.DeltaManipulation.Scale.X;

            var finalFact = matrix.M11*xScale;
            if (finalFact > MAX_ZOOM || finalFact < MIN_ZOOM)
                return;

            matrix.ScaleAt(xScale,
                           xScale,
                           e.ManipulationOrigin.X,
                           e.ManipulationOrigin.Y);

            //matrix.RotateAt(e.DeltaManipulation.Rotation,
            //                e.ManipulationOrigin.X,
            //                e.ManipulationOrigin.Y);

            matrix.Translate(e.DeltaManipulation.Translation.X,
                             e.DeltaManipulation.Translation.Y);

            updateZoomFactor(matrix.M11);

            SetTransform(matrix);
        }

        private void updateZoomFactor(double val)
        {
            _zoomFactor = val;
            zoomSlider.Value = val;
        }

        private double PrevX, PrevY;

        private void SurfaceWindow_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);

            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) !=
                System.Windows.Forms.Keys.None)
            {
                var matrix = GetTransform();

                if (PrevX > 0 && PrevY > 0)
                {
                    matrix.Translate(mousePos.X - PrevX, mousePos.Y - PrevY);
                }

                SetTransform(matrix);
            }

            PrevX = mousePos.X;
            PrevY = mousePos.Y;
        }

        private void SurfaceWindow_MouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            var matrix = GetTransform();

            Point mousePos = e.GetPosition(this);
            double factor = e.Delta > 0 ? 1.1 : 0.9;

            var finalFact = matrix.M11*factor;
            if (finalFact > MAX_ZOOM || finalFact < MIN_ZOOM)
                return;

            matrix.ScaleAt(factor,
                           factor,
                           mousePos.X,
                           mousePos.Y);


            SetTransform(matrix);

            updateZoomFactor(matrix.M11);
        }

        private void SurfaceWindow_Loaded_1(object sender, RoutedEventArgs e)
        {
        }

        private void ImageWindow_Closed_1(object sender, EventArgs e)
        {
            ExplanationModeMediator.Inst.OnWndClosed(this);
        }
    }
}