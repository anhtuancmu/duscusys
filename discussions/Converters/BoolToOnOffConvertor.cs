using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Discussions.YouViewer;
using Discussions.DbModel;
using System.Windows.Media;

namespace Discussions
{
    public class BoolToOnOffConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //in order to handle design time problems, handle null value case
            if (value == null)
                return null;

            if ((bool) value)
                return "On";
            else
                return "Off";
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}