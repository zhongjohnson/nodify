namespace System.Windows.Markup
{
    /// <summary>Inert WPF-compatible content-property metadata attribute.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class ContentPropertyAttribute : Attribute
    {
        /// <summary>Initializes the attribute with the content property name.</summary>
        public ContentPropertyAttribute(string name)
        {
            Name = name;
        }

        /// <summary>Gets the content property name.</summary>
        public string Name { get; }
    }
}

namespace System.Windows.Controls
{
    /// <summary>WPF-compatible data-template selector carrier.</summary>
    public abstract class DataTemplateSelector
    {
        /// <summary>Selects a template for an item and container.</summary>
        public virtual DataTemplate? SelectTemplate(object? item, DependencyObject? container) => null;
    }
}

namespace System.Windows.Shapes
{
    /// <summary>WPF-compatible rectangle shape.</summary>
    public class Rectangle : Shape
    {
        /// <inheritdoc />
        protected override Geometry? DefiningGeometry
            => new Avalonia.Media.RectangleGeometry(new Rect(Bounds.Size));
    }
}
