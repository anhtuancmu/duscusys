using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Threading;
using Discussions.rt;
using Discussions.RTModel.Model;

namespace Discussions.webkit_host
{
    public partial class WebKitFrm : Form
    {
        private static WebKitFrm _inst = null;

        private readonly string _url;
        private readonly BrowserBar _browserBar;

        public delegate void UserRequestedClosing();

        public static UserRequestedClosing userRequestedClosing = null;

        private readonly ExplanationModeMediator _mediator;

        private DispatcherTimer _scrollStateChecker;

        public WebKitFrm(string url, int? topicId)
        {
            InitializeComponent();

            _url = url;

            _browserBar = new BrowserBar {Browser = webKitBrowser1, WinForm = this};
            elementHost1.Child = _browserBar;

            Width = (int) (0.8*Screen.PrimaryScreen.Bounds.Width);
            Height = (int) (0.8*Screen.PrimaryScreen.Bounds.Height);
            WindowState = FormWindowState.Normal;

            if (_inst != null)
                EnsureInstanceClosed(); //close previous instance
            _inst = this;
           
            _mediator = ExplanationModeMediator.Inst;

            if (topicId!=null)
                _mediator.CurrentTopicId = topicId; 
            UISharedRTClient.Instance.clienRt.onBrowserScroll += OnBrowserScroll;

            if (_mediator.ExplanationModeEnabled && 
                _mediator.CurrentTopicId != null && 
                SessionInfo.Get().person != null)
            {
                UISharedRTClient.Instance.clienRt.SendBrowserScrollGetPos(
                    new BrowserScrollPositionRequest { topicId = (int)_mediator.CurrentTopicId }
                );
            }

            _scrollStateChecker = new DispatcherTimer(DispatcherPriority.Background) {Interval = new TimeSpan(200)};
            _scrollStateChecker.Tick += _scrollStateChecker_Tick;
            _scrollStateChecker.Start();
        }

        private void OnBrowserScroll(BrowserScrollPosition scroll)
        {
            if (_mediator.CurrentTopicId != null &&
                _mediator.CurrentTopicId == scroll.topicId &&
                _mediator.ExplanationModeEnabled)
            {
                var newScrollState = new Point(scroll.X, scroll.Y);
                //if (newScrollState != _lastSentScrollState)
                {
                    ScrollBrowserTo(newScrollState);
                    _skipNextScrollPosChange = true;
                }
            }
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

        private void WebKitFrm_Load(object sender, EventArgs e)
        {
            _browserBar.addressBar.Text = _url;
            webKitBrowser1.Navigate(_url);
        }

        private void webKitBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
        }

        private void WebKitFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _scrollStateChecker.Stop();
            _scrollStateChecker.Tick -= _scrollStateChecker_Tick;
            _scrollStateChecker = null;
            UISharedRTClient.Instance.clienRt.onBrowserScroll -= OnBrowserScroll;
            _inst = null;
        }

        private void WebKitFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if this is local initiative, close             
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (userRequestedClosing != null)
                    userRequestedClosing();
            }
        }

        public void ScrollBrowserTo(Point offset)
        {
            webKitBrowser1.ScrollOffset = offset;          
        }

        public Point GetBrowserOffset()
        {
            return webKitBrowser1.ScrollOffset;    
        }

        private Point pt = new Point();
        private void button1_Click(object sender, EventArgs e)
        {
            pt = new Point(pt.X, pt.Y + 1);
            ScrollBrowserTo(pt);
        }


        private Point _prevLocalScrollState;
        //private Point _lastSentScrollState;
        private bool _skipNextScrollPosChange;
        void _scrollStateChecker_Tick(object sender, EventArgs e)
        {
            //randomize _lastSentScrollState in case we don't send anything.
            //Otherwise locations like Point(0,0) are ignored when received
            //_lastSentScrollState = new Point(235,742);

            if (webKitBrowser1.ScrollOffset != _prevLocalScrollState && !_skipNextScrollPosChange)
            {
                if (_mediator.CurrentTopicId != null && _mediator.ExplanationModeEnabled)
                {
                    var pers = SessionInfo.Get().person;
                    if (pers != null)
                    {
                        var _lastSentScrollState = webKitBrowser1.ScrollOffset;
                        UISharedRTClient.Instance.clienRt.SendBrowserScrolled(
                            new BrowserScrollPosition
                            {
                                ownerId = pers.Id,
                                topicId = (int) _mediator.CurrentTopicId,
                                X = _lastSentScrollState.X,
                                Y = _lastSentScrollState.Y
                            });
                    }
                }
            }

            _skipNextScrollPosChange = false;
            _prevLocalScrollState = webKitBrowser1.ScrollOffset;
        }

        private void webKitBrowser1_Scroll(object sender, ScrollEventArgs e)
        {
            //never called 
        }
    }
}