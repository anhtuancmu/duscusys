using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Discussions.webkit_host
{
    public partial class WebKitFrm : Form
    {
        string _url;
        BrowserBar _browserBar;

        public WebKitFrm(string Url)
        {
            InitializeComponent();

            _url = Url;

            _browserBar = new BrowserBar();
            _browserBar.Browser = webKitBrowser1;
            _browserBar.WinForm = this;
            elementHost1.Child = _browserBar;

            Width  = (int)(0.8 * (double)Screen.PrimaryScreen.Bounds.Width);
            Height = (int)(0.8 * (double)Screen.PrimaryScreen.Bounds.Height);
            WindowState = FormWindowState.Normal;
        }

        private void WebKitFrm_Load(object sender, EventArgs e)
        {
            webKitBrowser1.Navigate(_url);
        }

        private void webKitBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            _browserBar.addressBar.Text = webKitBrowser1.Url.ToString();             
        }
    }
}
