using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

namespace Discussions
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