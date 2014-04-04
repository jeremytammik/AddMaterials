using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using AddMaterials.ViewModel.Enum;

namespace AddMaterials.View.Converters
{
    public class StatusToErrorSignVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Status))
                return Visibility.Collapsed;
            var status = (Status) value;

            return status != Status.Normal ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}