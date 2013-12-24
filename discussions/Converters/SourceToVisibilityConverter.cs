using System;
using System.Windows;
using System.Windows.Data;
using Discussions.DbModel;

namespace Discussions.Converters
{
    public class SourceToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var src = value as Source;
            if (src == null)
                return Visibility.Hidden;

            var currentPers = SessionInfo.Get().person;
            if (currentPers == null)
                return Visibility.Hidden;

            if (src.RichText.ArgPoint.Person == null)
                return Visibility.Hidden;

            if (src.RichText.ArgPoint.Person.Id == currentPers.Id)
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