using System;
using System.Globalization;
using System.Windows.Data;
using AddMaterials.ViewModel.Enum;

namespace AddMaterials.View.Converters
{
    public class StatusToErrorMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Status))
                return string.Empty;
            var status = (Status) value;
            switch (status)
            {
                case Status.ProjectAlreadyContainsMaterialWithTheSameName:
                    return "Project already contains material with the same name";
                case Status.BaseMaterialClassNotFound:
                    return "Base material class not found in the project";
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}