using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Pvp.App.View
{
    internal class RegularControlPanelVisibilityValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isFullScreen = (bool)values[0];
            bool isVisible = (bool)values[1];

            return isVisible ? (isFullScreen ? Visibility.Collapsed : Visibility.Visible) : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
