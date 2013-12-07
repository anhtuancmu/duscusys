using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Discussions.DbModel.model;

namespace Discussions
{
    public class DevTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            switch ((DeviceType) value)
            {
                case DeviceType.Android:
                    return " app";
                case DeviceType.Sticky:
                    return " sticky";
                case DeviceType.Wpf:
                    return " Windows";
                default:
                    throw new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}