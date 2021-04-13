using System;
using System.Globalization;
using System.Windows.Data;
using Pvp.Core.MediaEngine;

namespace Pvp.App.View
{
    internal class VideoSizeToBooleanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterSize = (VideoSize)parameter;
            var size = (VideoSize)value;

            return size == parameterSize;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (VideoSize)parameter;
        }
    }
}