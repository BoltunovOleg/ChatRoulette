using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ChatRoulette.Repository.Model;

namespace ChatRoulette.Converters
{
    public class ChatSessionToConnectionsCount : IValueConverter
    {
        public ChatConnectionResultEnum Type { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is List<ChatSession> session))
                return -1;
            return session.Sum(x=> x.ChatConnections.Count);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}