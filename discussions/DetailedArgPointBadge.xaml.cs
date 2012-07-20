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
using Discussions.model;
using Discussions.DbModel;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for DetailedArgPointBadge.xaml
    /// </summary>
    public partial class DetailedArgPointBadge : UserControl
    {
        public DetailedArgPointBadge()
        {
            InitializeComponent();
        }

        public static readonly RoutedEvent CommentsEvent = EventManager.RegisterRoutedEvent(
               "CommentsChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DetailedArgPointBadge));

        // Provide CLR accessors for the event
        public event RoutedEventHandler CommentsChanged
        {
            add { AddHandler(CommentsEvent, value); }
            remove { RemoveHandler(CommentsEvent, value); }
        }

        void RaiseCommentsChanged()
        {
            this.RaiseEvent(new RoutedEventArgs(DetailedArgPointBadge.CommentsEvent, this));
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.NewValue==null)
                return;
            if(!(e.NewValue is ArgPoint))
                return;

            ArgPoint pt = (ArgPoint)e.NewValue;
            lblSide.Content = string.Format("({0})", Ctors.SideCodeToString(pt.SideCode));
        }

        static void AddCommentPlaceholder(ArgPoint pt)
        {
            Comment cmt = new Comment();
            cmt.Text = "";
            cmt.Person = SessionInfo.Get().person;
            pt.Comment.Add(cmt);
        }

        private void btnAddComment_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && (DataContext is ArgPoint))
                AddCommentPlaceholder((ArgPoint)DataContext);
        }

        private void btnRemoveComment_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && (DataContext is ArgPoint) && lstBxComments.SelectedItem != null)
            {
                ((ArgPoint)DataContext).Comment.Remove((Comment)lstBxComments.SelectedItem);
                RaiseCommentsChanged();
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image img = sender as Image;

            if (img == null)
                return;

            ImageWindow wnd = new ImageWindow();
            wnd.img.Source = img.Source;
            wnd.Show();
        }

        private void AllSources_Click(object sender, RoutedEventArgs e)
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            allRefsPopup.SetModel(ap.Description, null);
            allRefsPopup.IsOpen = true;
        }
    }
}
