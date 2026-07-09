// -----------------------------------------------------------------------------
//  Nodify.Avalonia port of Nodify\Utilities\UnscaleTransformConverter.cs
// -----------------------------------------------------------------------------
//  This converter set is NOT linked from upstream because its core line relies on
//  WPF's `Transform.Inverse` property, which Avalonia does not expose. The behavior
//  is reproduced with Avalonia's `Matrix.TryInvert(out ...)`:
//
//    WPF:      Transform result = (Transform)((TransformGroup)value).Children[0].Inverse;
//    Avalonia: invert the scale transform's matrix and wrap it in a MatrixTransform.
//
//  Everything else (the two scale multi-converters) is a faithful port. The multi
//  converters implement the WPF-shaped System.Windows.Data.IMultiValueConverter shim
//  (see Compatibility/Wpf/Converters.cs), so their bodies match upstream verbatim.
// -----------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Windows.Data;
using Avalonia;
using Avalonia.Media;

namespace Nodify
{
    /// <summary>
    /// Produces a transform that cancels out the viewport's scale, keeping an element visually
    /// unscaled while it still translates/positions with the viewport. Avalonia port of the WPF
    /// converter that returned <c>((TransformGroup)value).Children[0].Inverse</c>.
    /// </summary>
    internal sealed class UnscaleTransformConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Mirror WPF: invert the *scale* transform (the first child of the viewport transform group).
            Matrix matrix = value switch
            {
                TransformGroup group when group.Children.Count > 0 => group.Children[0].Value,
                Transform transform => transform.Value,
                Matrix m => m,
                _ => Matrix.Identity
            };

            if (matrix.TryInvert(out Matrix inverted))
            {
                return new MatrixTransform(inverted);
            }

            return new MatrixTransform(Matrix.Identity);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }

    /// <summary>Multiplies a <see cref="double"/> by the current zoom (values[0] * values[1]).</summary>
    internal sealed class ScaleDoubleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = (double)values[0] * (double)values[1];
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Scales a <see cref="Point"/> by the current zoom (values[0] * values[1]).</summary>
    internal sealed class ScalePointConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
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
