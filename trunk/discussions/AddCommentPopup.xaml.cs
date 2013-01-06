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
using System.Windows.Controls.Primitives;

namespace Discussions
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