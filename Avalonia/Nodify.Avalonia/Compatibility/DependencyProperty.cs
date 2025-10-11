global using DependencyProperty = Avalonia.AvaloniaProperty;
using Avalonia;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Xml.Linq;

namespace Nodify.Compatibility
{
    public delegate void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e);

    [Flags]
    public enum FrameworkPropertyMetadataOptions
    {
        //
        // Summary:
        //     No options are specified; the dependency property uses the default behavior of
        //     the WPF property system.
        None = 0,
        //
        // Summary:
        //     The measure pass of layout compositions is affected by value changes to this
        //     dependency property.
        AffectsMeasure = 1,
        //
        // Summary:
        //     The arrange pass of layout composition is affected by value changes to this dependency
        //     property.
        AffectsArrange = 2,
        //
        // Summary:
        //     The measure pass on the parent element is affected by value changes to this dependency
        //     property.
        AffectsParentMeasure = 4,
        //
        // Summary:
        //     The arrange pass on the parent element is affected by value changes to this dependency
        //     property.
        AffectsParentArrange = 8,
        //
        // Summary:
        //     Some aspect of rendering or layout composition (other than measure or arrange)
        //     is affected by value changes to this dependency property.
        AffectsRender = 16,
        //
        // Summary:
        //     The values of this dependency property are inherited by child elements.
        Inherits = 32,
        //
        // Summary:
        //     The values of this dependency property span separated trees for purposes of property
        //     value inheritance.
        OverridesInheritanceBehavior = 64,
        //
        // Summary:
        //     Data binding to this dependency property is not allowed.
        NotDataBindable = 128,
        //
        // Summary:
        //     The System.Windows.Data.BindingMode for data bindings on this dependency property
        //     defaults to System.Windows.Data.BindingMode.TwoWay.
        BindsTwoWayByDefault = 256,
        //
        // Summary:
        //     The values of this dependency property should be saved or restored by journaling
        //     processes, or when navigating by Uniform resource identifiers (URIs).
        Journal = 1024,
        //
        // Summary:
        //     The subproperties on the value of this dependency property do not affect any
        //     aspect of rendering.
        SubPropertiesDoNotAffectRender = 2048
    }

    public class PropertyMetadata
    {
        public object DefaultValue { get; set; }
        public PropertyChangedCallback PropertyChangedCallback { get; set; }

        public PropertyMetadata()
        {
        }
        public PropertyMetadata(object defaultValue)
        {
            DefaultValue = defaultValue;
        }
        public PropertyMetadata(PropertyChangedCallback propertyChangedCallback)
        {
            PropertyChangedCallback = propertyChangedCallback;
        }
        public PropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback)
        {
            DefaultValue = defaultValue;
            PropertyChangedCallback = propertyChangedCallback;
        }
    }

    public class FrameworkPropertyMetadata : PropertyMetadata
    {
        public FrameworkPropertyMetadataOptions Options { get; set; }
        public FrameworkPropertyMetadata(object defaultValue) : base(defaultValue) { }
        public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions options) : base(defaultValue) { Options = options; }
        public FrameworkPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback) : base(defaultValue, propertyChangedCallback) { }
        public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions options, PropertyChangedCallback propertyChangedCallback) { Options = options; }
    }

    public static partial class AvaloniaPropertyExtension
    {
        // TODO:
        extension(DependencyProperty)
        {
            public static DependencyProperty Register(string name, Type propertyType, Type ownerType)
            {
                var registerMethod = typeof(AvaloniaProperty).GetMethod(nameof(Register), BindingFlags.Public | BindingFlags.Static);
                var genericRegisterMethod = registerMethod.MakeGenericMethod(ownerType, propertyType);
                return (DependencyProperty)genericRegisterMethod.Invoke(null, new object[] { name });
            }

            public static DependencyProperty Register(string name, Type propertyType, Type ownerType, FrameworkPropertyMetadata metadata)
            {
                var registerMethod = typeof(AvaloniaProperty).GetMethod(nameof(Register), BindingFlags.Public | BindingFlags.Static);
                var genericRegisterMethod = registerMethod.MakeGenericMethod(ownerType, propertyType);
                return (DependencyProperty)genericRegisterMethod.Invoke(null, new object[] { name, metadata.DefaultValue });
            }

            public static DependencyProperty RegisterAttached(string name, Type propertyType, Type ownerType, FrameworkPropertyMetadata metadata)
            {
                var registerMethod = typeof(AvaloniaProperty).GetMethod(nameof(RegisterAttached), BindingFlags.Public | BindingFlags.Static);
                var genericRegisterMethod = registerMethod.MakeGenericMethod(ownerType, typeof(Control), propertyType);
                return (DependencyProperty)genericRegisterMethod.Invoke(null, new object[] { name });
            }

            public static DependencyProperty AddOwner(Type ownerType)
            {
                var addOwnerMethod = typeof(StyledProperty<>).GetMethod(nameof(AddOwner), BindingFlags.Public | BindingFlags.Static);
                var genericAddOwnerMethod = addOwnerMethod.MakeGenericMethod(ownerType);
                return (DependencyProperty)genericAddOwnerMethod.Invoke(null, new object[] { });
            }

            public static DependencyProperty AddOwner(Type ownerType, FrameworkPropertyMetadata typeMetadata)
            {
                var addOwnerMethod = typeof(StyledProperty<>).GetMethod(nameof(AddOwner), BindingFlags.Public | BindingFlags.Static);
                var genericAddOwnerMethod = addOwnerMethod.MakeGenericMethod(ownerType);
                return (DependencyProperty)genericAddOwnerMethod.Invoke(null, new object[] { });
            }
        }
    }
}
