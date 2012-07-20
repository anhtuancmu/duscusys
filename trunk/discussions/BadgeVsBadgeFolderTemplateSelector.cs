using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Discussions.DbModel;

namespace Discussions
{
    public class BadgeVsBadgeFolderTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BadgeTemplate { get; set; }
        public DataTemplate BadgeFolderTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ArgPoint)
                return BadgeTemplate;
            else
                return BadgeFolderTemplate;
        }
    }
}
