using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Nodify
{
    internal class ResizeDirectionToVisiblityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ResizeDirections resizeDirections))
            {
                return false;
            }

            if (Enum.TryParse(parameter.ToString(), out ResizeDirections direction))
            {
                return resizeDirections.HasFlag(direction);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
