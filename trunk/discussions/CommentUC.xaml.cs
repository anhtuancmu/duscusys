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
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for Comment.xaml
    /// </summary>
    public partial class CommentUC : UserControl
    {
        /***************************************************************************/

        public delegate void CommentEditingEventHandler(bool editing);

        public event CommentEditingEventHandler CommentEditLockChanged;

        private void RaiseCommentEditLockChanged(bool editing)
        {
            if (CommentEditLockChanged != null)
                CommentEditLockChanged(editing);
        }

        /***************************************************************************/

        /// <summary>
        /// CommentUC notifies items container about possiblity to focus new placeholder
        /// </summary>
        /// <param name="comment"></param>
        public delegate void PlaceholderFocusEventHandler(Comment comment);

        public event PlaceholderFocusEventHandler placeholderFocus;

        private void RaisePlaceholderFocus(Comment comment)
        {
            if (placeholderFocus != null)
                placeholderFocus(comment);
        }

        /***************************************************************************/

        public delegate void PossibilityToCloseBadgeHandler();

        public event PossibilityToCloseBadgeHandler possibilityToClose;

        private void RaisePossibilityToClose()
        {
            if (possibilityToClose != null)
                possibilityToClose();
        }

        /***************************************************************************/

        public static readonly DependencyProperty PermitsEditProperty =
            DependencyProperty.Register("PermitsEdit", typeof (bool),
                                        typeof (CommentUC), new FrameworkPropertyMetadata(true, OnPermitsEditChanged));

        public bool PermitsEdit
        {
            get { return (bool) GetValue(PermitsEditProperty); }
            set { SetValue(PermitsEditProperty, value); }
        }

        private static void OnPermitsEditChanged(DependencyObject source,
                                                 DependencyPropertyChangedEventArgs e)
        {
            CommentUC control = source as CommentUC;
            bool permits = (bool) e.NewValue;
            if (!permits)
                control.btnRemoveComment.Visibility = Visibility.Hidden;
        }

        /***************************************************************************/

        public CommentUC()
        {
            InitializeComponent();
        }

        public double MaxCommentWidth
        {
            set
            {
                txtBxText.MaxWidth = value;
                lblText.MaxWidth = value;
            }
        }

        private void txtBxText_LostFocus(object sender, RoutedEventArgs e)
        {
            RequestFinishEditing();

            RaiseCommentEditLockChanged(false);
        }

        #region visual state checks

        private void checkReadonly(Comment c)
        {
            if (c == null)
                return;

            var commentFilled = c.Text != DaoUtils.NEW_COMMENT && !string.IsNullOrWhiteSpace(c.Text);
            if (commentFilled)
            {
                txtBxText.Visibility = Visibility.Collapsed;
                lblText.Visibility = Visibility.Visible;
            }
        }

        private void checkRemovability(Comment c)
        {
            if (c == null)
                return;

            btnRemoveComment.Visibility = Visibility.Hidden;

            if (c.Person == null)
            {
                if (c.Text != DaoUtils.NEW_COMMENT)
                    btnRemoveComment.Visibility = Visibility.Visible;
            }
            else if (SessionInfo.Get().person.Id == c.Person.Id)
            {
                btnRemoveComment.Visibility = Visibility.Visible;
            }
        }

        #endregion

        private void txtBxText_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            CheckRaiseCommentEditLockEvent();
        }

        private void CheckRaiseCommentEditLockEvent()
        {
            var isEdited = txtBxText.Text != DaoUtils.NEW_COMMENT;
            RaiseCommentEditLockChanged(isEdited);
        }

        private void UserControl_DataContextChanged_1(object sender, DependencyPropertyChangedEventArgs e)
        {
            var c = DataContext as Comment;
            checkReadonly(c);
            checkRemovability(c);
        }

        private void btnRemoveComment_Click(object sender, RoutedEventArgs e)
        {
            var c = DataContext as Comment;
            if (c == null)
                return;

            var ap = c.ArgPoint;
            c.ArgPoint = null;
            c.Person = null;
            RaiseCommentEditLockChanged(false);

            try
            {
                Ctx2.Get().DeleteObject(c);
            }
            catch (Exception)
            {
                //doesn't exist in content 
            }

            DaoUtils.EnsureCommentPlaceholderExists(ap);

            ap.ChangesPending = true;
            UISharedRTClient.Instance.clienRt.SendStatsEvent(
                StEvent.CommentRemoved,
                ap.Person.Id,
                ap.Topic.Discussion.Id,
                ap.Topic.Id,
                DeviceType.Wpf);
        }

        private void RequestFinishEditing()
        {
            checkRemovability(DataContext as Comment);
            checkReadonly(DataContext as Comment);
            if (HandleCommentCommit())
                Recontext();
        }

        private void txtBxText_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                RequestFinishEditing();
        }

        private void Recontext()
        {
            var c = DataContext as Comment;
            if (c == null)
                return;
            DataContext = null;
            DataContext = c;
        }

        public bool HandleCommentCommit()
        {
            var c = DataContext as Comment;
            if (c == null)
                return false;

            var ap = c.ArgPoint;
            if (ap == null)
                return false;

            if (c.Text == DaoUtils.NEW_COMMENT || string.IsNullOrWhiteSpace(c.Text))
                return false;

            RaisePossibilityToClose();

            ap.ChangesPending = true;

            //inject author
            var commentAuthor = SessionInfo.Get().getPerson(ap);
            var res = DaoUtils.InjectAuthorOfComment(c, commentAuthor);

            //ensure placeholder           
            var placeholder = DaoUtils.EnsureCommentPlaceholderExists(ap);
            if (placeholder != null)
                RaisePlaceholderFocus(placeholder);

            UISharedRTClient.Instance.clienRt.SendStatsEvent(
                StEvent.CommentAdded,
                SessionInfo.Get().person.Id,
                ap.Topic.Discussion.Id,
                ap.Topic.Id,
                DeviceType.Wpf);

            return res;
        }
    }
}