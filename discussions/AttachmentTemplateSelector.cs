using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Discussions.model;
using System.Windows.Controls;
using Discussions.DbModel;

namespace Discussions
{
    public class AttachmentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ImageTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is Attachment))
                return null;

            Attachment a = (Attachment)item;
            if(a.Format == (int)AttachmentFormat.Youtube)
                return VideoTemplate;
            else
                return ImageTemplate;
        }
    }
}
