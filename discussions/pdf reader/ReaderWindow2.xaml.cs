using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AbstractionLayer;
using Discussions.bots;
using Discussions.rt;
using Discussions.RTModel.Model;
using DistributedEditor;
using MoonPdfLib;

namespace Discussions.pdf_reader
{
    public partial class ReaderWindow2 : PortableWindow, ICachedWindow
    {
        private ExplanationModeMediator _mediator;

        private float _recentSyncedZoom;
        private double _recentSyncedScrollX;
        private double _recentSyncedScrollY;
        private DispatcherTimer _viewStateTimer;
        private LaserPointerWndCtx _laserPointerWndCtx;

        private static ReaderWindow2 _inst;
        public static ReaderWindow2 Instance(string pdfPathName, int attachmentId, int? topicId, bool localRequest)
        {
            if (_inst == null)
                _inst = new ReaderWindow2();

            _inst.Init(pdfPathName, attachmentId, topicId, localRequest);

            return _inst;
        }

        ReaderWindow2()
        {
            InitializeComponent();
        }

        void ToggleLocalLaserPointer()
        {
            if (_laserPointerWndCtx == null)
                _laserPointerWndCtx = new LaserPointerWndCtx(laserScene,
                    ExplanationModeMediator.Inst.CurrentTopicId != null ?
                    ExplanationModeMediator.Inst.CurrentTopicId.Value : -1,
                    LaserPointerTargetSurface.PdfReader
                );

            _laserPointerWndCtx.LocalLazerEnabled = ExplanationModeMediator.Inst.LasersEnabled;
        }

        private bool _moonPdfLoaded;
        private void MoonPdfPanel_OnLoaded(object sender, RoutedEventArgs e)
        {
            _moonPdfLoaded = true;          
        }

        void Init(string pdfPathName, int attachmentId, int? topicId, bool localRequest)
        {
            _inst = this;

            DataContext = this;

            Width = 1024;
            Height = 768;

            this.WindowState = WindowState.Normal;

            btnLaserPointer.DataContext = ExplanationModeMediator.Inst;
            btnExplanationMode.DataContext = ExplanationModeMediator.Inst;

            _mediator = ExplanationModeMediator.Inst;
            _mediator.PdfOpen = true;

            ExplanationModeMediator.Inst.OnWndOpened(this, attachmentId, localRequest);

            if (_moonPdfLoaded)
                _inst.moonPdfPanel.OpenFile(pdfPathName);
            else
                Utils.DelayAsync(100).GetAwaiter().OnCompleted(
                    () => _inst.moonPdfPanel.OpenFile(pdfPathName));

            moonPdfPanel.ZoomType = ZoomType.FitToWidth;

            //wait the doc to load before starting sync.
            _viewStateTimer = new DispatcherTimer();
            _viewStateTimer.Interval = TimeSpan.FromMilliseconds(100);
            _viewStateTimer.Tick += _viewStateTimer_Tick;
            if (_mediator.ExplanationModeEnabled)
            {
                Utils.DelayAsync(100).GetAwaiter().OnCompleted(RequestScrollPosition);
                Utils.DelayAsync(150).GetAwaiter().OnCompleted(() => _viewStateTimer.Start());
            }

            if (topicId != null)
                _mediator.CurrentTopicId = topicId;

            SetListeners(true);
        }

        public void Deinit()
        {
            SetListeners(false);
            _mediator.PdfOpen = false;
            _mediator.LasersEnabled = false;

            if (_viewStateTimer != null)
            {
                _viewStateTimer.Stop();
                _viewStateTimer.Tick -= _viewStateTimer_Tick;
                _viewStateTimer = null;
            }

            _recentSyncedScrollX = 0;
            _recentSyncedScrollY = 0;
            _recentSyncedZoom = 0;

            ExplanationModeMediator.Inst.OnWndClosed(this);
            Hide();
        }
        
        public static void EnsureInstanceClosed()
        {
            if (_inst == null)
                return; //already closed 

            try
            {
                _inst.Close();
            }
            catch
            {
            }

            _inst = null;
        }

