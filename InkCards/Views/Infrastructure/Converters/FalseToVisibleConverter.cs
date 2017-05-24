using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace InkCards.Views.Infrastructure.Converters
{
    class FalseToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
                return !(bool)value
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            if (value is bool?)
                return !((bool?)value).HasValue || !((bool?)value).Value
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            throw new InvalidOperationException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
