// -----------------------------------------------------------------------------
//  WPF hit-testing shims (System.Windows.Media)
// -----------------------------------------------------------------------------
//  Upstream Nodify's DependencyObjectExtensions performs callback-based hit
//  testing via WPF's VisualTreeHelper.HitTest overload:
//
//      VisualTreeHelper.HitTest(container,
//          filterCallback:  d  => HitTestFilterBehavior.*,
//          resultCallback:  hr => HitTestResultBehavior.*,
//          hitTestParameters: new PointHitTestParameters(p)      // or GeometryHitTestParameters
//      );
//
//  Avalonia has NO callback-driven hit-test model (only InputHitTest / GetVisualsAt),
//  so these types reproduce WPF's hit-test vocabulary. The actual traversal is
//  implemented as a hand-written recursive visual-tree walk in the shim
//  VisualTreeHelper.HitTest(...) overload (see VisualTreeHelper.cs).
//
//  Only the members Nodify consumes are provided.
// -----------------------------------------------------------------------------

using System;
using AvVisual = Avalonia.Visual;
using AvGeometry = Avalonia.Media.Geometry;

namespace System.Windows.Media
{
    /// <summary>Controls how the hit-test filter callback prunes the visual tree walk.</summary>
    public enum HitTestFilterBehavior
    {
        /// <summary>Skip the current visual and all of its children.</summary>
        ContinueSkipSelfAndChildren,

        /// <summary>Hit-test the current visual's children but not the visual itself.</summary>
        ContinueSkipSelf,

        /// <summary>Hit-test the current visual but not its children.</summary>
        ContinueSkipChildren,

        /// <summary>Hit-test the current visual and its children.</summary>
        Continue,

        /// <summary>Stop the hit-test walk entirely.</summary>
        Stop,
    }

    /// <summary>Controls whether the hit-test result callback continues or stops the walk.</summary>
    public enum HitTestResultBehavior
    {
        /// <summary>Stop the hit-test walk.</summary>
        Stop,

        /// <summary>Continue the hit-test walk.</summary>
        Continue,
    }

    /// <summary>Base class for hit-test results, exposing the visual that was hit.</summary>
    public class HitTestResult
    {
        /// <summary>Initializes a new instance for the given visual.</summary>
        /// <param name="visualHit">The visual that was hit.</param>
        public HitTestResult(AvVisual visualHit)
        {
            VisualHit = visualHit;
        }

        /// <summary>Gets the visual that was hit.</summary>
        public AvVisual VisualHit { get; }
    }

    /// <summary>Hit-test result produced by a <see cref="PointHitTestParameters"/> test.</summary>
    public sealed class PointHitTestResult : HitTestResult
    {
        /// <summary>Initializes a new instance for the given visual and point.</summary>
        /// <param name="visualHit">The visual that was hit.</param>
        /// <param name="pointHit">The point (in the visual's coordinate space) that was hit.</param>
        public PointHitTestResult(AvVisual visualHit, Point pointHit) : base(visualHit)
        {
            PointHit = pointHit;
        }

        /// <summary>Gets the point that was hit.</summary>
        public Point PointHit { get; }
    }

    /// <summary>Hit-test result produced by a <see cref="GeometryHitTestParameters"/> test.</summary>
    public sealed class GeometryHitTestResult : HitTestResult
    {
        /// <summary>Initializes a new instance for the given visual.</summary>
        /// <param name="visualHit">The visual that was hit.</param>
        public GeometryHitTestResult(AvVisual visualHit) : base(visualHit)
        {
        }
    }

    /// <summary>Base class for hit-test parameter objects.</summary>
    public abstract class HitTestParameters
    {
    }

    /// <summary>Parameters for a point-based hit test.</summary>
    public sealed class PointHitTestParameters : HitTestParameters
    {
        /// <summary>Initializes point hit-test parameters at the specified location.</summary>
        /// <param name="point">The hit-test point in the container's coordinate space.</param>
        public PointHitTestParameters(Point point)
        {
            HitPoint = point;
        }

        /// <summary>Gets the hit-test point.</summary>
        public Point HitPoint { get; }
    }

    /// <summary>Parameters for a geometry-based hit test.</summary>
    public sealed class GeometryHitTestParameters : HitTestParameters
    {
        /// <summary>Initializes geometry hit-test parameters for the specified geometry.</summary>
        /// <param name="geometry">The hit-test geometry in the container's coordinate space.</param>
        public GeometryHitTestParameters(AvGeometry geometry)
        {
            HitGeometry = geometry;
        }

        /// <summary>Gets the hit-test geometry.</summary>
        public AvGeometry HitGeometry { get; }
    }

    /// <summary>Callback invoked to decide whether a visual participates in a hit test.</summary>
    /// <param name="potentialHitTestTarget">The visual being considered.</param>
    /// <returns>The filtering behavior for the visual and its subtree.</returns>
    public delegate HitTestFilterBehavior HitTestFilterCallback(AvVisual potentialHitTestTarget);

    /// <summary>Callback invoked with each hit result during a hit test.</summary>
    /// <param name="result">The hit-test result.</param>
    /// <returns>Whether to continue or stop the hit-test walk.</returns>
    public delegate HitTestResultBehavior HitTestResultCallback(HitTestResult result);
}
