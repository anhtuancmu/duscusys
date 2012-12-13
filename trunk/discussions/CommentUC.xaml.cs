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

namespace Discussions
{
    /// <summary>
    /// Interaction logic for Comment.xaml
    /// </summary>
    public partial class CommentUC : UserControl
    {
        public static readonly RoutedEvent CommentDeleteEvent = EventManager.RegisterRoutedEvent(
           "CommentDelete", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CommentUC));
        
        public static readonly RoutedEvent CommentEditEvent = EventManager.RegisterRoutedEvent(
            "CommentEdit", RoutingStrategy.Bubble, typeof(CommentEventHandler), typeof(CommentUC));

        public static readonly RoutedEvent CommentEndEvent = EventManager.RegisterRoutedEvent(
            "CommentEnd", RoutingStrategy.Bubble, typeof(CommentEventHandler), typeof(CommentUC));

        public static readonly RoutedEvent CommentEditingEvent = EventManager.RegisterRoutedEvent(
          "CommentEditingChanged", RoutingStrategy.Bubble, typeof(CommentEditingEventHandler), typeof(CommentUC));

        public event CommentEditingEventHandler CommentEditingChanged
        {
            add { AddHandler(CommentEditingEvent, value); }
            remove { RemoveHandler(CommentEditingEvent, value); }
        }

        public event CommentEventHandler CommentDelete
        {
            add { AddHandler(CommentDeleteEvent, value); }
            remove { RemoveHandler(CommentDeleteEvent, value); }
        }

        public event CommentEventHandler CommentEdit
        {
            add { AddHandler(CommentEditEvent, value); }
            remove { RemoveHandler(CommentEditEvent, value); }
        }

        public event CommentEventHandler CommentEnd
        {
            add { AddHandler(CommentEndEvent, value); }
            remove { RemoveHandler(CommentEndEvent, value); }
        }

        public delegate void CommentEventHandler(object sender,  CommentRoutedEventArgs e);

        public delegate void CommentEditingEventHandler(object sender, CommentEditabilityChanged e);

        public static readonly DependencyProperty PermitsEditProperty =
             DependencyProperty.Register("PermitsEdit", typeof(bool),
             typeof(CommentUC), new FrameworkPropertyMetadata(true, OnPermitsEditChanged));
        
        public bool PermitsEdit
        {
            get { return (bool)GetValue(PermitsEditProperty); }
            set { SetValue(PermitsEditProperty, value); }
        }

        private static void OnPermitsEditChanged(DependencyObject source, 
        DependencyPropertyChangedEventArgs e)
        {
            CommentUC control = source as CommentUC;
            bool permits = (bool)e.NewValue;    
            if (!permits)
                control.btnRemoveComment.Visibility = Visibility.Hidden;
        }

        public CommentUC()
        {
            InitializeComponent();
        }

        public double MaxCommentWidth
        {
            set
            {
                txtBxText.MaxWidth = value;
                lblText.MaxWidth   = value;
            }
        }

        private void txtBxText_LostFocus(object sender, RoutedEventArgs e)
        {
            checkReadonly(DataContext as Comment);

            var c = DataContext as Comment;
            bool requiresOwnerInjection;
            if (c != null && c.Text != DaoUtils.NEW_COMMENT && string.IsNullOrWhiteSpace(c.Text))
                requiresOwnerInjection = true;
            else
                requiresOwnerInjection = false;

            checkRemovability(DataContext as Comment);
            this.RaiseEvent(new CommentRoutedEventArgs(CommentEditEvent, c, this, requiresOwnerInjection));
            this.RaiseEvent(new CommentRoutedEventArgs(CommentEndEvent, DataContext as Comment, this, requiresOwnerInjection));

            this.RaiseEvent(new CommentEditabilityChanged(CommentEditingEvent, false));            
        }

        void checkReadonly(Comment c)
        {
            if (c == null)
                return;

            if (c.Text != DaoUtils.NEW_COMMENT && c.Text.Length > 0)
            {
                txtBxText.Visibility = Visibility.Collapsed;
                lblText.Visibility   = Visibility.Visible;
            }
        }

        private void txtBxText_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            CheckRaiseCommentEditabilityEvent();
        }

        void CheckRaiseCommentEditabilityEvent()
        {
            var isEdited = txtBxText.Text != DaoUtils.NEW_COMMENT;
            this.RaiseEvent(new CommentEditabilityChanged(CommentEditingEvent, isEdited));
        }

        private void UserControl_DataContextChanged_1(object sender, DependencyPropertyChangedEventArgs e)
        {
            var c = DataContext as Comment;
            checkReadonly(c);
            checkRemovability(c);
        }

        void checkRemovability(Comment c)
        {
            if (c == null)
                return;

            if (c.Person == null || SessionInfo.Get().person.Id != c.Person.Id)
                btnRemoveComment.Visibility = Visibility.Hidden;
            else
                btnRemoveComment.Visibility = Visibility.Visible;
        }

        private void btnRemoveComment_Click(object sender, RoutedEventArgs e)
        {
            var c = DataContext as Comment;
            if (c == null)
                return;

            this.RaiseEvent(new RoutedEventArgs(CommentDeleteEvent));
            var ap = c.ArgPoint;
            c.ArgPoint = null;
            c.Person = null;
            this.RaiseEvent(new CommentEditabilityChanged(CommentEditingEvent,false));   

            try
            {
                Ctx2.Get().DeleteObject(c);
            }
            catch(Exception)
            {
                //doesn't exist in content 
            }

            DaoUtils.EnsureCommentPlaceholderExists(ap, null);
        }

        private void txtBxText_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && txtBxText.Text != DaoUtils.NEW_COMMENT && !string.IsNullOrWhiteSpace(txtBxText.Text))
            {
                checkReadonly(DataContext as Comment);
                checkRemovability(DataContext as Comment);
                this.RaiseEvent(new CommentRoutedEventArgs(CommentEndEvent, DataContext as Comment, this, true));               
            }
        }
    }
}
