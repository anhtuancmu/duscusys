using System.Windows;
using System.Windows.Controls;
using Discussions.DbModel;

namespace Discussions.view
{
    public partial class ReadOnlyCommentUC : UserControl
    {
        public ReadOnlyCommentUC()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged_1(object sender, DependencyPropertyChangedEventArgs e)
        {
            var c = DataContext as Comment;
            if (c == null)
                return;

            if (c.Text == DaoUtils.NEW_COMMENT)
                this.Visibility = Visibility.Hidden;
        }
    }
}