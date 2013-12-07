using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using AbstractionLayer;
using Discussions.rt;
using Discussions.RTModel.Model;

namespace Discussions.pdf_reader
{
    public partial class ReaderWindow : PortableWindow
    {
        private readonly ReaderOverlayWindow _overlayWnd;

        private readonly ExplanationModeMediator _mediator;

        private bool _skipNextScrollPosChange;

        private static ReaderWindow _inst;

        public ReaderWindow(string pdfPathName, int attachmentId, int? topicId)
        {
            InitializeComponent();

            _inst = this;

            DataContext = this;

            //Width  = 0.8  * SystemParameters.PrimaryScreenWidth;
            //Height = 0.8 * SystemParameters.PrimaryScreenHeight;
            Width = 1024;
            Height = 768;

            this.WindowState = WindowState.Normal;

            btnLaserPointer.DataContext = ExplanationModeMediator.Inst;
            btnExplanationMode.DataContext = ExplanationModeMediator.Inst;

            pdfViewerUC.PdfPathName = pdfPathName;

            _mediator = ExplanationModeMediator.Inst;
            _mediator.PdfOpen = true;

            ExplanationModeMediator.Inst.OnWndOpened(this, attachmentId);

            if (topicId != null)
                _mediator.CurrentTopicId = topicId;
          
            if (_mediator.ExplanationModeEnabled)
                RequestScrollPosition();

            SetListeners(true);

            _overlayWnd = new ReaderOverlayWindow {Window = this};
            _overlayWnd.Show();
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
                ScrollTo(scroll.Y);
                _skipNextScrollPosChange = true;
            }
        }

        void Inst_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ExplanationModeEnabled")
            {
                if(_mediator.ExplanationModeEnabled)
                    RequestScrollPosition();
            }
            else if (e.PropertyName == "LasersEnabled")
            {
                ToggleLaserPointer();
            }
        }

        public void ScrollBy(int delta)
        {
            ScrollTo(pdfViewerUC.VerticalScroll.Value + delta);            
        }

        public void ScrollTo(int offset)
        {
           // pdfViewerUC.VerticalScroll.Value = offset;
        }

        void ToggleLaserPointer()
        {
            _overlayWnd.ToggleLocalLaserPointer();
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _inst = null;
            SetListeners(false);
            _mediator.PdfOpen = false;
            _mediator.LasersEnabled = false;
            pdfViewerUC.Dispose();
            _overlayWnd.Close();
        }

        private void ReaderWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlignLaserWindow();
        }

        private void ReaderWindow_OnLocationChanged(object sender, EventArgs e)
        {
            AlignLaserWindow();
        }

        void AlignLaserWindow()
        {
            try
            {
                Point topLeft = this.PointToScreen(new Point(0, 0));
                Point bottomRight = this.PointToScreen(new Point(this.Width, this.Height));

                _overlayWnd.Top  = topLeft.Y;
                _overlayWnd.Width = this.Width - 15;
                _overlayWnd.Height = this.Height - 30;
                _overlayWnd.Left = bottomRight.X - this.Width;
            }
            catch
            {
                //can throw presentation source not attached during loading 
            }
        }

        private void RequestScrollPosition()
        {
            if (_mediator.CurrentTopicId != 0)
            {
                UISharedRTClient.Instance.clienRt.SendPdfScrollGetPos(_mediator.CurrentTopicId!=null ? 
                                                                          (int)_mediator.CurrentTopicId :
                                                                          -1);
            }
        }

        private void PdfViewerUC_OnScroll(object sender, ScrollEventArgs e)
        {
            if (!_skipNextScrollPosChange)
            {
                if (_mediator.CurrentTopicId != null && _mediator.ExplanationModeEnabled)
                {
                    var pers = SessionInfo.Get().person;
                    if (pers != null)
                    {
                        var lastSentScrollState = e.NewValue;
                        UISharedRTClient.Instance.clienRt.SendPdfScrolled(
                            pers.Id, 
                            lastSentScrollState, 
                            (int)_mediator.CurrentTopicId);
                    }
                }
            }

            _skipNextScrollPosChange = false;
        }

        private void ReaderWindow_OnClosed(object sender, EventArgs e)
        {
            ExplanationModeMediator.Inst.OnWndClosed(this);
        }

        private void BtnZoom_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ReaderWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ToggleLaserPointer();
            AlignLaserWindow();
        }
    }
}