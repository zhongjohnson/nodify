// -----------------------------------------------------------------------------
//  WPF authoring attribute shims (System.Windows)
// -----------------------------------------------------------------------------
//  Upstream Nodify controls annotate their template parts and style-typed properties
//  with WPF's design-time attributes (`[TemplatePart]`, `[StyleTypedProperty]`).
//  Avalonia has no equivalents, and these attributes carry no runtime behavior in
//  WPF either (they are metadata for designers/toolers), so they are provided here as
//  inert shims purely so the linked upstream sources compile unchanged.
// -----------------------------------------------------------------------------

namespace System.Windows
{
    /// <summary>
    /// Inert WPF-compatible <c>TemplatePart</c> attribute. Metadata only; no runtime behavior.
    /// Exists so upstream controls that declare their named template parts compile unchanged.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class TemplatePartAttribute : Attribute
    {
        /// <summary>Gets or sets the name of the template part.</summary>
        public string? Name { get; set; }

        /// <summary>Gets or sets the type of the template part.</summary>
        public Type? Type { get; set; }
    }

    /// <summary>
    /// Inert WPF-compatible <c>StyleTypedProperty</c> attribute. Metadata only; no runtime behavior.
    /// Exists so upstream controls that declare style-typed properties compile unchanged.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class StyleTypedPropertyAttribute : Attribute
    {
        /// <summary>Gets or sets the name of the style-typed property.</summary>
        public string? Property { get; set; }

        /// <summary>Gets or sets the target type the style applies to.</summary>
        public Type? StyleTargetType { get; set; }
    }
}
