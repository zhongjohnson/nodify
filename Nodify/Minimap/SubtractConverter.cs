using System;
using System.Globalization;
using System.Collections.Generic;

#if Avalonia
using Avalonia;
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
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
#endif

namespace Nodify
{
    internal class SubtractConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = (double)values[0] - (double)values[1];
            return result;
        }

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
