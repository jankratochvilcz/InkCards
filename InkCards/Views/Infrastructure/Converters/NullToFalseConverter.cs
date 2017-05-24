using System;
using Windows.UI.Xaml.Data;

namespace InkCards.Views.Infrastructure.Converters
{
    public class NullToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => value != null;

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
