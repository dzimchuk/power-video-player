using System;
using System.Globalization;
using System.Windows.Data;

namespace Pvp.App.View
{
    internal class FileTypeTranslateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Resources.Resources.ResourceManager.GetString(string.Format("file_type_{0}", value).ToLowerInvariant());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}