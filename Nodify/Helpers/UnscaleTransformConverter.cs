using System;
using System.Globalization;
using System;
using System.Globalization;
using System.Collections.Generic;

#if Avalonia
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Nodify.Avalonia;
using Nodify.Avalonia.Helpers.Gestures;
using Nodify.Avalonia.Helpers;
using Nodify.Avalonia.Extensions;
using CommandBinding = Nodify.Avalonia.RoutedCommandBinding;
using MultiSelector = Avalonia.Controls.Primitives.SelectingItemsControl;
#else
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
#endif

namespace Nodify
{
#if Avalonia
    internal class UnscaleTransformConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is double x && values[1] is double y)
            {
                return new ScaleTransform(1 / x, 1 / y);
            }

            return new ScaleTransform(1, 1);
        }
    }
    internal class UnscaleDoubleConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is double && values[1] is double) //todo
            {
                double result = (double)values[0] * (double)values[1];
                return result;
            }

            return 0d;
        }
    }

    public class NodifyConverters
    {
        public static IMultiValueConverter UnscaleDoubleConverter { get; } = new UnscaleDoubleConverter();
        public static IMultiValueConverter UnscaleTransformConverter { get; } = new UnscaleTransformConverter();
    }
#else
    internal class UnscaleTransformConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Transform result = (Transform)((TransformGroup)value).Children[0].Inverse;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
#endif

    internal class ScaleDoubleConverter : IMultiValueConverter
    {
#if Avalonia
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
#else
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
#endif
        {
            double result = (double)values[0] * (double)values[1];
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ScalePointConverter : IMultiValueConverter
    {
#if Avalonia
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
#else
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
#endif
        {
            Point result = (Point)((Vector)(Point)values[0] * (double)values[1]);
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
