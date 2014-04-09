using System;
using System.Globalization;
using System.Windows.Data;
using Pvp.Core.MediaEngine;

namespace Pvp.App.View
{
    internal class AspectRatioToBooleanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterRatio = (AspectRatio)parameter;
            var ratio = (AspectRatio)value;

            return ratio == parameterRatio;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (AspectRatio)parameter;
        }
    }
}