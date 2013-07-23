using System;
using System.Windows;

namespace Discussions
{
    public partial class BrowserWindow : Window
    {
        public BrowserWindow(string Url)
        {
            InitializeComponent();

            browser.Source = new Uri(Url, UriKind.Absolute);
        }

        private void btnZoom_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SurfaceWindow_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            browser.Dispose();
        }
    }
}