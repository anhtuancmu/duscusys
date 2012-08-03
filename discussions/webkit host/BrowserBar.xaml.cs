using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Discussions.webkit_host
{  
    public partial class BrowserBar : System.Windows.Controls.UserControl
    {
        public WebKit.WebKitBrowser Browser{get;set;}
        public Form WinForm { get; set; }


        public BrowserBar()
        {
            InitializeComponent();
        }

        private void btnBack_Click_1(object sender, RoutedEventArgs e)
        {
            if(!Browser.CanGoBack)
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

        private void btnClose_Click_1(object sender, RoutedEventArgs e)
        {
            WinForm.Close();
        }

        private void addressBar_KeyDown_1(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Browser.Navigate(addressBar.Text);
            }
        }
    }
}
