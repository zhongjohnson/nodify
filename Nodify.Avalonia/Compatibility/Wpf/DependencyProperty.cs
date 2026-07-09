// -----------------------------------------------------------------------------
//  WPF DependencyProperty / DependencyPropertyKey shims (System.Windows)
// -----------------------------------------------------------------------------
//  A WPF DependencyProperty is untyped and registered per (name, ownerType).
//  Avalonia uses strongly-typed generic StyledProperty<TValue>/AttachedProperty<TValue>.
//  This shim wraps a real Avalonia property (created via reflection so the runtime
//  value type is preserved for XAML/animation), and keeps a per-owner metadata table
//  so WPF change/coerce callbacks and default-value/metadata overrides keep working.
//
//  Change and coerce callbacks are dispatched by the shim control bases from their
//  AvaloniaObject.OnPropertyChanged override (see DependencyPropertyServices).
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Data;

namespace System.Windows
{
    /// <summary>WPF-compatible dependency property backed by an Avalonia property.</summary>
    public class DependencyProperty
    {
        private static readonly Dictionary<AvaloniaProperty, DependencyProperty> ByAvalonia = new();

        private readonly Dictionary<Type, PropertyMetadata> _metadataByOwner = new();

        private DependencyProperty(AvaloniaProperty avaloniaProperty, string name, Type propertyType, Type ownerType, PropertyMetadata defaultMetadata, bool isAttached)
        {
            AvaloniaProperty = avaloniaProperty;
            Name = name;
            PropertyType = propertyType;
            OwnerType = ownerType;
            DefaultMetadata = defaultMetadata;
            IsAttached = isAttached;
            _metadataByOwner[ownerType] = defaultMetadata;

            lock (ByAvalonia)
            {
                ByAvalonia[avaloniaProperty] = this;
            }
        }

        /// <summary>The underlying Avalonia property that stores the value.</summary>
        internal AvaloniaProperty AvaloniaProperty { get; }

        public string Name { get; }

        public Type PropertyType { get; }

        public Type OwnerType { get; private set; }

        public bool IsAttached { get; }

        public bool ReadOnly { get; internal set; }

        internal PropertyMetadata DefaultMetadata { get; }

        /// <summary>Resolves the shim wrapper for an Avalonia property (used by change routing).</summary>
        internal static DependencyProperty? FromAvalonia(AvaloniaProperty avaloniaProperty)
        {
            lock (ByAvalonia)
            {
                return ByAvalonia.TryGetValue(avaloniaProperty, out var dp) ? dp : null;
            }
        }

        /// <summary>Returns the metadata that applies to <paramref name="forType"/> (most derived override wins).</summary>
        internal PropertyMetadata GetMetadata(Type forType)
        {
            for (Type? t = forType; t != null; t = t.BaseType)
            {
                if (_metadataByOwner.TryGetValue(t, out var metadata))
                {
                    return metadata;
                }
            }

            return DefaultMetadata;
        }

        public static DependencyProperty Register(string name, Type propertyType, Type ownerType)
            => Register(name, propertyType, ownerType, null);

        public static DependencyProperty Register(string name, Type propertyType, Type ownerType, PropertyMetadata? typeMetadata)
        {
            PropertyMetadata metadata = typeMetadata ?? new FrameworkPropertyMetadata();
            AvaloniaProperty avaloniaProperty = AvaloniaPropertyFactory.CreateStyled(
                ownerType, propertyType, name, ResolveDefault(metadata, propertyType), Inherits(metadata), TwoWay(metadata));

            var dp = new DependencyProperty(avaloniaProperty, name, propertyType, ownerType, metadata, isAttached: false);
            AvaloniaPropertyFactory.ApplyAffectFlags(avaloniaProperty, ownerType, metadata as FrameworkPropertyMetadata);
            return dp;
        }

        public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType)
            => RegisterAttached(name, propertyType, ownerType, null);

