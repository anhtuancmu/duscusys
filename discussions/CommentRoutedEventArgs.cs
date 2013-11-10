using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Discussions.DbModel;
using Discussions.view;

namespace Discussions
{
    public class CommentRoutedEventArgs : RoutedEventArgs
    {
        public CommentRoutedEventArgs(RoutedEvent routedEvent, Comment c, CommentUC control, bool requiresDataRecontext)
            :
                base(routedEvent)
        {
            Comment = c;
            CommentControl = control;
            this.RequiresDataRecontext = requiresDataRecontext;
        }

        public Comment Comment { get; set; }

        public CommentUC CommentControl { get; set; }

        public bool RequiresDataRecontext { get; set; }
    }
}