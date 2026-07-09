// -----------------------------------------------------------------------------
//  WPF VisualTreeHelper shim (System.Windows.Media)
// -----------------------------------------------------------------------------
//  Maps the visual-tree *navigation* members used across the ported Nodify code
//  onto Avalonia's visual tree. These map cleanly and let ported files keep the
//  original `VisualTreeHelper.GetParent/GetChild/GetChildrenCount` calls verbatim.
//
//  NOTE: WPF's callback-driven `VisualTreeHelper.HitTest(...)` (filter + result
//  callbacks + PointHitTestParameters/GeometryHitTestParameters) has NO direct
//  Avalonia equivalent. Code that used it (DependencyObjectExtensions) must be
//  ported to a hand-written tree walk / Avalonia hit-testing instead of relying
//  on a shim. See PORTING.md ("Hit testing").
// -----------------------------------------------------------------------------

using System.Linq;
using Avalonia;
using Avalonia.VisualTree;

namespace System.Windows.Media
{
    /// <summary>Avalonia-backed subset of WPF's <c>VisualTreeHelper</c> (navigation only).</summary>
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
    }
}
