using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Pvp.App.View
{
    internal class BooleanToWindowStartupLocationValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var centerWindow = (bool)value;
            return centerWindow ? WindowStartupLocation.CenterScreen : WindowStartupLocation.Manual;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}