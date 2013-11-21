using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AbstractionLayer;
using Discussions.rt;
using Discussions.RTModel.Model;
using DistributedEditor;

namespace Discussions.view
{
    public partial class ImageWindow : PortableWindow
    {
        public const int NO_ATTACHMENT = -1;
        private readonly int _attachId;
        private readonly int _topicId;
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
                    _zoomFactor = value;
                    zoomBySlider(_zoomFactor);
                }
            }
        }

        private void zoomBySlider(double finalFactor)
        {
            var matrix = GetTransform();

            var factorStep = finalFactor / matrix.M11;

            var t = img.TransformToVisual(this);
            var center = t.Transform(new Point(0.5 * img.ActualWidth, 0.5 * img.ActualHeight));

            matrix.ScaleAt(factorStep,
                           factorStep,
                           center.X,
                           center.Y);

            SetTransform(matrix);

            CheckSendMatrixBackground();
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

        private LaserPointerWndCtx _laserPointerWndCtx;

        public ImageWindow(int attachId, int topicId)
        {
            _attachId = attachId;
            _topicId = topicId;
            InitializeComponent();

            btnExplanationMode.DataContext = ExplanationModeMediator.Inst;
            btnLaserPointer.DataContext = ExplanationModeMediator.Inst;

            DataContext = this;

            //Width = 0.8 * System.Windows.SystemParameters.PrimaryScreenWidth;
            //Height = 0.8 * System.Windows.SystemParameters.PrimaryScreenHeight;
            Width = 1024;
            Height = 768;

            ExplanationModeMediator.Inst.OnWndOpened(this, attachId);

            //we cannot use HorizontalAlignment==Center, so center the image via RenderTransform
            Dispatcher.BeginInvoke((Action)(() =>
                {
                    var mt = (MatrixTransform)img.RenderTransform;
                    var matrix = mt.Matrix;
                    matrix.Translate(0.5 * (this.ActualWidth - img.ActualWidth), 0);
                    mt.Matrix = matrix;
                }),
                DispatcherPriority.Background);

            SetListeners(true);

            CheckSendImgStateRequest();
        }

        private void ImageWindow_OnClosing(object sender, CancelEventArgs e)
        {
            SetListeners(false);

            if (_laserPointerWndCtx != null)
            {
                _laserPointerWndCtx.Dispose();
                _laserPointerWndCtx = null;                
            }

            ExplanationModeMediator.Inst.LasersEnabled = false;
        }

        void SetListeners(bool doSet)
        {
            if (doSet)
                UISharedRTClient.Instance.clienRt.onImageViewerManipulated += OnImageViewerManipulated;
            else
                UISharedRTClient.Instance.clienRt.onImageViewerManipulated -= OnImageViewerManipulated;

            if (doSet)
                ExplanationModeMediator.Inst.PropertyChanged += Inst_PropertyChanged;
            else
                ExplanationModeMediator.Inst.PropertyChanged -= Inst_PropertyChanged;
        }

        void Inst_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ExplanationModeEnabled")
            {
                CheckSendImgStateRequest();
            }
            else if (e.PropertyName == "LasersEnabled")
            {
                ToggleLaserPointer();
            }
        }

        void ToggleLaserPointer()
        {
            if (_laserPointerWndCtx == null)
            {
                _laserPointerWndCtx = new LaserPointerWndCtx(laserScene,
                    _topicId,
                    LaserPointerTargetSurface.ImageViewer);
            }

            if(_laserPointerWndCtx!=null)
                _laserPointerWndCtx.LocalLazerEnabled = ExplanationModeMediator.Inst.LasersEnabled;
        }

        void CheckSendImgStateRequest()
        {
            if (ExplanationModeMediator.Inst.ExplanationModeEnabled && _attachId != NO_ATTACHMENT)
                UISharedRTClient.Instance.clienRt.SendImageViewerStateRequest(
                    new ImageViewerStateRequest
                    {
                        ImageAttachmentId = _attachId,
                        OwnerId = SessionInfo.Get().person.Id,
                        TopicId = _topicId
                    });
        }

        private void OnImageViewerManipulated(ImageViewerMatrix mat)
        {
            if (!ExplanationModeMediator.Inst.ExplanationModeEnabled)
                return;

            if (_topicId != mat.TopicId || _attachId != mat.ImageAttachmentId)
                return;

            SetTransform(new Matrix(mat.M11, mat.M12, mat.M21, mat.M22, mat.OffsetX, mat.OffsetY));

            updateZoomFactor(mat.M11);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {            
            Close();
        }

        protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Handled = true;

            base.OnManipulationStarting(e);
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs args)
        {
            UIElement element = args.Source as UIElement;
            MatrixTransform xform = element.RenderTransform as MatrixTransform;
            Matrix matrix = xform.Matrix;
            ManipulationDelta delta = args.DeltaManipulation;

            var finalFactor = matrix.M11 * delta.Scale.X;
            if (finalFactor > MIN_ZOOM && finalFactor < MAX_ZOOM)
            {
                Point center = args.ManipulationOrigin;

                matrix.ScaleAt(delta.Scale.X, delta.Scale.Y, center.X, center.Y);
                //matrix.RotateAt(delta.Rotation, center.X, center.Y);
                matrix.Translate(delta.Translation.X, delta.Translation.Y);
                xform.Matrix = matrix;

                updateZoomFactor(finalFactor);

                CheckSendMatrixBackground();
            }

            args.Handled = true;
            base.OnManipulationDelta(args);
        }

        private void updateZoomFactor(double val)
        {
            _zoomFactor = val;
            zoomSlider.Value = val;
        }

        private double _prevX, _prevY;

        private void WindowMouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);

            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) !=
                System.Windows.Forms.Keys.None)
            {
                var matrix = GetTransform();

                if (_prevX > 0 && _prevY > 0)
                {
                    matrix.Translate(mousePos.X - _prevX, mousePos.Y - _prevY);
                }

                SetTransform(matrix);

                CheckSendMatrixBackground();
            }

            _prevX = mousePos.X;
            _prevY = mousePos.Y;
        }

        private void Window_MouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            var matrix = GetTransform();

            Point mousePos = e.GetPosition(this);
            double factor = e.Delta > 0 ? 1.1 : 0.9;

            var finalFact = matrix.M11 * factor;
            if (finalFact > MAX_ZOOM || finalFact < MIN_ZOOM)
                return;

            matrix.ScaleAt(factor,
                           factor,
                           mousePos.X,
                           mousePos.Y);

            updateZoomFactor(matrix.M11);

            SetTransform(matrix);

            CheckSendMatrixBackground();
        }

        void CheckSendMatrixBackground()
        {
            Dispatcher.BeginInvoke((Action)CheckSendMatrix, DispatcherPriority.Normal);
        }

        void CheckSendMatrix()
        {
            if (!ExplanationModeMediator.Inst.ExplanationModeEnabled)
                return;

            if (_attachId == NO_ATTACHMENT)
                return;

            var tr = GetTransform();
            var mat = new ImageViewerMatrix
                {
                    ImageAttachmentId = _attachId,
                    M11 = tr.M11,
                    M12 = tr.M12,
                    M21 = tr.M21,
                    M22 = tr.M22,
                    OffsetX = tr.OffsetX,
                    OffsetY = tr.OffsetY,
                    OwnerId = SessionInfo.Get().person.Id,
                    TopicId = _topicId
                };

            UISharedRTClient.Instance.clienRt.SendManipulateImageViewer(mat);
        }

        private void ImageWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ToggleLaserPointer();
        }

        private void ImageWindow_OnClosed(object sender, EventArgs e)
        {
            ExplanationModeMediator.Inst.OnWndClosed(this);
        }
    }
}