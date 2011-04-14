using System;
using System.Net;
using System.Windows;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace LCANewsgroup
{
    public class DateToFuzzyStringConvert : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime? dt = value as DateTime?;

            if (dt == null)
            {
                return "Posted a few minutes ago";
            }

            TimeSpan ts = DateTime.Now.Subtract(dt.Value);

            if(ts.Hours == 0 && ts.Days == 0 && ts.Minutes == 0)
            {
                return "Posted a few seconds ago";
            }

            if (ts.Hours == 0 && ts.Days == 0)
            {
                return String.Format("Posted {0} minutes ago", ts.Minutes);
            }

            if (ts.Days == 0)
            {
                return String.Format("Posted {0} hours ago", ts.Hours);
            }

            return String.Format("Posted on {0}", dt.Value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EmptyStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string str = value as string;

            if (String.IsNullOrEmpty(str))
            {
                return "No Text";
            }

            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PostDescriptionConverter : IValueConverter
    {
        static Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value as string;

            if (String.IsNullOrEmpty(str))
            {
                return "No Description";
            }

            var val = _htmlRegex.Replace(str, string.Empty);

            if (!System.ComponentModel.DesignerProperties.IsInDesignTool)
            {
                val = HttpUtility.HtmlDecode(val);
            }

            if (val.Length > 255)
            {
                return String.Concat(val.Substring(0, 255), "...");
            }

            return val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
