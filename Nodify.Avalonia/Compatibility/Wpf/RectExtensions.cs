// -----------------------------------------------------------------------------
//  WPF Rect API shims (System.Windows)
// -----------------------------------------------------------------------------
//  `Rect` is aliased to `Avalonia.Rect` (see GlobalUsings.cs). Avalonia's Rect covers
//  most of WPF's surface but uses different member names for a few operations the
//  ported Nodify code relies on. This file bridges those gaps as extension methods so
//  the upstream sources keep their original calls:
//
//    WPF `rect.IntersectsWith(other)`  ->  Avalonia `rect.Intersects(other)`
//
//  Only the members Nodify actually uses are provided.
// -----------------------------------------------------------------------------

namespace System.Windows
{
    /// <summary>WPF-compatibility extensions over <see cref="Avalonia.Rect"/>.</summary>
    internal static class RectExtensions
    {
        /// <summary>
        /// WPF-named alias for <see cref="Avalonia.Rect.Intersects(Avalonia.Rect)"/>: returns true
        /// when the two rectangles overlap.
        /// </summary>
        public static bool IntersectsWith(this Avalonia.Rect self, Avalonia.Rect other)
            => self.Intersects(other);
    }
}
