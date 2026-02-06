using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Nodify
{
    internal sealed class UnscaleTransformConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is TransformGroup group && group.Children.Count > 0)
            {
                var inverse = group.Children[0].Value.Invert();
                return new MatrixTransform(inverse);
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }

    internal sealed class ScaleDoubleConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count >= 2 && values[0] is double d1 && values[1] is double d2)
            {
                return d1 * d2;
            }
            return 0.0;
        }
    }

    internal sealed class ScalePointConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count >= 2 && values[0] is Point p && values[1] is double scale)
            {
                return new Point(p.X * scale, p.Y * scale);
            }
            return new Point();
        }
    }
}
