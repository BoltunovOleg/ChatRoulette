﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ChatRoulette.Core.Session;
using ChatRoulette.Utils;

namespace ChatRoulette.Converters
{
    public class StatusToVisibilityConverter : IValueConverter
    {
        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Status v)
            {
                switch (v)
                {
                    case Status.Wait:
                        return this.TrueValue;
                    case Status.EnableCamera:
                    case Status.Start:
                    case Status.Error1:
                    case Status.Error2:
                    case Status.Error3:
                    case Status.Error4:
                    case Status.Error7:
                    case Status.Ban:
                    case Status.PartnerDisconnected:
                    case Status.PartnerConnected:
                    case Status.PutResult:
                        return this.FalseValue;
                    default:
                        return this.TrueValue;
                }
            }
            return this.FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}