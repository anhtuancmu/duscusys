using System.Windows;
using System.Windows.Controls;
using Discussions.DbModel;

namespace Discussions.selectors
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