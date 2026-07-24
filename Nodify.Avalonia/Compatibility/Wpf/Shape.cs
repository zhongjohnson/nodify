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
using System.Windows.Media;
using AvGeometry = Avalonia.Media.Geometry;

namespace System.Windows.Shapes
{
    /// <summary>
    /// WPF-compatible base for path-drawing shapes. Derives from the compat
    /// <see cref="FrameworkElement"/> (mirroring WPF's <c>Shape : FrameworkElement</c>) so
    /// <c>BaseConnection</c> satisfies the interactivity constraint <c>TElement : FrameworkElement</c>,
    /// and adapts the geometry build hook and render entry point so upstream shapes override
    /// <c>DefiningGeometry</c> and <see cref="OnRender"/> unchanged.
    /// </summary>
    public abstract class Shape : FrameworkElement
    {
        /// <summary>WPF default style key dependency property.</summary>
        public static readonly DependencyProperty DefaultStyleKeyProperty =
            System.Windows.Controls.WpfControlServices.DefaultStyleKeyProperty;

        /// <summary>WPF hit-test-visible dependency property.</summary>
        public static new readonly DependencyProperty IsHitTestVisibleProperty =
            DependencyProperty.FromExisting(Avalonia.Input.InputElement.IsHitTestVisibleProperty, typeof(Shape));

        /// <summary>WPF enabled dependency property.</summary>
        public static new readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.FromExisting(Avalonia.Input.InputElement.IsEnabledProperty, typeof(Shape));

        /// <summary>WPF <c>Stroke</c> dependency property (brush used to draw the outline).</summary>
        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(nameof(Stroke), typeof(Avalonia.Media.IBrush), typeof(Shape), new FrameworkPropertyMetadata(null));

        /// <summary>WPF <c>Fill</c> dependency property (brush used to fill the interior).</summary>
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register(nameof(Fill), typeof(Avalonia.Media.IBrush), typeof(Shape), new FrameworkPropertyMetadata(null));

        /// <summary>WPF <c>StrokeThickness</c> dependency property.</summary>
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(Shape), new FrameworkPropertyMetadata(1d));

        /// <summary>WPF <c>StrokeDashArray</c> dependency property.</summary>
        public static readonly DependencyProperty StrokeDashArrayProperty =
            DependencyProperty.Register(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(Shape), new FrameworkPropertyMetadata(null));

        /// <summary>Gets or sets the brush used to draw the shape's outline.</summary>
        public Avalonia.Media.IBrush? Stroke
        {
            get => (Avalonia.Media.IBrush?)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        /// <summary>Gets or sets the brush used to fill the shape's interior.</summary>
        public Avalonia.Media.IBrush? Fill
        {
            get => (Avalonia.Media.IBrush?)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        /// <summary>Gets or sets the width of the shape's outline.</summary>
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty)!;
            set => SetValue(StrokeThicknessProperty, value);
        }

        /// <summary>Gets or sets the pattern of dashes and gaps used to draw the outline.</summary>
        public DoubleCollection? StrokeDashArray
        {
            get => (DoubleCollection?)GetValue(StrokeDashArrayProperty);
            set => SetValue(StrokeDashArrayProperty, value);
        }

        /// <summary>
        /// WPF-style buildable geometry upstream shapes override to build their path. The base
        /// returns <see langword="null"/>.
        /// </summary>
        protected virtual AvGeometry? DefiningGeometry => null;

        /// <summary>
        /// Bridges Avalonia's render pass to the WPF <see cref="OnRender"/> hook so upstream shapes
        /// render unchanged.
        /// </summary>
        public sealed override void Render(DrawingContext context) => OnRender(context);

        /// <summary>
        /// WPF-style render callback. The base draws the built <see cref="DefiningGeometry"/> with
        /// <see cref="Fill"/>/<see cref="Stroke"/>; upstream overrides draw decorations on top and call
        /// <c>base.OnRender</c>.
        /// </summary>
        /// <param name="drawingContext">The drawing context to render into.</param>
        protected virtual void OnRender(DrawingContext drawingContext)
        {
            AvGeometry? geometry = DefiningGeometry;
            if (geometry is null)
            {
                return;
            }

            Avalonia.Media.IPen? pen = Stroke is null
                ? null
                : new Avalonia.Media.Pen(Stroke, StrokeThickness);

            drawingContext.DrawGeometry(Fill, pen, geometry);
        }
    }
}

