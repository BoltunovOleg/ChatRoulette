using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatRoulette.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool v)
                return v ? this.TrueValue : this.FalseValue;
            return this.FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v == this.TrueValue;
            return false;
        }
    }
}