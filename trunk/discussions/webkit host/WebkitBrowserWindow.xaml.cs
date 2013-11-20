using System;
using System.Windows;
using System.Windows.Threading;
using AbstractionLayer;
using Discussions.rt;
using Discussions.RTModel.Model;
using Discussions.webkit_host;
using Point = System.Drawing.Point;

namespace Discussions.view
{
    public partial class WebkitBrowserWindow : PortableWindow
    {
        private static WebkitBrowserWindow _inst;

        private readonly string _url;

        private readonly WebKit.WebKitBrowser _webKitBrowser1;

        public delegate void UserRequestedClosing();

        public static UserRequestedClosing userRequestedClosing = null;

        private readonly ExplanationModeMediator _mediator;

        private DispatcherTimer _scrollStateChecker;

        public WebkitBrowserWindow(string url, int? topicId)
        {
            InitializeComponent();

            _url = url;

            // 
            // _webKitBrowser1
            // 
            _webKitBrowser1 = new WebKit.WebKitBrowser();
            _webKitBrowser1.SuspendLayout();
            _webKitBrowser1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                              | System.Windows.Forms.AnchorStyles.Left)
                                                                              | System.Windows.Forms.AnchorStyles.Right)));
            _webKitBrowser1.BackColor = System.Drawing.Color.White;
            _webKitBrowser1.Location = new System.Drawing.Point(0, 0);
            _webKitBrowser1.Margin = new System.Windows.Forms.Padding(0);
            _webKitBrowser1.Name = "_webKitBrowser1";
            _webKitBrowser1.TabIndex = 0;
            _webKitBrowser1.MaximumSize = new System.Drawing.Size(1016-6, 630-6);
            webkitHost.Child = _webKitBrowser1;
            _webKitBrowser1.ResumeLayout();

            browserBar.Browser = _webKitBrowser1;
            browserBar.Window = this;

            browserBar.addressBar.Text = _url;
            _webKitBrowser1.Navigate(_url);

            ResizeMode = ResizeMode.NoResize;

            if (_inst != null)
                EnsureInstanceClosed(); //close previous instance
            _inst = this;

            _mediator = ExplanationModeMediator.Inst;

            if (topicId != null)
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

            _scrollStateChecker = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = new TimeSpan(200)
            };
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

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if this is local initiative, close             
            if (userRequestedClosing != null)
                userRequestedClosing();
        }

        public void ScrollBrowserTo(Point offset)
        {
            _webKitBrowser1.ScrollOffset = offset;
        }

        public Point GetBrowserOffset()
        {
            return _webKitBrowser1.ScrollOffset;
        }

        private Point _prevLocalScrollState;
        //private Point _lastSentScrollState;
        private bool _skipNextScrollPosChange;
        void _scrollStateChecker_Tick(object sender, EventArgs e)
        {
            //randomize _lastSentScrollState in case we don't send anything.
            //Otherwise locations like Point(0,0) are ignored when received
            //_lastSentScrollState = new Point(235,742);

            if (_webKitBrowser1.ScrollOffset != _prevLocalScrollState && !_skipNextScrollPosChange)
            {
                if (_mediator.CurrentTopicId != null && _mediator.ExplanationModeEnabled)
                {
                    var pers = SessionInfo.Get().person;
                    if (pers != null)
                    {
                        var lastSentScrollState = _webKitBrowser1.ScrollOffset;
                        UISharedRTClient.Instance.clienRt.SendBrowserScrolled(
                            new BrowserScrollPosition
                            {
                                ownerId = pers.Id,
                                topicId = (int)_mediator.CurrentTopicId,
                                X = lastSentScrollState.X,
                                Y = lastSentScrollState.Y
                            });
                    }
                }
            }

            _skipNextScrollPosChange = false;
            _prevLocalScrollState = _webKitBrowser1.ScrollOffset;
        }

        private void WebkitBrowserWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
        }
    }
}