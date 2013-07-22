using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Discussions.rt;

namespace Discussions.webkit_host
{
    public partial class WebKitFrm : Form
    {
        private static WebKitFrm inst = null;

        private string _url;
        private BrowserBar _browserBar;

        public delegate void UserRequestedClosing();

        public static UserRequestedClosing userRequestedClosing = null;

        public WebKitFrm(string Url)
        {
            InitializeComponent();

            _url = Url;

            _browserBar = new BrowserBar();
            _browserBar.Browser = webKitBrowser1;
            _browserBar.WinForm = this;
            elementHost1.Child = _browserBar;

            Width = (int) (0.8*(double) Screen.PrimaryScreen.Bounds.Width);
            Height = (int) (0.8*(double) Screen.PrimaryScreen.Bounds.Height);
            WindowState = FormWindowState.Normal;

            if (inst != null)
                EnsureInstanceClosed(); //close previous instance
            inst = this;
        }

        public static void EnsureInstanceClosed()
        {
            if (inst == null)
                return; //already closed 

            try
            {
                inst.Close();
            }
            catch (Exception)
            {
            }

            inst = null;
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
            inst = null;
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
    }
}