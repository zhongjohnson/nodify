// -----------------------------------------------------------------------------
//  Phase 4b rendering-shim smoke test (NOT a port; validation only)
// -----------------------------------------------------------------------------
//  The pure-render upstream shape we wanted as a smoke test — Nodify's CuttingLine —
//  turned out to be control-coupled: its IsOverElement attached property is
//  `PendingConnection.IsOverElementProperty.AddOwner(...)`, and PendingConnection is a
//  full Connector control that is deferred to the control phase. Linking CuttingLine now
//  would pull that deferred control forward and break the bottom-up port order.
//
//  Instead, this tiny local shape mirrors exactly the rendering surface CuttingLine uses,
//  so the geometry/shape/drawing shims are compiled and exercised in isolation:
//    * derives from the WPF `System.Windows.Shapes.Shape` shim,
//    * builds `DefiningGeometry` with the WPF `StreamGeometry` + `StreamGeometryContext`
//      shims (BeginFigure / LineTo, FillRule.EvenOdd),
//    * overrides the WPF-style `OnRender(DrawingContext)` and draws with
//      `DrawingContext.DrawEllipse(Fill, null, ...)` (Avalonia-native overload).
//
//  This validates that upstream pure-render shapes will compile verbatim against the shim
//  layer. It can be removed once a real pure-render consumer is linked.
// -----------------------------------------------------------------------------

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Nodify.Avalonia.Compatibility
{
    internal sealed class ShimValidationShape : Shape
    {
        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(nameof(StartPoint), typeof(Point), typeof(ShimValidationShape), new FrameworkPropertyMetadata(BoxValue.Point, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(nameof(EndPoint), typeof(Point), typeof(ShimValidationShape), new FrameworkPropertyMetadata(BoxValue.Point, FrameworkPropertyMetadataOptions.AffectsRender));

        public Point StartPoint
        {
            get => (Point)GetValue(StartPointProperty);
            set => SetValue(StartPointProperty, value);
        }

        public Point EndPoint
        {
            get => (Point)GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }

        private readonly StreamGeometry _geometry = new StreamGeometry
        {
            FillRule = FillRule.EvenOdd
        };

        protected override Geometry DefiningGeometry
        {
            get
            {
                using (StreamGeometryContext context = _geometry.Open())
                {
                    context.BeginFigure(StartPoint, false, false);
                    context.LineTo(EndPoint, true, true);
                }

                return _geometry;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            drawingContext.DrawEllipse(Fill, null, StartPoint, StrokeThickness * 1.2, StrokeThickness * 1.2);
            drawingContext.DrawEllipse(Fill, null, EndPoint, StrokeThickness * 1.2, StrokeThickness * 1.2);
        }
    }
}
