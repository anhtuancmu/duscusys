using System;
using System.Windows.Data;
using System.Windows;

namespace Discussions
{
    public class ModeratorToVisibilityConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (SessionInfo.Get().person == null)
                return Visibility.Collapsed;

            if (SessionInfo.Get().person.Name.StartsWith(DaoUtils.MODER_SUBNAME))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}