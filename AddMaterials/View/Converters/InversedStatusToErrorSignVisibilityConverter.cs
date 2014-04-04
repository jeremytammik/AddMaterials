using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AddMaterials.View.Converters
{
    public class InversedStatusToErrorSignVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var converter = new StatusToErrorSignVisibilityConverter();
            var converted = (Visibility)converter.Convert(value, targetType, parameter, culture);
            return converted == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}