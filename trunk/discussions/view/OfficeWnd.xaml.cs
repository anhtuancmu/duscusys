using System;
using System.Windows;

namespace Discussions.view
{
    public partial class OfficeWnd : Window
    {
        public OfficeWnd(string docPathName)
        {
            InitializeComponent();

            browser.Navigate(new Uri(@"file:///C:\projects\TDS\discussions\test files\Test file for WORD.docx"));
        }

        private void Window_Activated(object sender, EventArgs e)
        {
        }

        private bool centered = false;

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!centered)
            {
                centered = true;
                Left = System.Windows.SystemParameters.FullPrimaryScreenWidth*0.5 - ActualWidth*0.5;
                Top = System.Windows.SystemParameters.FullPrimaryScreenHeight*0.5 - ActualHeight*0.5;
            }
        }
    }
}