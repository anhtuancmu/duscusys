using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Discussions.DbModel;

namespace Discussions
{
    public class LinkTruncater : IValueConverter
    {
        public int LinkLen { get; set; }

        public LinkTruncater()
        {
            LinkLen = 20;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //in order to handle design time problems, handle null value case
            if (value == null)
                return null;

            string fullLink = (value as Source).Text;
            return fullLink.Substring(0, LinkLen);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}