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