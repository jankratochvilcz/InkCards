using System;
using Windows.UI.Xaml.Data;

namespace InkCards.Views.Infrastructure.Converters
{
    class LongToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var valueTyped = (long)value;
            return (double)valueTyped;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