        void _viewStateTimer_Tick(object sender, EventArgs e)
        {
            if (moonPdfPanel.ScrollViewer.VerticalOffset != _recentSyncedScrollY ||
                moonPdfPanel.ScrollViewer.HorizontalOffset != _recentSyncedScrollX ||
                moonPdfPanel.CurrentZoom != _recentSyncedZoom)
            {
                if (_mediator.CurrentTopicId != null && _mediator.ExplanationModeEnabled)
                {
                    var pers = SessionInfo.Get().person;
                    if (pers != null)
                    {
                        _recentSyncedScrollX = moonPdfPanel.ScrollViewer.HorizontalOffset;
                        _recentSyncedScrollY = moonPdfPanel.ScrollViewer.VerticalOffset;
                        _recentSyncedZoom = moonPdfPanel.CurrentZoom;
                        UISharedRTClient.Instance.clienRt.SendPdfScrolled(
                                            pers.Id,
                                            _recentSyncedScrollX,
                                            _recentSyncedScrollY,
                                            _recentSyncedZoom,
                                            (int)_mediator.CurrentTopicId);
                    }
                }
            }
        }

        void SetListeners(bool doSet)
        {
            if (doSet)
                UISharedRTClient.Instance.clienRt.onPdfScroll += OnPdfScroll;
            else
                UISharedRTClient.Instance.clienRt.onPdfScroll -= OnPdfScroll;

            if (doSet)
                _mediator.PropertyChanged += Inst_PropertyChanged;
            else
                _mediator.PropertyChanged -= Inst_PropertyChanged;
        }

        private void OnPdfScroll(PdfScrollPosition scroll)
        {
            if (_mediator.CurrentTopicId != null &&
                _mediator.CurrentTopicId == scroll.topicId &&
                _mediator.ExplanationModeEnabled)
            {
                _recentSyncedScrollX = scroll.X;
                _recentSyncedScrollY = scroll.Y;
                _recentSyncedZoom = scroll.Zoom;

                SetView(scroll.X, scroll.Y, scroll.Zoom);
            }
        }

        void Inst_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ExplanationModeEnabled")
            {
                if (_mediator.ExplanationModeEnabled)
                    RequestScrollPosition();
            }
            else if (e.PropertyName == "LasersEnabled")
            {
                ToggleLocalLaserPointer();
            }
        }

        void SetView(double x, double y, float zoom)
        {
            moonPdfPanel.Zoom(zoom, false);//resets scroll
            moonPdfPanel.ScrollViewer.ScrollToVerticalOffset(y);
            moonPdfPanel.ScrollViewer.ScrollToHorizontalOffset(x); 
        }

        private void Window_Closing_1(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Deinit();
        }

        private void RequestScrollPosition()
        {
            if (_mediator.CurrentTopicId != 0)
            {
                UISharedRTClient.Instance.clienRt.SendPdfScrollGetPos(_mediator.CurrentTopicId != null ?
                                                                          (int)_mediator.CurrentTopicId :
                                                                          -1);
            }
        }

        private void BtnZoom_OnClick(object sender, RoutedEventArgs e)
        {
            Deinit();
        }

        private void ReaderWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ToggleLocalLaserPointer();
        }

        public async Task BotScrollAsync(Random rnd)
        {
            for (int i = 0; i < 100 + rnd.Next(400); ++i)
            {
                moonPdfPanel.ScrollViewer.ScrollToVerticalOffset(moonPdfPanel.ScrollViewer.VerticalOffset + 10);
                await Utils.DelayAsync(8);
            }
            await Utils.DelayAsync(100);
            for (int i = 0; i < 100+rnd.Next(400); ++i)
            {
                moonPdfPanel.ScrollViewer.ScrollToVerticalOffset(moonPdfPanel.ScrollViewer.VerticalOffset - 10);
                await Utils.DelayAsync(8);
            }
            for (int i = 0; i < 200+rnd.Next(200); ++i)
            {
                moonPdfPanel.ScrollViewer.ScrollToVerticalOffset(moonPdfPanel.ScrollViewer.VerticalOffset + 6);
                await Utils.DelayAsync(8);
            }
        }

        public async Task BotLaserActivityAsync()
        {
            await Utils.DelayAsync(10);
            ExplanationModeMediator.Inst.LasersEnabled = true;
            await Utils.DelayAsync(500);
            await BotUtils.LaserMovementAsync(_laserPointerWndCtx);
            await Utils.DelayAsync(500);
        }

        private void ReaderWindow2_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var ctrlDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (ctrlDown)
                moonPdfPanel.MouseWheel(sender, e);
            else
                moonPdfPanel.ScrollViewer.ScrollToVerticalOffset(moonPdfPanel.ScrollViewer.VerticalOffset - e.Delta);
        }
    }
}