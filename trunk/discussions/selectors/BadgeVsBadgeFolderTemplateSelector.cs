using System.Windows;
using System.Windows.Controls;
using Discussions.DbModel;

namespace Discussions.selectors
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