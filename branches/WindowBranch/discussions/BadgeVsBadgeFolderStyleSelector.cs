using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;
using Discussions.DbModel;

namespace Discussions
{
    public class BadgeVsBadgeFolderStyleSelector : StyleSelector
    {
        public Style BadgeStyle { get; set; }
        public Style BadgeFolderStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            if (item is ArgPoint)
                return BadgeStyle;
            else
                return BadgeFolderStyle;
        }
    }
}