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
using System.Windows.Shapes;

namespace Discussions
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