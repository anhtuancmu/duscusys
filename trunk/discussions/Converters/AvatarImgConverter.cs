using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Discussions.DbModel;

namespace Discussions
{
    public class AvatarImgConverter : IValueConverter
    {
        static BitmapImage _emptyAvatar = null;
        public static BitmapImage EmptyAvatar
        {
            get
            {
                if (_emptyAvatar == null)
                {
                    _emptyAvatar = new BitmapImage(new Uri("pack://application:,,,/LoginEngine;component/Resources/non_avatar.jpg"));
                }
                    
                return _emptyAvatar;
            }
        }
                
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {            
            if (value == null)
            {
                //no avatar, return empty avatar
                return EmptyAvatar;
            }

            return MiniAttachmentManager.GetAttachmentBitmap2(value as Attachment);                               
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
