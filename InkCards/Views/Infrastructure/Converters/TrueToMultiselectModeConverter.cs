using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace InkCards.Views.Infrastructure.Converters
{
    class TrueToMultiselectModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return ListViewSelectionMode.Single;
            return (bool)value
                ? ListViewSelectionMode.Multiple
                : ListViewSelectionMode.Single;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
