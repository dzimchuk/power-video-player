using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Pvp.App.View
{
    internal class PercentToStringValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var percent = double.Parse((string)parameter, CultureInfo.InvariantCulture);

            CultureInfo ci = new CultureInfo(System.Threading.Thread.CurrentThread.CurrentCulture.Name); // otherwise NumberFormat is readonly
            NumberFormatInfo nfi = ci.NumberFormat;
            nfi.PercentDecimalDigits = 0;

            return percent.ToString("P", ci);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
