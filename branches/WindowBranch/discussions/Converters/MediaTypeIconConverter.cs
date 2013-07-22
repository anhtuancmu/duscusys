using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Discussions.YouViewer;
using Discussions.DbModel;
using Discussions.model;
using System.Windows;

namespace Discussions
{
    public class MediaTypeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var attach = value as Attachment;
            if (attach == null)
                return null;

            switch ((AttachmentFormat) attach.Format)
            {
                case AttachmentFormat.Bmp:
                case AttachmentFormat.Jpg:
                case AttachmentFormat.Png:
                    return Application.Current.TryFindResource("flower");
                case AttachmentFormat.PngScreenshot:
                    return Application.Current.TryFindResource("camera");
                case AttachmentFormat.Pdf:
                    return Application.Current.TryFindResource("book");
                case AttachmentFormat.Youtube:
                    return Application.Current.TryFindResource("ytIcon");
                case AttachmentFormat.WordDocSet:
                    return Application.Current.TryFindResource("msOffice");
                case AttachmentFormat.PowerPointDocSet:
                    return Application.Current.TryFindResource("msOffice");
                case AttachmentFormat.ExcelDocSet:
                    return Application.Current.TryFindResource("msOffice");
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}