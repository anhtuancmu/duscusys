using System;
using System.Windows;
using System.Windows.Data;
using Discussions.model;

namespace Discussions.VectorEditor
{
    public class PersonExtToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (((PersonExt)value).HasPointsWithUnreadComments)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}