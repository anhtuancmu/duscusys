using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AbstractionLayer;
using Discussions.bots;
using Discussions.rt;
using Discussions.RTModel.Model;
using DistributedEditor;

namespace Discussions.view
{
    public partial class ImageWindow : PortableWindow, ICachedWindow
    {
        public const int NO_ATTACHMENT = -1;
        private int _attachId;
        private int _topicId;
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
                    ZoomBySlider(_zoomFactor);
                }
            }
        }

        private void ZoomBySlider(double finalFactor)
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

        private static ImageWindow _inst;
        public static ImageWindow Instance(int attachId, int topicId, bool localRequest)
        {
            if (_inst == null)
                _inst = new ImageWindow();

            _inst.Init(attachId, topicId, localRequest);

            return _inst;
        }

        ImageWindow()
        {
            InitializeComponent();
        }

        void Init(int attachId, int topicId, bool localRequest)
        {
            _attachId = attachId;
            _topicId = topicId;

            ExplanationModeMediator.Inst.OnWndOpened(this, attachId, localRequest);

            //if (ExplanationModeMediator.Inst.ExplanationModeEnabled)
            //{
            //    WindowState = WindowState.Maximized;
            //}
            //else
            //{
            WindowState = WindowState.Normal;
            Width = 1280;
            Height = 768;
            //}

            btnExplanationMode.DataContext = ExplanationModeMediator.Inst;
            btnLaserPointer.DataContext = ExplanationModeMediator.Inst;

            DataContext = this;

            //Width = 0.8 * System.Windows.SystemParameters.PrimaryScreenWidth;
            //Height = 0.8 * System.Windows.SystemParameters.PrimaryScreenHeight;
            //Width = 1024;
            //Height = 768;

            if (ExplanationModeMediator.Inst.ExplanationModeEnabled)
                DiscWindows.Get().HidePublic();

            SetTransform(Matrix.Identity);

            //we cannot use HorizontalAlignment==Center, so center the image via RenderTransform
            var op = Dispatcher.BeginInvoke((Action)(() =>
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

        public void Deinit()
        {
            ExplanationModeMediator.Inst.OnWndClosed(this);

            DiscWindows.Get().ShowPublic();

            SetListeners(false);

            img.Source = null;

            //if (_laserPointerWndCtx != null)
            //{
            //    _laserPointerWndCtx.Dispose();
            //    _laserPointerWndCtx = null;
            //}

            ExplanationModeMediator.Inst.LasersEnabled = false;

            Hide();
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
                if(ExplanationModeMediator.Inst.ExplanationModeEnabled)
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
            Deinit();
            Hide();
        }

        protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Handled = true;

            base.OnManipulationStarting(e);
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs args)
        {
            var element = args.Source as UIElement;
            var xform = element.RenderTransform as MatrixTransform;
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
               Pan(mousePos);
            }
           
            _prevX = mousePos.X;
            _prevY = mousePos.Y;
        }

        void Pan(Point pos)
        {
            var matrix = GetTransform();

            if (_prevX > 0 && _prevY > 0)
            {
                matrix.Translate(pos.X - _prevX, pos.Y - _prevY);
            }

            SetTransform(matrix);

            CheckSendMatrixBackground();
        }

        private void Window_MouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            Point mousePos = e.GetPosition(this);
            double factor = e.Delta > 0 ? 1.1 : 0.9;
            ZoomInOut(mousePos, factor);
        }

        void ZoomInOut(Point center, double factor)
        {
            var matrix = GetTransform();

            var finalFact = matrix.M11 * factor;
            if (finalFact > MAX_ZOOM || finalFact < MIN_ZOOM)
                return;

            matrix.ScaleAt(factor,
                           factor,
                           center.X,
                           center.Y);

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

        private void ImageWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Deinit();
        }

        public async Task BotManipulationsAsync()
        {
            Point wndCenter = this.PointToScreen(new Point(ActualWidth / 2, ActualHeight / 2));
           
            for (int i = 0; i < 30; ++i)
            {
                ZoomInOut(wndCenter, 0.9);
                await Utils.DelayAsync(10);
            }
            
            for (int phi = 0; phi < 360; phi+=5)
            {
                const double r = 200;
                var pos = new Point(
                    wndCenter.X + r * Math.Cos(Math.PI * phi / 180),
                    wndCenter.Y + r * Math.Sin(Math.PI * phi / 180)
                );

                BotPan(pos);

                await Utils.DelayAsync(10);
            }

            ExplanationModeMediator.Inst.LasersEnabled = true;
            await Utils.DelayAsync(50);
            await BotUtils.LaserMovementAsync(_laserPointerWndCtx);

            await Utils.DelayAsync(200);
            for (int i = 0; i < 15; ++i)
            {
                ZoomInOut(wndCenter, 1.1);
                await Utils.DelayAsync(10);
            }
        }

        private void BotPan(Point pos)
        {
             Pan(pos);

            _prevX = pos.X;
            _prevY = pos.Y;
        }
    }
}