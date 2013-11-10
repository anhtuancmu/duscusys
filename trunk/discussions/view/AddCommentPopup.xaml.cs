using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for AddCommentPopup.xaml
    /// </summary>
    public partial class AddCommentPopup : Popup
    {
        public delegate void CommentRequest(bool add);

        public CommentRequest addCommentRequest;

        public AddCommentPopup()
        {
            InitializeComponent();
        }

        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            IsOpen = false;
        }

        private void Popup_LostFocus(object sender, RoutedEventArgs e)
        {
            IsOpen = false;
        }

        private void btnAddComment_Click(object sender, RoutedEventArgs e)
        {
            if (addCommentRequest != null)
                addCommentRequest(true);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (addCommentRequest != null)
                addCommentRequest(false);
        }
    }
}