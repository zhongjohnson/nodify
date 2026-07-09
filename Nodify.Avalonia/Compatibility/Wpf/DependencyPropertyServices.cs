// -----------------------------------------------------------------------------
//  Dependency-property runtime services
// -----------------------------------------------------------------------------
//  Implements the WPF value accessors (GetValue/SetValue/SetCurrentValue/ClearValue/
//  CoerceValue) on top of Avalonia's AvaloniaObject, plus the change/coerce dispatch
//  that shim control bases forward from their OnPropertyChanged override.
//
//  These are exposed to the linked sources as instance methods on the shim control
//  bases (for unqualified `GetValue(...)` calls) and as extension methods on
//  DependencyObject (for qualified `element.GetValue(...)` calls).
// -----------------------------------------------------------------------------

using System;
using Avalonia;

namespace System.Windows
{
    internal static class DependencyPropertyServices
    {
        public static object? GetValue(AvaloniaObject target, DependencyProperty property)
            => target.GetValue(property.AvaloniaProperty);

        public static void SetValue(AvaloniaObject target, DependencyProperty property, object? value)
        {
            if (property.ReadOnly)
            {
                throw new InvalidOperationException($"'{property.Name}' is read-only and can only be set through its DependencyPropertyKey.");
            }

            _ = target.SetValue(property.AvaloniaProperty, value);
        }

        public static void SetValue(AvaloniaObject target, DependencyPropertyKey key, object? value)
            => target.SetValue(key.DependencyProperty.AvaloniaProperty, value);

        public static void SetCurrentValue(AvaloniaObject target, DependencyProperty property, object? value)
            => target.SetValue(property.AvaloniaProperty, value);

        public static void ClearValue(AvaloniaObject target, DependencyProperty property)
            => target.ClearValue(property.AvaloniaProperty);

        public static void ClearValue(AvaloniaObject target, DependencyPropertyKey key)
            => target.ClearValue(key.DependencyProperty.AvaloniaProperty);

        public static object? CoerceValue(AvaloniaObject target, DependencyProperty property)
        {
            PropertyMetadata metadata = property.GetMetadata(target.GetType());
            object? current = target.GetValue(property.AvaloniaProperty);

            if (metadata.CoerceValueCallback is { } coerce)
            {
                object? coerced = coerce(target, current);
                if (!Equals(coerced, current))
                {
                    _ = target.SetValue(property.AvaloniaProperty, coerced);
                }

                return coerced;
            }

            return current;
        }

        /// <summary>
        /// Forwarded from a shim control's <see cref="AvaloniaObject.OnPropertyChanged"/> override to
        /// run WPF coercion and invoke the registered <see cref="PropertyChangedCallback"/>.
        /// </summary>
        public static void OnPropertyChanged(AvaloniaObject target, AvaloniaPropertyChangedEventArgs change)
        {
            DependencyProperty? property = DependencyProperty.FromAvalonia(change.Property);
            if (property == null)
            {
                return;
            }

            PropertyMetadata metadata = property.GetMetadata(target.GetType());

            if (metadata.CoerceValueCallback is { } coerce)
            {
                object? newValue = change.NewValue;
                object? coerced = coerce(target, newValue);
                if (!Equals(coerced, newValue))
                {
                    _ = target.SetValue(change.Property, coerced);
                    // The re-set raises another change notification that carries the callback.
                    return;
                }
            }

            if (metadata.PropertyChangedCallback is { } callback)
            {
                callback(target, new DependencyPropertyChangedEventArgs(property, change.OldValue, change.NewValue));
            }
        }
    }

    /// <summary>
    /// Qualified WPF accessors (<c>element.GetValue(prop)</c>) for any <see cref="DependencyObject"/>.
    /// Unqualified calls inside the shim controls resolve to instance methods declared on the bases.
    /// </summary>
    public static class DependencyObjectValueExtensions
    {
        public static object? GetValue(this AvaloniaObject target, DependencyProperty property)
            => DependencyPropertyServices.GetValue(target, property);

        public static void SetValue(this AvaloniaObject target, DependencyProperty property, object? value)
            => DependencyPropertyServices.SetValue(target, property, value);

        public static void SetValue(this AvaloniaObject target, DependencyPropertyKey key, object? value)
            => DependencyPropertyServices.SetValue(target, key, value);

        public static void SetCurrentValue(this AvaloniaObject target, DependencyProperty property, object? value)
            => DependencyPropertyServices.SetCurrentValue(target, property, value);

        public static void ClearValue(this AvaloniaObject target, DependencyProperty property)
            => DependencyPropertyServices.ClearValue(target, property);

        public static object? CoerceValue(this AvaloniaObject target, DependencyProperty property)
            => DependencyPropertyServices.CoerceValue(target, property);
    }
}
