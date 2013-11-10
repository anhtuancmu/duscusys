using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Discussions.DbModel;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for SourceUC.xaml
    /// </summary>
    public partial class LargeImageUC : UserControl
    {
        private MultiClickRecognizer mediaDoubleClick;

        public LargeImageUC()
        {
            InitializeComponent();

            mediaDoubleClick = new MultiClickRecognizer(MediaDoubleClick, null);
        }

        //private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        //{
        //    System.Diagnostics.Process.Start(linkTarget.Text);
        //}

        //private void Hyperlink_TouchDown_1(object sender, TouchEventArgs e)
        //{
        //    System.Diagnostics.Process.Start(linkTarget.Text);
        //}

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mediaDoubleClick.Click(sender, e);
        }

        private void Image_TouchDown(object sender, TouchEventArgs e)
        {
            mediaDoubleClick.Click(sender, e);
        }

        private void MediaDoubleClick(object sender, InputEventArgs e)
        {
            AttachmentManager.RunViewer(((FrameworkElement) sender).DataContext as Attachment);
        }
    }
}