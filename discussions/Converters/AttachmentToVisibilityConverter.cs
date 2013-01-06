using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Discussions.YouViewer;
using Discussions.DbModel;
using System.Windows;

namespace Discussions
{
    public class AttachmentToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var attach = value as Attachment;
            if (attach == null)
                return Visibility.Hidden;

            var currentPers = SessionInfo.Get().person;
            if (currentPers == null)
                return Visibility.Hidden;

            if (attach.Person == null)
                return Visibility.Hidden;

            if (attach.Person.Id == currentPers.Id)
                return Visibility.Visible;

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}