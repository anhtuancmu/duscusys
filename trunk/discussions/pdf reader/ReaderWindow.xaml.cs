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

namespace Discussions.pdf_reader
{
    public partial class ReaderWindow : SurfaceWindow
    {
        public ReaderWindow(string PdfPathName)
        {
            InitializeComponent();

            DataContext = this;

            Width = 0.8*System.Windows.SystemParameters.PrimaryScreenWidth;
            Height = 0.8*System.Windows.SystemParameters.PrimaryScreenHeight;
            this.WindowState = WindowState.Normal;

            pdfViewerUC.PdfPathName = PdfPathName;
        }

        private void btnZoom_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SurfaceWindow_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            pdfViewerUC.Dispose();
        }
    }
}