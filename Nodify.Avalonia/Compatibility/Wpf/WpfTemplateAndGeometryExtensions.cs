// -----------------------------------------------------------------------------
//  WPF templating / geometry API projected onto Avalonia (C# 14 extension members)
// -----------------------------------------------------------------------------
//  Companion to WpfElementExtensions.cs. These cover the remaining WPF APIs the
//  linked Nodify sources call that live on types other than Control:
//    * ControlTemplate.FindName(name, owner) -> Avalonia template name-scope lookup
//    * Geometry widen/outline/combine helpers -> Avalonia geometry equivalents
// -----------------------------------------------------------------------------
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvControl = Avalonia.Controls.Control;
using AvGeometry = Avalonia.Media.Geometry;
using AvPen = Avalonia.Media.Pen;
using AvTransform = Avalonia.Media.Transform;

namespace Nodify.Avalonia.Compatibility
{
    /// <summary>
    /// WPF's <c>GeometryCombineMode</c>. Avalonia exposes the same set of boolean operations
    /// through <see cref="global::Avalonia.Media.CombinedGeometry"/>'s <c>GeometryCombineMode</c>,
    /// so the members map one-to-one.
    /// </summary>
    public enum GeometryCombineMode
    {
        /// <summary>The union of the two geometries.</summary>
        Union = 0,

        /// <summary>The intersection of the two geometries.</summary>
        Intersect = 1,

        /// <summary>The area covered by exactly one of the two geometries.</summary>
        Xor = 2,

        /// <summary>The first geometry minus the second.</summary>
        Exclude = 3
    }

    /// <summary>
    /// Projects the WPF templating and geometry API surface used by the linked Nodify sources
    /// onto their Avalonia counterparts.
    /// </summary>
    public static class WpfTemplateAndGeometryExtensions
    {
        /// <summary>Backing instance for the WPF-compatible <c>Transform.Identity</c>.</summary>
        private static readonly AvTransform _identity = new MatrixTransform(global::Avalonia.Matrix.Identity);

        extension(IControlTemplate template)
        {
            /// <summary>
            /// WPF's <c>ControlTemplate.FindName(name, templatedParent)</c>. Avalonia resolves named
            /// template parts through the templated parent's name scope rather than through the
            /// template object, so the lookup is delegated to the owner.
            /// </summary>
            /// <param name="name">The template part name.</param>
            /// <param name="templatedParent">The control the template was applied to.</param>
            public object? FindName(string name, AvControl templatedParent)
            {
                if (templatedParent is TemplatedControl templated
                    && templated.GetValue(WpfTemplateNameScope.NameScopeProperty) is INameScope scope
                    && scope.Find(name) is object part)
                {
                    return part;
                }

                // Fallback: the name scope is only captured for controls that route through the
                // compatibility layer's OnApplyTemplate. Otherwise search the visual tree, which is
                // where Avalonia places the realized template parts.
                foreach (var descendant in templatedParent.GetVisualDescendants())
                {
                    if (descendant is global::Avalonia.StyledElement styled && styled.Name == name)
                    {
                        return descendant;
                    }
                }

                return null;
            }
        }

        extension(AvGeometry geometry)
        {
            /// <summary>
            /// WPF's <c>Geometry.GetWidenedPathGeometry(Pen)</c>: the outline produced by stroking
            /// this geometry with the given pen. Avalonia's direct counterpart is
            /// <see cref="AvGeometry.GetWidenedGeometry(global::Avalonia.Media.IPen)"/>.
            /// </summary>
            /// <param name="pen">The pen whose thickness defines the widened outline.</param>
            public AvGeometry GetWidenedPathGeometry(AvPen pen)
                => geometry.GetWidenedGeometry(pen);

            /// <summary>
            /// WPF's <c>Geometry.GetOutlinedPathGeometry()</c>. Avalonia geometries are already
            /// usable as their own outline for hit-testing and stroking purposes, so the geometry
            /// is returned unchanged rather than flattened.
            /// </summary>
            public AvGeometry GetOutlinedPathGeometry()
                => geometry;
        }

        extension(AvGeometry)
        {
            /// <summary>
            /// WPF's static <c>Geometry.Combine</c>. Maps onto Avalonia's
            /// <see cref="CombinedGeometry"/>, which performs the same boolean operations.
            /// </summary>
            /// <param name="geometry1">The first geometry.</param>
            /// <param name="geometry2">The second geometry.</param>
            /// <param name="mode">The boolean operation to apply.</param>
            /// <param name="transform">An optional transform applied to the result.</param>
            public static AvGeometry Combine(
                AvGeometry geometry1,
                AvGeometry geometry2,
                GeometryCombineMode mode,
                AvTransform? transform)
                => new CombinedGeometry(
                    (global::Avalonia.Media.GeometryCombineMode)mode,
                    geometry1,
                    geometry2)
                {
                    Transform = transform
                };
        }

        extension(AvTransform)
        {
            /// <summary>
            /// WPF's <c>Transform.Identity</c>. Avalonia's equivalent is a zero-effect matrix
            /// transform; upstream only uses it as a "no transform" argument.
            /// </summary>
            public static AvTransform Identity => _identity;
        }
    }

    /// <summary>
    /// Carries the applied template's name scope on any Avalonia control so WPF-style
    /// <c>Template.FindName</c> lookups can resolve template parts.
    /// </summary>
    public static class WpfTemplateNameScope
    {
        /// <summary>The attached name scope captured when a template is applied.</summary>
        public static readonly global::Avalonia.AttachedProperty<INameScope?> NameScopeProperty =
            global::Avalonia.AvaloniaProperty.RegisterAttached<AvControl, INameScope?>(
                "WpfTemplateNameScope", typeof(WpfTemplateNameScope));

        static WpfTemplateNameScope()
        {
            // Capture every template application so FindName has a name scope to search.
            TemplatedControl.TemplateAppliedEvent.AddClassHandler<TemplatedControl>(
                (sender, e) => sender.SetValue(NameScopeProperty, e.NameScope));
        }

        /// <summary>Forces the static constructor to run so template applications are observed.</summary>
        public static void Initialize()
        {
        }
    }
}
