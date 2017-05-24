using System;
using Windows.UI.Xaml.Data;

namespace InkCards.Views.Infrastructure.Converters
{
    class BoolToNullableBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return false;
            return (bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return false;
            return (bool)value;
        }
    }
}
