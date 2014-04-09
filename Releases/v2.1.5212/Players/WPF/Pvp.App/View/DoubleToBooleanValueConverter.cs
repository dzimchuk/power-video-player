using System;
using System.Globalization;
using System.Windows.Data;

namespace Pvp.App.View
{
    internal class DoubleToBooleanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterValue = double.Parse((string)parameter, CultureInfo.InvariantCulture);
            var theValue = (double)value;

            return theValue == parameterValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)parameter;
        }
    }
}