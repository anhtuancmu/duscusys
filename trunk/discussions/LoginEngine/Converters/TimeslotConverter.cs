using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Discussions.DbModel.model;

namespace Discussions
{
    public class TimeslotConverter : IValueConverter
    {
        //int -> string
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            int ts = 0;
            try
            {
                ts = (int) ((double) value);
            }
            catch (Exception)
            {
            }

            switch (ts)
            {
                case (int) TimeSlot.Evening:
                    return "Evening";
                case (int) TimeSlot.Noon:
                    return "Noon";
                case (int) TimeSlot.Morning:
                    return "Morning";
                default:
                    throw new Exception("no more time slots!");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}