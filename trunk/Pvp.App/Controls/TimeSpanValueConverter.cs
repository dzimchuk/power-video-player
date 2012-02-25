using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Dzimchuk.Pvp.App.Controls
{
    internal class TimeSpanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var time = (TimeSpan)value;
            return string.Format("{0:d2}:{1:d2}:{2:d2}", time.Hours, time.Minutes, time.Seconds);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
