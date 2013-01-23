using System;
using System.Globalization;
using System.Windows.Data;
using Pvp.App.ViewModel;

namespace Pvp.App.Controls
{
    internal class TimeSpanToDoubleValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var position = (TimeSpan)value;
            var duration = ((IDurationProvider)parameter).Duration.TotalMilliseconds;

            return duration != 0.0 ? position.TotalMilliseconds / duration : 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var position = (double)value;
            var duration = ((IDurationProvider)parameter).Duration.TotalMilliseconds;

            return TimeSpan.FromMilliseconds(duration * position);
        }
    }
}