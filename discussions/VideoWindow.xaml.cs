using System;
using System.Windows;
using AbstractionLayer;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for VideoWindow.xaml
    /// </summary>
    public partial class VideoWindow : PortableWindow
    {
        public VideoWindow()
        {
            InitializeComponent();
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