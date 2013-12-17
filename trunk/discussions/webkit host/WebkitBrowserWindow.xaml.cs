using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using AbstractionLayer;
using Discussions.rt;
using Discussions.RTModel.Model;
using Discussions.webkit_host;
using Point = System.Drawing.Point;

namespace Discussions.view
{
    public partial class WebkitBrowserWindow : PortableWindow, ICachedWindow
    {
        private string _url;

        private WebKit.WebKitBrowser _webKitBrowser1;

        public delegate void UserRequestedClosing();

        public static UserRequestedClosing userRequestedClosing = null;

        private ExplanationModeMediator _mediator;

        private DispatcherTimer _scrollStateChecker;

        private BrowserOverlayWindow _overlayWnd;

        private static WebkitBrowserWindow _inst;
        public static WebkitBrowserWindow Instance(string url, int? topicId)
        {
            if (_inst == null)
                _inst = new WebkitBrowserWindow();

            _inst.Init(url, topicId);

            return _inst;
        }

        WebkitBrowserWindow()
        {
            InitializeComponent();
        }

        void Init(string url, int? topicId)
        {
            _url = url;

            ExplanationModeMediator.Inst.WebkitOpen = true;

            //if (ExplanationModeMediator.Inst.ExplanationModeEnabled)
            //{
            //    WindowState = WindowState.Maximized;
            //}
            //else
            {
                WindowState = WindowState.Normal;
                Width = 1280;
                Height = 768;
            }

            // 
            // _webKitBrowser1
            // 
            if (_webKitBrowser1 == null)
            {
                _webKitBrowser1 = new WebKit.WebKitBrowser();
                _webKitBrowser1.Anchor =
                    ((System.Windows.Forms.AnchorStyles)
                        ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                           | System.Windows.Forms.AnchorStyles.Left)
                          | System.Windows.Forms.AnchorStyles.Right)));
                _webKitBrowser1.BackColor = System.Drawing.Color.White;
                _webKitBrowser1.Location = new System.Drawing.Point(0, 0);
                _webKitBrowser1.Margin = new System.Windows.Forms.Padding(0);
                _webKitBrowser1.Name = "_webKitBrowser1";
                _webKitBrowser1.TabIndex = 0;
                _webKitBrowser1.Navigated += _webKitBrowser1_Navigated;
                webkitHost.Child = _webKitBrowser1;
                _webKitBrowser1.ResumeLayout();
            }

            browserBar.Browser = _webKitBrowser1;
            browserBar.Window = this;

            browserBar.addressBar.Text = _url;
            _webKitBrowser1.Navigate(_url);
            // _webKitBrowser1.Navigate(@"file:///C:\Users\User\Documents\Visual Studio 2013\Projects\tds3\discussions\bin\x86\Debug\qwe.html");
            //string str = Reencoder.GetUrlContent("http://www.shinmai.co.jp/olympic/jouhou/shochi.htm");
            //var reencoded = Reencoder.ShiftJisToUtf8(str);
            //_webKitBrowser1.Document.CreateTextNode(reencoded);// aTextContent = reencoded;

            if (ExplanationModeMediator.Inst.ExplanationModeEnabled)
                DiscWindows.Get().HidePublic();

            ResizeMode = ResizeMode.NoResize;

            //if (_inst != null)
            //    EnsureInstanceDeinited(); //close previous instance
            _inst = this;

            _mediator = ExplanationModeMediator.Inst;

            if (topicId != null)
                _mediator.CurrentTopicId = topicId;

            if (_mediator.ExplanationModeEnabled)
                RequestScrollPosition();

            if (_scrollStateChecker==null)
            {
                _scrollStateChecker = new DispatcherTimer(DispatcherPriority.Background)
                {
                    Interval = new TimeSpan(200)
                };
            }
            _scrollStateChecker.Tick += _scrollStateChecker_Tick;
            _scrollStateChecker.Start();

            if (_overlayWnd==null)
                _overlayWnd = new BrowserOverlayWindow { Window = this };
            _overlayWnd.Show();

            SetListeners(true);
        }

        void _webKitBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            _webKitBrowser1.Visible = true;
        }

        public void Deinit()
        {
            //if this is local initiative, close             
            if (userRequestedClosing != null)
                userRequestedClosing();

            _scrollStateChecker.Stop();
            _scrollStateChecker.Tick -= _scrollStateChecker_Tick;

            //browserBar.Browser = null;
            //browserBar.Window = null;

            //_webKitBrowser1.Dispose();
            //_webKitBrowser1 = null;
            _webKitBrowser1.Visible = false;

            DiscWindows.Get().ShowPublic();

            SetListeners(false);

            ExplanationModeMediator.Inst.LasersEnabled = false;

            ExplanationModeMediator.Inst.WebkitOpen = false;

            if (_overlayWnd != null)
            {
                _overlayWnd.Hide();
            }

            Hide();
        }


        public void SetListeners(bool doSet)
        {
            if (doSet)
            {
                UISharedRTClient.Instance.clienRt.onBrowserScroll += OnBrowserScroll;
                ExplanationModeMediator.Inst.PropertyChanged += Inst_PropertyChanged;
            }
            else
            {
                UISharedRTClient.Instance.clienRt.onBrowserScroll -= OnBrowserScroll;
                ExplanationModeMediator.Inst.PropertyChanged -= Inst_PropertyChanged;
            }
        }

        void Inst_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LasersEnabled")
            {
                ToggleLaserPointer();
            }
            else if (e.PropertyName == "ExplanationModeEnabled")
            {
                if (_mediator.ExplanationModeEnabled)
                    RequestScrollPosition();
            }
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
        public static void EnsureInstanceDeinited()
        {
            if (_inst == null)
                return; //already closed 

            try
            {
                _inst.Deinit();
            }
            catch
            {
            }

            _inst = null;
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
        private bool _skipNextScrollPosChange;
        void _scrollStateChecker_Tick(object sender, EventArgs e)
        {
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
            ToggleLaserPointer();
            AlignLaserWindow();
        }

        public void RequestScrollPosition()
        {
            if (_mediator.CurrentTopicId != 0)
            {
                UISharedRTClient.Instance.clienRt.SendBrowserScrollGetPos(
                    new BrowserScrollPositionRequest {topicId = (int)_mediator.CurrentTopicId}
                    );
            }
        }

        void ToggleLaserPointer()
        {
            _overlayWnd.ToggleLocalLaserPointer();      
        }

        private void WebkitBrowserWindow_OnLocationChanged(object sender, EventArgs e)
        {
            if (_overlayWnd == null)
                return;

            AlignLaserWindow();
        }

        void AlignLaserWindow()
        {
            try
            {
                System.Windows.Point topLeft = webkitHost.PointToScreen(new System.Windows.Point(0, 0));
                System.Windows.Point bottomRight = webkitHost.PointToScreen(
                    new System.Windows.Point(webkitHost.ActualWidth, webkitHost.ActualHeight));

                _overlayWnd.Width = bottomRight.X - topLeft.X;
                _overlayWnd.Height = bottomRight.Y - topLeft.Y;
                _overlayWnd.Top = topLeft.Y;
                _overlayWnd.Left = topLeft.X;
            }
            catch
            {
                //can throw presentation source not attached during loading 
            }
        }

        public void ScrollBrowser(int delta)
        {
            Point newOffset = _webKitBrowser1.ScrollOffset;
            newOffset.Offset(0, -delta);
            ScrollBrowserTo(newOffset);      
        }

        private void WebkitBrowserWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _webKitBrowser1.MaximumSize = new System.Drawing.Size((int)e.NewSize.Width-15, (int)e.NewSize.Height);
        }

        public async Task BotScrollRandomAsync(Random rnd)
        {
            for (int i = 0; i < 100 + rnd.Next(400); ++i)
            {
                _webKitBrowser1.ScrollOffset = new Point(0, _webKitBrowser1.ScrollOffset.Y + 7);
                await Utils.DelayAsync(5);
            }
            await Utils.DelayAsync(100);
            for (int i = 0; i < 100+rnd.Next(400); ++i)
            {
                _webKitBrowser1.ScrollOffset = new Point(0, _webKitBrowser1.ScrollOffset.Y - 7);
                await Utils.DelayAsync(5);
            }
            for (int i = 0; i < 200+rnd.Next(200); ++i)
            {
                _webKitBrowser1.ScrollOffset = new Point(0, _webKitBrowser1.ScrollOffset.Y + 4);
                await Utils.DelayAsync(5);
            }
        }

        public async Task BotLaserActivityAsync()
        {
            await _overlayWnd.BotLaserActivityAsync();
        }

        private void WebkitBrowserWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Deinit();
        }
    }
}