using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Pvp.App.View
{
    internal class SupportedLanguageToBooleanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var lang = (SupportedLanguage)value;
            var param = (SupportedLanguage)parameter;

            return lang == param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (SupportedLanguage)parameter;
        }
    }
}