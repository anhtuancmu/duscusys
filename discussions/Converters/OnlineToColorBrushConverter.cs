using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Discussions
{
    public class OnlineToColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //in order to handle design time problems, handle null value case
            if (value == null)
                return null;

            if ((bool) value)
                return new SolidColorBrush(Colors.LightGreen);
            else
                return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}