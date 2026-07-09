// -----------------------------------------------------------------------------
//  WPF data-converter shims (System.Windows.Data)
// -----------------------------------------------------------------------------
//  Upstream Nodify implements a few converters:
//
//      internal sealed class UnscaleTransformConverter : IValueConverter { ... }
//      internal sealed class ScaleDoubleConverter : IMultiValueConverter { ... }
//      internal sealed class ScalePointConverter  : IMultiValueConverter { ... }
//
//  Avalonia's IValueConverter (Avalonia.Data.Converters) has the *same* method
//  signatures as WPF's, so it is exposed here through a type alias -- no shim class
//  is needed and single-value converters port verbatim.
//
//  Avalonia's IMultiValueConverter is shaped *differently* (it takes
//  IList<object?> values and has no ConvertBack), so this file declares a
//  WPF-shaped IMultiValueConverter (object[] values + ConvertBack) that ALSO
//  implements Avalonia's interface, bridging Convert(IList<object?>) to the WPF
//  Convert(object[]) so upstream multi-converters compile and run under Avalonia's
//  binding engine.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System.Windows.Data
{
    /// <summary>
    /// WPF-compatible multi-value converter. Upstream converters implement this WPF-shaped
    /// interface (<c>object[]</c> values plus <c>ConvertBack</c>); the default
    /// <see cref="Avalonia.Data.Converters.IMultiValueConverter.Convert(IList{object?}, Type, object?, CultureInfo)"/>
    /// implementation forwards to it so they work with Avalonia's binding engine.
    /// </summary>
    public interface IMultiValueConverter : Avalonia.Data.Converters.IMultiValueConverter
    {
        /// <summary>Converts source values to a target value (WPF signature).</summary>
        object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

        /// <summary>Converts a target value back to source values (WPF signature).</summary>
        object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);

        /// <summary>
        /// Avalonia entry point. Bridges Avalonia's <see cref="IList{T}"/> value list to the
        /// WPF <c>object[]</c> overload. Explicitly implemented so implementers only write the
        /// WPF-shaped methods.
        /// </summary>
        object? Avalonia.Data.Converters.IMultiValueConverter.Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
            => Convert(values.ToArray()!, targetType, parameter!, culture);
    }
}
