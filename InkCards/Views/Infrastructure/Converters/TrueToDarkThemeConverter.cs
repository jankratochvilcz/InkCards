using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace InkCards.Views.Infrastructure.Converters
{
    class TrueToDarkThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return ApplicationTheme.Light;
            var valueTyped = (bool)value;

            return valueTyped
                ? ElementTheme.Dark
                : ElementTheme.Light;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
