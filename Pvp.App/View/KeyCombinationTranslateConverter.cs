using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Pvp.App.View
{
    internal class KeyCombinationTranslateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Resources.Resources.ResourceManager.GetString(string.Format("settings_keyboard_{0}", value).ToLowerInvariant());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
