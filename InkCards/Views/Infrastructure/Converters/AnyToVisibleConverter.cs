using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace InkCards.Views.Infrastructure.Converters
{
    class AnyToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var valueTyped = (int)value;

            return valueTyped > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
