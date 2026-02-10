using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace Nodify
{
    public class StringToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public bool NullVisibility { get; set; } = false;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) 
            => string.IsNullOrEmpty(value as string) ? NullVisibility : true;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
