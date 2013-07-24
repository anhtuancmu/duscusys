﻿using System;
using System.Windows.Data;
using System.IO;

namespace QuickZip.Tools
{
    [ValueConversion(typeof (string), typeof (string))]
    public class PathToNameConverter : IValueConverter
    {
        public static PathToNameConverter Instance = new PathToNameConverter();

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string fileName = (string) value;
            return Path.GetFileName(fileName);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}