using System;
using System.Windows;
using System.Windows.Input;
using Discussions.view;

namespace Discussions.webkit_host
{
    public partial class BrowserBar : System.Windows.Controls.UserControl
    {
        public WebKit.WebKitBrowser Browser { get; set; }
        public WebkitBrowserWindow Window { get; set; }


        public BrowserBar()
        {
            InitializeComponent();

            btnExplanationMode.DataContext = ExplanationModeMediator.Inst;
            btnLaserPointer.DataContext = ExplanationModeMediator.Inst;
        }

        private void btnBack_Click_1(object sender, RoutedEventArgs e)
        {
            if (!Browser.CanGoBack)
            {
                Console.Beep();
                return;
            }
            Browser.GoBack();
        }

        private void btnNext_Click_1(object sender, RoutedEventArgs e)
        {
            if (!Browser.CanGoForward)
            {
                Console.Beep();
                return;
            }
            Browser.GoForward();
        }

        private void btnRefresh_Click_1(object sender, RoutedEventArgs e)
        {
            Browser.Refresh();
        }

        private void addressBar_KeyDown_1(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Browser.Navigate(addressBar.Text);
            }
        }

        private void BtnExplanationMode_OnClick(object sender, RoutedEventArgs e)
        {
            Window.RequestScrollPosition();
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }
    }
}