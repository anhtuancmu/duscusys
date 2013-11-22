using System;
using System.Windows;
using AbstractionLayer;

namespace Discussions.pdf_reader
{
    public partial class ReaderWindow : PortableWindow
    {
        private readonly ReaderOverlayWindow _overlayWnd;

        public ReaderWindow(string pdfPathName)
        {
            InitializeComponent();

            DataContext = this;

            Width  = 0.8  * SystemParameters.PrimaryScreenWidth;
            Height = 0.8 * SystemParameters.PrimaryScreenHeight;
            this.WindowState = WindowState.Normal;

            pdfViewerUC.PdfPathName = pdfPathName;

            _overlayWnd = new ReaderOverlayWindow {Window = this};
            _overlayWnd.Show();
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            pdfViewerUC.Dispose();
            _overlayWnd.Close();
        }

        private void ReaderWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlignLaserWindow();
        }

        private void ReaderWindow_OnLocationChanged(object sender, EventArgs e)
        {
            AlignLaserWindow();
        }

        void AlignLaserWindow()
        {
            try
            {
                Point topLeft = this.PointToScreen(new Point(0, 0));
                Point bottomRight = this.PointToScreen(new Point(this.ActualWidth, this.ActualHeight));

                _overlayWnd.Top  = topLeft.Y;
                _overlayWnd.Left = bottomRight.X - _overlayWnd.ActualWidth - 30;
            }
            catch
            {
                //can throw presentation source not attached during loading 
            }
        }
    }
}