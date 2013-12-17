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
        private MultiClickRecognizer _mediaDoubleClick;

        public LargeImageUC()
        {
            InitializeComponent();

            _mediaDoubleClick = new MultiClickRecognizer(MediaDoubleClick, null);
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
            if (_mediaDoubleClick != null)
                _mediaDoubleClick.Click(sender, e);
        }

        private void Image_TouchDown(object sender, TouchEventArgs e)
        {
            if (_mediaDoubleClick!=null)
                _mediaDoubleClick.Click(sender, e);
        }

        private void MediaDoubleClick(object sender, InputEventArgs e)
        {
            BotLaunch();
        }

        private void LargeImageUC_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _mediaDoubleClick.Dispose();
            _mediaDoubleClick = null;
        }

        public object BotLaunch()
        {
            return AttachmentManager.RunViewer(DataContext as Attachment, true);
        }
    }
}