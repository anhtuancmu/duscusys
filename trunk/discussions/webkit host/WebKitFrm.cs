using System;
using System.Drawing;
using System.Windows.Forms;
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
        }

        private void OnBrowserScroll(BrowserScrollPosition scroll)
        {
            if (_mediator.CurrentTopicId != null &&
                _mediator.CurrentTopicId == scroll.topicId &&
                _mediator.ExplanationModeEnabled)
            {
                ScrollBrowserTo(new Point(scroll.X, scroll.Y));
            }
        }

        public static void EnsureInstanceClosed()
        {
            if (_inst == null)
                return; //already closed 

            try
            {
                UISharedRTClient.Instance.clienRt.onBrowserScroll -= _inst.OnBrowserScroll;
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

        private void webKitBrowser1_Scroll(object sender, ScrollEventArgs e)
        {
            if (_mediator.CurrentTopicId != null && _mediator.ExplanationModeEnabled)
            {                
                var pers = SessionInfo.Get().person;
                if(pers != null)
                    UISharedRTClient.Instance.clienRt.SendBrowserScrolled(
                        new BrowserScrollPosition
                        {
                            ownerId = pers.Id,
                            topicId = (int)_mediator.CurrentTopicId,
                            X = webKitBrowser1.ScrollOffset.X,
                            Y = webKitBrowser1.ScrollOffset.Y
                        });
            }
        }
    }
}