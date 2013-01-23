using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Pvp.App.ViewModel;

namespace Pvp.App.View
{
    internal class MouseWheelActionToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var action = (MouseWheelAction)value;
            var param = (MouseWheelAction)parameter;

            return action == param;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (MouseWheelAction)parameter;
        }
    }
}