using System.Windows;
using AbstractionLayer;

namespace Discussions.pdf_reader
{
    public partial class ReaderWindow : PortableWindow
    {
        public ReaderWindow(string pdfPathName)
        {
            InitializeComponent();

            DataContext = this;

            Width = 0.8*System.Windows.SystemParameters.PrimaryScreenWidth;
            Height = 0.8*System.Windows.SystemParameters.PrimaryScreenHeight;
            this.WindowState = WindowState.Normal;

            pdfViewerUC.PdfPathName = pdfPathName;
        }

        private void btnZoom_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            pdfViewerUC.Dispose();
        }
    }
}