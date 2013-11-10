using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Discussions.DbModel;
using Discussions.view;

namespace Discussions
{
    public class EditBadgeEventArgs : RoutedEventArgs
    {
        public EditBadgeEventArgs(RoutedEvent routedEvent, ArgPoint ap, EditableBadge control) :
            base(routedEvent)
        {
            ArgPt = ap;
            BadgeControl = control;
        }

        public ArgPoint ArgPt { get; set; }

        public EditableBadge BadgeControl { get; set; }
    }
}