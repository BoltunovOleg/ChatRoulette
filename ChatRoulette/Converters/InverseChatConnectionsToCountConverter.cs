using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ChatRoulette.Repository.Model;

namespace ChatRoulette.Converters
{
    public class InverseChatConnectionsToCountConverter : IValueConverter
    {
        public ChatConnectionResultEnum Type { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is List<ChatConnection> connections))
                return -1;
            return connections.Count(y => y.Result != Type);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}