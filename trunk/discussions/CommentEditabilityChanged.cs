using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Discussions.DbModel;

namespace Discussions
{
    public class CommentEditabilityChanged : RoutedEventArgs
    {
        public CommentEditabilityChanged(RoutedEvent re,  bool IsBeingEdited) :
            base(re)
        {
            this.IsBeingEdited = IsBeingEdited;
        }

        public bool IsBeingEdited { get; set; }
    }
}
