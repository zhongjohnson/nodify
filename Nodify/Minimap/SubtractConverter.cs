using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Nodify
{
    internal sealed class SubtractConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count >= 2 && values[0] is double d1 && values[1] is double d2)
            {
                return d1 - d2;
            }
            return 0.0;
        }
    }
}
