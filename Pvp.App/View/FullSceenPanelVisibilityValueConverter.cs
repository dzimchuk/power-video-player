using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Dzimchuk.Pvp.App.View
{
    internal class FullSceenPanelVisibilityValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isFullScreen = (bool)values[0];
            bool isVisible = (bool)values[1];

            return isVisible ? (isFullScreen ? Visibility.Visible : Visibility.Collapsed) : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
