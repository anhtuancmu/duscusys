using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Discussions.YouViewer;
using Discussions.DbModel;

namespace Discussions
{
    public class AttachmentToVideoConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //in order to handle design time problems, handle null value case
            if (value == null)
                return null;

            return AttachToYtInfo(value as Attachment);     
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        
        public static YouTubeInfo AttachToYtInfo(Attachment a)
        {           
            YouTubeInfo info = new YouTubeInfo();
            info.ThumbNailUrl = a.VideoThumbURL;
            info.EmbedUrl = a.VideoEmbedURL;
            info.LinkUrl = a.VideoLinkURL;
            return info;     
        }
    }
}
