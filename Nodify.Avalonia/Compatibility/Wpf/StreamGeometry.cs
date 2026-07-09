// -----------------------------------------------------------------------------
//  WPF stream-geometry shims (System.Windows.Media)
// -----------------------------------------------------------------------------
//  Upstream Nodify shapes build their `DefiningGeometry` with WPF's retained
//  `StreamGeometry` + `StreamGeometryContext`:
//
//      private readonly StreamGeometry _geometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
//      protected override Geometry DefiningGeometry
//      {
//          get
//          {
//              using (StreamGeometryContext context = _geometry.Open())
//              {
//                  context.BeginFigure(start, isFilled: false, isClosed: false);
//                  context.LineTo(p, isStroked: true, isSmoothJoin: true);
//                  context.BezierTo(c1, c2, end, true, true);
//                  context.QuadraticBezierTo(c, end, true, true);
//              }
//              return _geometry;
//          }
//      }
//
//  Avalonia's `StreamGeometryContext` has a DIFFERENT shape:
//    * `BeginFigure(Point, bool isFilled)` — the WPF `isClosed` flag is instead applied
//      via a separate `EndFigure(bool isClosed)` call, which must happen before the next
//      `BeginFigure` (or before `Dispose`).
//    * `LineTo(Point[, bool isStroked])` — no WPF `isSmoothJoin`.
//    * `CubicBezierTo(cp1, cp2, end)` — WPF calls this `BezierTo`.
//    * `QuadraticBezierTo(cp, end)`.
//
//  This shim:
//    * `System.Windows.Media.StreamGeometry` derives from `Avalonia.Media.StreamGeometry`
//      (so a shape's `DefiningGeometry` can return it directly where the return type is the
//      `Geometry = Avalonia.Media.Geometry` alias) and hides `Open()` to hand back the
//      WPF-shaped context wrapper.
//    * `StreamGeometryContext` wraps Avalonia's context and re-exposes the WPF method
//      signatures, tracking the current figure's pending `isClosed` so it can emit
//      `EndFigure(...)` at the right time.
// -----------------------------------------------------------------------------

using System;
using AvStreamGeometry = Avalonia.Media.StreamGeometry;
using AvStreamGeometryContext = Avalonia.Media.StreamGeometryContext;

namespace System.Windows.Media
{
    /// <summary>WPF-compatible fill rule (maps to <see cref="Avalonia.Media.FillRule"/>).</summary>
    public enum FillRule
    {
        /// <summary>Even-odd fill rule.</summary>
        EvenOdd = 0,

        /// <summary>Nonzero winding fill rule.</summary>
        Nonzero = 1
    }

    /// <summary>WPF-compatible arc sweep direction (maps to <see cref="Avalonia.Media.SweepDirection"/>).</summary>
    public enum SweepDirection
    {
        /// <summary>Counterclockwise sweep.</summary>
        Counterclockwise = 0,

        /// <summary>Clockwise sweep.</summary>
        Clockwise = 1
    }

    /// <summary>
    /// WPF-compatible <see cref="Avalonia.Media.StreamGeometry"/>: adds the WPF-style
    /// <see cref="FillRule"/> property and an <see cref="Open"/> that returns the WPF-shaped
    /// <see cref="StreamGeometryContext"/> wrapper.
    /// </summary>
    public class StreamGeometry : AvStreamGeometry
    {
        /// <summary>
        /// Gets or sets the fill rule. Mapped onto the fill rule the wrapping context applies to
        /// the first figure it begins (Avalonia sets the fill rule through the context).
        /// </summary>
        public FillRule FillRule { get; set; } = FillRule.EvenOdd;

        /// <summary>
        /// Opens the geometry for population and returns a WPF-shaped context. The returned context
        /// applies <see cref="FillRule"/> and must be disposed (upstream uses a <c>using</c> block).
        /// </summary>
        public new StreamGeometryContext Open()
        {
            AvStreamGeometryContext inner = base.Open();
            return new StreamGeometryContext(inner, FillRule);
        }
    }

    /// <summary>
    /// WPF-compatible wrapper over <see cref="Avalonia.Media.StreamGeometryContext"/>. Re-exposes the
    /// WPF figure/segment method signatures and defers Avalonia's required <c>EndFigure</c> call until
    /// the next figure begins or the context is disposed.
    /// </summary>
    public sealed class StreamGeometryContext : IDisposable
    {
        private readonly AvStreamGeometryContext _inner;
        private bool _figureOpen;
        private bool _pendingClosed;

        internal StreamGeometryContext(AvStreamGeometryContext inner, FillRule fillRule)
        {
            _inner = inner;
            _inner.SetFillRule(fillRule == FillRule.Nonzero ? Avalonia.Media.FillRule.NonZero : Avalonia.Media.FillRule.EvenOdd);
        }

        /// <summary>Begins a new figure. WPF's <paramref name="isClosed"/> is applied on figure end.</summary>
        /// <param name="startPoint">The figure's start point.</param>
        /// <param name="isFilled">Whether the figure is filled.</param>
        /// <param name="isClosed">Whether the figure is closed (applied when the figure ends).</param>
        public void BeginFigure(Point startPoint, bool isFilled, bool isClosed)
        {
            EndCurrentFigure();

            _inner.BeginFigure(startPoint, isFilled);
            _figureOpen = true;
            _pendingClosed = isClosed;
        }

        /// <summary>Draws a line segment. WPF's <c>isSmoothJoin</c> has no Avalonia analogue and is ignored.</summary>
        public void LineTo(Point point, bool isStroked, bool isSmoothJoin)
        {
            _inner.LineTo(point, isStroked);
        }

        /// <summary>Draws a cubic bezier segment (WPF's <c>BezierTo</c>).</summary>
        public void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
        {
            _inner.CubicBezierTo(point1, point2, point3, isStroked);
        }

        /// <summary>Draws a quadratic bezier segment.</summary>
        public void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin)
        {
            _inner.QuadraticBezierTo(point1, point2, isStroked);
        }

        /// <summary>Draws an elliptical arc segment.</summary>
        public void ArcTo(Point point, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, bool isStroked, bool isSmoothJoin)
        {
            var direction = sweepDirection == SweepDirection.Clockwise
                ? Avalonia.Media.SweepDirection.Clockwise
                : Avalonia.Media.SweepDirection.CounterClockwise;
            _inner.ArcTo(point, size, rotationAngle, isLargeArc, direction, isStroked);
        }

        private void EndCurrentFigure()
        {
            if (_figureOpen)
            {
                _inner.EndFigure(_pendingClosed);
                _figureOpen = false;
            }
        }

        /// <summary>Closes any open figure and disposes the underlying Avalonia context.</summary>
        public void Dispose()
        {
            EndCurrentFigure();
            _inner.Dispose();
        }
    }
}
