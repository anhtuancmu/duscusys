using System;
using System.Windows;

namespace Discussions.pdf_reader
{
    public partial class ReaderOverlayWindow : Window
    {
        public ReaderOverlayWindow()
        {
            InitializeComponent();
        }

        public ReaderWindow Window { get; set;}
  
        private void ReaderOverlayWindow_OnSourceInitialized(object sender, EventArgs e)
        {
            //WinAPI.SetHitTestVisible(this, visible: false);
        }

        private void BtnZoom_OnClick(object sender, RoutedEventArgs e)
        {
            Window.Close();
        }
    }
}
