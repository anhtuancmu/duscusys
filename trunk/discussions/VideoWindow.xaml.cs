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
    /// <summary>
    /// Interaction logic for VideoWindow.xaml
    /// </summary>
    public partial class VideoWindow : Window
    {
        public VideoWindow()
        {
            InitializeComponent();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
           
        }

        bool centered = false;
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!centered)
            {
                centered = true;
                Left = System.Windows.SystemParameters.FullPrimaryScreenWidth * 0.5 - ActualWidth * 0.5;
                Top = System.Windows.SystemParameters.FullPrimaryScreenHeight * 0.5 - ActualHeight * 0.5;
            }
        }
    }
}
