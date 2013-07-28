using System;
using System.Windows;
using System.Windows.Data;

namespace CloudStorage.Converters
{
    [ValueConversion(typeof (string), typeof (string))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool) value)
                return Visibility.Visible;
            
            return Visibility.Collapsed;            
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}