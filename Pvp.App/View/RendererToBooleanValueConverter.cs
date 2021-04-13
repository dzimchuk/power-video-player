using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Pvp.Core.MediaEngine;

namespace Pvp.App.View
{
    internal class RendererToBooleanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var renderer = (Renderer)value;
            var parameterRenderer = (Renderer)parameter;

            return renderer == parameterRenderer;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Renderer)parameter;
        }
    }
}