// -----------------------------------------------------------------------------
//  WPF VisualTreeHelper shim (System.Windows.Media)
// -----------------------------------------------------------------------------
//  Maps the visual-tree *navigation* members used across the ported Nodify code
//  onto Avalonia's visual tree. These map cleanly and let ported files keep the
//  original `VisualTreeHelper.GetParent/GetChild/GetChildrenCount` calls verbatim.
//
//  WPF's callback-driven `VisualTreeHelper.HitTest(...)` (filter + result callbacks
//  + PointHitTestParameters/GeometryHitTestParameters) has NO direct Avalonia
//  equivalent, so it is reimplemented here as a hand-written recursive visual-tree
//  walk. The traversal reproduces WPF's semantics closely enough for Nodify:
//    * children are visited front-to-back (reverse visual order = top-most first);
//    * the filter callback prunes self / children / subtree exactly as in WPF;
//    * a visual "hits" when the point is inside its bounds (point test) or its
//      bounds intersect the hit geometry's bounds (geometry test), evaluated in the
//      container's coordinate space;
//    * the result callback's Stop/Continue controls termination.
//  This granularity matches how Nodify uses hit testing (element-at-position and
//  cutting-line intersection over container-level bounds).
// -----------------------------------------------------------------------------

using System;
using System.Linq;
using Avalonia;
using Avalonia.VisualTree;

namespace System.Windows.Media
{
    /// <summary>Avalonia-backed subset of WPF's <c>VisualTreeHelper</c> (navigation + hit testing).</summary>
    public static class VisualTreeHelper
    {
        /// <summary>Returns the visual parent of <paramref name="reference"/>, or null.</summary>
        public static DependencyObject? GetParent(DependencyObject reference)
            => reference is Visual visual ? visual.GetVisualParent() : null;

        /// <summary>Returns the number of visual children of <paramref name="reference"/>.</summary>
        public static int GetChildrenCount(DependencyObject reference)
            => reference is Visual visual ? visual.GetVisualChildren().Count() : 0;

        /// <summary>Returns the visual child at <paramref name="childIndex"/>.</summary>
        public static DependencyObject GetChild(DependencyObject reference, int childIndex)
        {
            if (reference is Visual visual)
            {
                return visual.GetVisualChildren().ElementAt(childIndex);
            }

            throw new ArgumentOutOfRangeException(nameof(childIndex));
        }

        /// <summary>Gets the effective DPI scale for a visual.</summary>
        public static DpiScale GetDpi(Visual visual)
        {
            double scale = Avalonia.Controls.TopLevel.GetTopLevel(visual)?.RenderScaling ?? 1d;
            return new DpiScale(scale, scale);
        }

        /// <summary>
        /// WPF-compatible callback-based hit test. Walks the visual subtree rooted at
        /// <paramref name="reference"/>, invoking <paramref name="filterCallback"/> to prune the
        /// walk and <paramref name="resultCallback"/> for each visual that geometrically matches
        /// <paramref name="hitTestParameters"/>.
        /// </summary>
        public static void HitTest(
            Visual reference,
            HitTestFilterCallback? filterCallback,
            HitTestResultCallback resultCallback,
            HitTestParameters hitTestParameters)
        {
            if (resultCallback is null)
            {
                throw new ArgumentNullException(nameof(resultCallback));
            }

            HitTestVisual(reference, reference, filterCallback, resultCallback, hitTestParameters);
        }

        // Returns HitTestResultBehavior.Stop to abort the entire walk.
        private static HitTestResultBehavior HitTestVisual(
            Visual root,
            Visual current,
            HitTestFilterCallback? filterCallback,
            HitTestResultCallback resultCallback,
            HitTestParameters parameters)
        {
            HitTestFilterBehavior filter = filterCallback?.Invoke(current) ?? HitTestFilterBehavior.Continue;

            if (filter == HitTestFilterBehavior.Stop || filter == HitTestFilterBehavior.ContinueSkipSelfAndChildren)
            {
                return filter == HitTestFilterBehavior.Stop
                    ? HitTestResultBehavior.Stop
                    : HitTestResultBehavior.Continue;
            }

            bool testSelf = filter != HitTestFilterBehavior.ContinueSkipSelf;
            bool testChildren = filter != HitTestFilterBehavior.ContinueSkipChildren;

            // Visit children top-most first (reverse visual order), matching WPF's front-to-back walk.
            if (testChildren)
            {
                var children = current.GetVisualChildren().ToList();
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    if (children[i] is Visual child &&
                        HitTestVisual(root, child, filterCallback, resultCallback, parameters) == HitTestResultBehavior.Stop)
                    {
                        return HitTestResultBehavior.Stop;
                    }
                }
            }

            if (testSelf && current != root && Intersects(root, current, parameters, out Point localPoint))
            {
                HitTestResult result = parameters is GeometryHitTestParameters
                    ? new GeometryHitTestResult(current)
                    : new PointHitTestResult(current, localPoint);

                if (resultCallback(result) == HitTestResultBehavior.Stop)
                {
                    return HitTestResultBehavior.Stop;
                }
            }

            return HitTestResultBehavior.Continue;
        }

        // Tests the current visual's bounds (in root coordinates) against the hit-test parameters.
        private static bool Intersects(Visual root, Visual current, HitTestParameters parameters, out Point localPoint)
        {
            localPoint = default;

            Rect? boundsInRoot = GetBoundsInAncestor(current, root);
            if (boundsInRoot is not Rect bounds)
            {
                return false;
            }

            switch (parameters)
            {
                case PointHitTestParameters point:
                    if (bounds.Contains(point.HitPoint))
                    {
                        localPoint = point.HitPoint - new Point(bounds.X, bounds.Y);
                        return true;
                    }
                    return false;

                case GeometryHitTestParameters geometry:
                    Rect geometryBounds = geometry.HitGeometry.Bounds;
                    return bounds.IntersectsWith(geometryBounds);

                default:
                    return false;
            }
        }

        // Returns the bounds of <paramref name="visual"/> expressed in <paramref name="ancestor"/>'s coordinate space.
        private static Rect? GetBoundsInAncestor(Visual visual, Visual ancestor)
        {
            Point? topLeft = visual.TranslatePoint(default, ancestor);
            if (topLeft is not Point origin)
            {
                return null;
            }

            return new Rect(origin, visual.Bounds.Size);
        }
    }
}
