using System;
using System.Windows.Data;

namespace Discussions
{
    //int32 <-> System.Windows.Media.Color for color picker 

    public class ARGBColorConvertor : IValueConverter
    {
        //int32 -> color
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //in order to handle design time problems, handle null value case
            if (value == null)
                return null;

            int i = (int) value;
            return Utils.IntToColor(i);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            var c = (System.Windows.Media.Color) value;
            return Utils.ColorToInt(c);
        }
    }
}