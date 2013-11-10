using System;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions.view
{
    public partial class BrowserWindow : SurfaceWindow
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