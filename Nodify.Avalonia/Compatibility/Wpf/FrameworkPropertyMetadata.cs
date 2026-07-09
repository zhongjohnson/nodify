// -----------------------------------------------------------------------------
//  WPF dependency-property metadata shims (System.Windows)
// -----------------------------------------------------------------------------
//  Mirrors the subset of WPF's PropertyMetadata / FrameworkPropertyMetadata API
//  used by the linked Nodify sources. The metadata is stored verbatim and later
//  translated to Avalonia semantics by DependencyProperty (registration) and by
//  the shim control bases (change/coerce routing).
// -----------------------------------------------------------------------------

using System;

namespace System.Windows
{
    /// <summary>Invoked after a dependency property value changed.</summary>
    public delegate void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e);

    /// <summary>Coerces a dependency property value before it is applied.</summary>
    public delegate object? CoerceValueCallback(DependencyObject d, object? baseValue);

    /// <summary>Validates a dependency property value.</summary>
    public delegate bool ValidateValueCallback(object? value);

    /// <summary>Handles <see cref="DependencyPropertyChangedEventArgs"/> style notifications.</summary>
    public delegate void DependencyPropertyChangedEventHandler(object sender, DependencyPropertyChangedEventArgs e);

    /// <summary>WPF framework metadata flags mapped onto the closest Avalonia behavior.</summary>
    [Flags]
    public enum FrameworkPropertyMetadataOptions
    {
        None = 0,
        AffectsMeasure = 1 << 0,
        AffectsArrange = 1 << 1,
        AffectsParentMeasure = 1 << 2,
        AffectsParentArrange = 1 << 3,
        AffectsRender = 1 << 4,
        Inherits = 1 << 5,
        OverridesInheritanceBehavior = 1 << 6,
        NotDataBindable = 1 << 7,
        BindsTwoWayByDefault = 1 << 8,
        Journal = 1 << 9,
        SubPropertiesDoNotAffectRender = 1 << 10,
    }

    /// <summary>Base metadata carrying a default value and change/coerce callbacks.</summary>
    public class PropertyMetadata
    {
        public PropertyMetadata()
        {
        }

        public PropertyMetadata(object? defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public PropertyMetadata(PropertyChangedCallback? propertyChangedCallback)
        {
            PropertyChangedCallback = propertyChangedCallback;
        }

        public PropertyMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback)
        {
            DefaultValue = defaultValue;
            PropertyChangedCallback = propertyChangedCallback;
        }

        public PropertyMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback, CoerceValueCallback? coerceValueCallback)
        {
            DefaultValue = defaultValue;
            PropertyChangedCallback = propertyChangedCallback;
            CoerceValueCallback = coerceValueCallback;
        }

        public object? DefaultValue { get; set; }

        public PropertyChangedCallback? PropertyChangedCallback { get; set; }

        public CoerceValueCallback? CoerceValueCallback { get; set; }

        /// <summary>True when a default value was explicitly supplied (WPF distinguishes "unset").</summary>
        internal bool HasDefaultValue => DefaultValue is not null || _defaultValueExplicit;

        private protected bool _defaultValueExplicit;
    }

    /// <summary>Framework-level metadata adding layout/render affect flags and binding defaults.</summary>
    public class FrameworkPropertyMetadata : PropertyMetadata
    {
        public FrameworkPropertyMetadata()
        {
        }

        public FrameworkPropertyMetadata(object? defaultValue)
            : base(defaultValue)
        {
            _defaultValueExplicit = true;
        }

        public FrameworkPropertyMetadata(PropertyChangedCallback? propertyChangedCallback)
            : base(propertyChangedCallback)
        {
        }

        public FrameworkPropertyMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback)
            : base(defaultValue, propertyChangedCallback)
        {
            _defaultValueExplicit = true;
        }

        public FrameworkPropertyMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback, CoerceValueCallback? coerceValueCallback)
            : base(defaultValue, propertyChangedCallback, coerceValueCallback)
        {
            _defaultValueExplicit = true;
        }

        public FrameworkPropertyMetadata(object? defaultValue, FrameworkPropertyMetadataOptions flags)
            : base(defaultValue)
        {
            _defaultValueExplicit = true;
            Flags = flags;
        }

        public FrameworkPropertyMetadata(object? defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback? propertyChangedCallback)
            : base(defaultValue, propertyChangedCallback)
        {
            _defaultValueExplicit = true;
            Flags = flags;
        }

        public FrameworkPropertyMetadata(object? defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback? propertyChangedCallback, CoerceValueCallback? coerceValueCallback)
            : base(defaultValue, propertyChangedCallback, coerceValueCallback)
        {
            _defaultValueExplicit = true;
            Flags = flags;
        }

        public FrameworkPropertyMetadataOptions Flags { get; set; }

        public bool AffectsMeasure
        {
            get => Flags.HasFlag(FrameworkPropertyMetadataOptions.AffectsMeasure);
            set => SetFlag(FrameworkPropertyMetadataOptions.AffectsMeasure, value);
        }

        public bool AffectsArrange
        {
            get => Flags.HasFlag(FrameworkPropertyMetadataOptions.AffectsArrange);
            set => SetFlag(FrameworkPropertyMetadataOptions.AffectsArrange, value);
        }

        public bool AffectsParentMeasure
        {
            get => Flags.HasFlag(FrameworkPropertyMetadataOptions.AffectsParentMeasure);
            set => SetFlag(FrameworkPropertyMetadataOptions.AffectsParentMeasure, value);
        }

        public bool AffectsParentArrange
        {
            get => Flags.HasFlag(FrameworkPropertyMetadataOptions.AffectsParentArrange);
            set => SetFlag(FrameworkPropertyMetadataOptions.AffectsParentArrange, value);
        }

        public bool AffectsRender
        {
            get => Flags.HasFlag(FrameworkPropertyMetadataOptions.AffectsRender);
            set => SetFlag(FrameworkPropertyMetadataOptions.AffectsRender, value);
        }

        public bool Inherits
        {
            get => Flags.HasFlag(FrameworkPropertyMetadataOptions.Inherits);
            set => SetFlag(FrameworkPropertyMetadataOptions.Inherits, value);
        }

        public bool BindsTwoWayByDefault
        {
            get => Flags.HasFlag(FrameworkPropertyMetadataOptions.BindsTwoWayByDefault);
            set => SetFlag(FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, value);
        }

        private void SetFlag(FrameworkPropertyMetadataOptions flag, bool value)
        {
            if (value)
            {
                Flags |= flag;
            }
            else
            {
                Flags &= ~flag;
            }
        }
    }
}
