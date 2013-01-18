using System;
using System.Windows.Data;

namespace Discussions
{
    public class SummaryTextConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //in order to handle design time problems, handle null value case
            if (value == null)
                return null;

            string fullText = value as string;
            string[] lines = fullText.Split('\n');
            if (lines.Length > 1)
                return ShortenLine(lines[0]);
            else
                return ShortenLine(fullText);
        }

        public static string ShortenLine(string line, int len=40)
        {
            if (line.Length > len)
                return line.Substring(0, Math.Min(line.Length - 1, len)) + "...";
            else
                return line;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}