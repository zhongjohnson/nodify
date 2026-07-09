// -----------------------------------------------------------------------------
//  WPF Shape shim (System.Windows.Shapes)
// -----------------------------------------------------------------------------
//  Upstream Nodify shapes (CuttingLine, BaseConnection, ...) derive from WPF's
//  `System.Windows.Shapes.Shape` and render by:
//    * overriding `protected override Geometry DefiningGeometry` to BUILD the path, and
//    * overriding `protected override void OnRender(DrawingContext)` and calling
//      `base.OnRender(drawingContext)` to draw the geometry, then drawing extra decorations
//      (arrowheads, ellipses, text, focus visuals) on top.
//
//  Avalonia's `Avalonia.Controls.Shapes.Shape` has a DIFFERENT shape than WPF's:
//    * `DefiningGeometry` is a NON-virtual property (a cache), so it CANNOT be overridden
//      (CS0506). The overridable build hook is instead `protected abstract Geometry
//      CreateDefiningGeometry()` (must be implemented -> CS0534).
//    * `Render(DrawingContext)` is SEALED on `Shape` (it draws the cached geometry with
//      Stroke/Fill), so it CANNOT be overridden (CS0239) and there is no `OnRender` hook.
//
//  This shim bridges both differences:
//    * It re-declares a WPF-style `protected virtual Geometry DefiningGeometry` with `new`
//      (shadowing Avalonia's non-virtual member) so upstream `protected override Geometry
//      DefiningGeometry` binds to THIS virtual and keeps building the path unchanged.
//    * It implements Avalonia's `CreateDefiningGeometry()` to return that WPF `DefiningGeometry`,
//      so Avalonia's sealed `Render` draws the built path with Stroke/Fill.
//    * It exposes a WPF-style `protected virtual void OnRender(DrawingContext)` hook that upstream
//      shapes override. Because Avalonia already draws the geometry via `CreateDefiningGeometry`,
//      the base `OnRender` is a no-op; runtime wiring of the EXTRA `OnRender` drawing (ellipses,
//      text, adorners) is deferred to the control/adorner phase along with the shapes that use it.
// -----------------------------------------------------------------------------

using System.Windows;
using AvShape = Avalonia.Controls.Shapes.Shape;

namespace System.Windows.Shapes
{
    /// <summary>
    /// WPF-compatible base for path-drawing shapes. Derives from Avalonia's
    /// <see cref="Avalonia.Controls.Shapes.Shape"/> and adapts the geometry build hook and render
    /// entry point so upstream shapes can override <c>DefiningGeometry</c> and <see cref="OnRender"/>
    /// unchanged.
    /// </summary>
    public abstract class Shape : AvShape
    {
        /// <summary>WPF-style value accessor. Shadows Avalonia's <c>GetValue(AvaloniaProperty)</c> so
        /// unqualified <c>GetValue(dp)</c> in upstream shapes binds the WPF <see cref="DependencyProperty"/>.</summary>
        public object? GetValue(DependencyProperty property)
            => DependencyPropertyServices.GetValue(this, property);

        /// <summary>WPF-style value setter for a <see cref="DependencyProperty"/>.</summary>
        public void SetValue(DependencyProperty property, object? value)
            => DependencyPropertyServices.SetValue(this, property, value);

        /// <summary>WPF-style value setter for a read-only <see cref="DependencyPropertyKey"/>.</summary>
        public void SetValue(DependencyPropertyKey key, object? value)
            => DependencyPropertyServices.SetValue(this, key, value);

        /// <summary>WPF-style local-value setter (no coercion re-entry) for a <see cref="DependencyProperty"/>.</summary>
        public void SetCurrentValue(DependencyProperty property, object? value)
            => DependencyPropertyServices.SetCurrentValue(this, property, value);

        /// <summary>WPF-style value clear for a <see cref="DependencyProperty"/>.</summary>
        public void ClearValue(DependencyProperty property)
            => DependencyPropertyServices.ClearValue(this, property);

        /// <summary>
        /// WPF-style buildable geometry. Shadows Avalonia's non-virtual <c>DefiningGeometry</c> so
        /// upstream shapes can <c>override</c> it to build their path. The base returns <see langword="null"/>.
        /// </summary>
        protected new virtual Geometry? DefiningGeometry => null;

        /// <summary>
        /// Avalonia's abstract geometry build hook, bridged to the WPF-style <see cref="DefiningGeometry"/>
        /// so Avalonia's sealed render draws the built path with Stroke/Fill.
        /// </summary>
        protected override Geometry? CreateDefiningGeometry() => DefiningGeometry;

        /// <summary>
        /// WPF-style render callback that upstream shapes override to draw decorations on top of the
        /// geometry. Avalonia already draws the geometry (via <see cref="CreateDefiningGeometry"/>), so the
        /// base is a no-op; wiring the extra drawing into a render pass is deferred to the control phase.
        /// </summary>
        /// <param name="drawingContext">The drawing context to render into.</param>
        protected virtual void OnRender(DrawingContext drawingContext)
        {
        }
    }
}