        public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType, PropertyMetadata? defaultMetadata)
        {
            PropertyMetadata metadata = defaultMetadata ?? new FrameworkPropertyMetadata();
            AvaloniaProperty avaloniaProperty = AvaloniaPropertyFactory.CreateAttached(
                ownerType, propertyType, name, ResolveDefault(metadata, propertyType), Inherits(metadata));

            return new DependencyProperty(avaloniaProperty, name, propertyType, ownerType, metadata, isAttached: true);
        }

        public static DependencyPropertyKey RegisterReadOnly(string name, Type propertyType, Type ownerType, PropertyMetadata typeMetadata)
        {
            DependencyProperty dp = Register(name, propertyType, ownerType, typeMetadata);
            dp.ReadOnly = true;
            return new DependencyPropertyKey(dp);
        }

        public DependencyProperty AddOwner(Type ownerType) => AddOwner(ownerType, null);

        public DependencyProperty AddOwner(Type ownerType, PropertyMetadata? typeMetadata)
        {
            // Avalonia properties are shared across owner types, so the same underlying
            // property is reused. Only metadata (default value / affect flags) is per-owner.
            if (typeMetadata != null)
            {
                _metadataByOwner[ownerType] = typeMetadata;
                if (typeMetadata.HasDefaultValue)
                {
                    AvaloniaPropertyFactory.OverrideDefaultValue(AvaloniaProperty, ownerType, PropertyType, typeMetadata.DefaultValue);
                }

                AvaloniaPropertyFactory.ApplyAffectFlags(AvaloniaProperty, ownerType, typeMetadata as FrameworkPropertyMetadata);
            }

            return this;
        }

        public void OverrideMetadata(Type forType, PropertyMetadata typeMetadata)
        {
            _metadataByOwner[forType] = typeMetadata;

            if (typeMetadata.HasDefaultValue)
            {
                AvaloniaPropertyFactory.OverrideDefaultValue(AvaloniaProperty, forType, PropertyType, typeMetadata.DefaultValue);
            }

            AvaloniaPropertyFactory.ApplyAffectFlags(AvaloniaProperty, forType, typeMetadata as FrameworkPropertyMetadata);
        }

        private static object? ResolveDefault(PropertyMetadata metadata, Type propertyType)
        {
            if (metadata.DefaultValue != null)
            {
                return metadata.DefaultValue;
            }

            return propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
        }

        private static bool Inherits(PropertyMetadata metadata) => metadata is FrameworkPropertyMetadata f && f.Inherits;

        private static bool TwoWay(PropertyMetadata metadata) => metadata is FrameworkPropertyMetadata f && f.BindsTwoWayByDefault;
    }

    /// <summary>Grants write access to a read-only <see cref="DependencyProperty"/>.</summary>
    public sealed class DependencyPropertyKey
    {
        internal DependencyPropertyKey(DependencyProperty dependencyProperty)
            => DependencyProperty = dependencyProperty;

        public DependencyProperty DependencyProperty { get; }

        public void OverrideMetadata(Type forType, PropertyMetadata typeMetadata)
            => DependencyProperty.OverrideMetadata(forType, typeMetadata);
    }

    /// <summary>
    /// Bridges WPF-style registration onto Avalonia's generic property API through reflection.
    /// Non-essential metadata application (affect flags, default overrides) is best-effort so
    /// that registration never throws during static initialization of the linked controls.
    /// </summary>
    internal static class AvaloniaPropertyFactory
    {
        private static readonly MethodInfo RegisterStyledOpen = typeof(AvaloniaProperty)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(AvaloniaProperty.Register)
                        && m.IsGenericMethodDefinition
                        && m.GetGenericArguments().Length == 2
                        && m.GetParameters() is { Length: >= 4 } p
                        && p[0].ParameterType == typeof(string));

        private static readonly MethodInfo RegisterAttachedOpen = typeof(AvaloniaProperty)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(AvaloniaProperty.RegisterAttached)
                        && m.IsGenericMethodDefinition
                        && m.GetGenericArguments().Length == 3
                        && m.GetParameters() is { Length: >= 3 } p
                        && p[0].ParameterType == typeof(string));

        public static AvaloniaProperty CreateStyled(Type ownerType, Type valueType, string name, object? defaultValue, bool inherits, bool twoWay)
        {
            MethodInfo method = RegisterStyledOpen.MakeGenericMethod(ownerType, valueType);
            ParameterInfo[] parameters = method.GetParameters();
            var args = new object?[parameters.Length];

            args[0] = name;
            args[1] = CoerceToType(defaultValue, valueType);
            if (parameters.Length > 2)
            {
                args[2] = inherits;
            }

            if (parameters.Length > 3)
            {
                args[3] = twoWay ? BindingMode.TwoWay : BindingMode.Default;
            }

            for (int i = 4; i < parameters.Length; i++)
            {
                args[i] = parameters[i].HasDefaultValue ? Type.Missing : Default(parameters[i].ParameterType);
            }

            return (AvaloniaProperty)method.Invoke(null, args)!;
        }

        public static AvaloniaProperty CreateAttached(Type ownerType, Type valueType, string name, object? defaultValue, bool inherits)
        {
            // RegisterAttached<TOwner, THost, TValue>(...) -- host is any AvaloniaObject-derived control.
            MethodInfo method = RegisterAttachedOpen.MakeGenericMethod(ownerType, typeof(Avalonia.Controls.Control), valueType);
            ParameterInfo[] parameters = method.GetParameters();
            var args = new object?[parameters.Length];

            args[0] = name;
            args[1] = CoerceToType(defaultValue, valueType);
            if (parameters.Length > 2)
            {
                args[2] = inherits;
            }

            for (int i = 3; i < parameters.Length; i++)
            {
                args[i] = parameters[i].HasDefaultValue ? Type.Missing : Default(parameters[i].ParameterType);
            }

            return (AvaloniaProperty)method.Invoke(null, args)!;
        }

        public static void OverrideDefaultValue(AvaloniaProperty property, Type ownerType, Type valueType, object? value)
        {
            try
            {
                MethodInfo? generic = property.GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.Name == "OverrideDefaultValue"
                                         && m.IsGenericMethodDefinition
                                         && m.GetParameters().Length == 1);

                if (generic != null)
                {
                    generic.MakeGenericMethod(ownerType).Invoke(property, new[] { CoerceToType(value, valueType) });
                }
            }
            catch
            {
                // Best-effort: an unsupported default override must not break registration.
            }
        }

        public static void ApplyAffectFlags(AvaloniaProperty property, Type ownerType, FrameworkPropertyMetadata? metadata)
        {
            if (metadata == null)
            {
                return;
            }

            if (metadata.AffectsRender)
            {
                InvokeAffects(typeof(Avalonia.Visual), "AffectsRender", ownerType, property);
            }

            if (metadata.AffectsMeasure)
            {
                InvokeAffects(typeof(Avalonia.Layout.Layoutable), "AffectsMeasure", ownerType, property);
            }

            if (metadata.AffectsArrange)
            {
                InvokeAffects(typeof(Avalonia.Layout.Layoutable), "AffectsArrange", ownerType, property);
            }
        }

        private static void InvokeAffects(Type declaringType, string methodName, Type ownerType, AvaloniaProperty property)
        {
            try
            {
                MethodInfo? open = declaringType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == methodName && m.IsGenericMethodDefinition);

                if (open == null || !declaringType.IsAssignableFrom(ownerType))
                {
                    return;
                }

                MethodInfo closed = open.MakeGenericMethod(ownerType);
                // The helpers accept a params AvaloniaProperty[] argument.
                var array = Array.CreateInstance(typeof(AvaloniaProperty), 1);
                array.SetValue(property, 0);
                closed.Invoke(null, new object?[] { array });
            }
            catch
            {
                // Best-effort: affect-flag wiring is a rendering optimization, not correctness-critical.
            }
        }

        private static object? CoerceToType(object? value, Type targetType)
        {
            if (value == null)
            {
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
            }

            if (targetType.IsInstanceOfType(value))
            {
                return value;
            }

            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return value;
            }
        }

        private static object? Default(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
