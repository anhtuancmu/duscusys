using System;
using System.Windows;
using AbstractionLayer;

namespace Discussions.view
{
    public partial class BrowserWindow : PortableWindow
    {
        public BrowserWindow(string Url)
        {
            InitializeComponent();

            browser.Source = new Uri(Url, UriKind.Absolute);

            ResizeMode = ResizeMode.NoResize;
        }

        private void btnZoom_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            browser.Dispose();
        }
    }
}