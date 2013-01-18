using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Discussions
{
    public class ARGBColorBrushConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //in order to handle design time problems, handle null value case
            if (value == null)
                return null;

            return new SolidColorBrush(Utils.IntToColor((int) value));
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}