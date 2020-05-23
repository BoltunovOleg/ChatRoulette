using System;
using System.Globalization;
using System.Windows.Data;

namespace ChatRoulette.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool v)
                return !v;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool v)
                return !v;
            return false;
        }
    }
}