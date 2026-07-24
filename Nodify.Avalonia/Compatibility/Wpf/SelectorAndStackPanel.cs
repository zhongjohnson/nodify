// -----------------------------------------------------------------------------
//  WPF stack-panel primitive shim
// -----------------------------------------------------------------------------
//  Upstream NodeInput/NodeOutput reference StackPanel.OrientationProperty via AddOwner(...).
//  Avalonia exposes the equivalent on Avalonia.Controls.StackPanel, so this re-exposes it as a
//  WPF-shaped DP static wrapping the real Avalonia property (via DependencyProperty.FromExisting)
//  so upstream AddOwner(...) works unchanged.
//
//  NOTE: the WPF Selector/MultiSelector control bases and the Selector.IsSelected attached
//  property live in Compatibility\Wpf\Selection.cs (Phase 8a).
// -----------------------------------------------------------------------------

using System.Windows;
using AvStackPanel = Avalonia.Controls.StackPanel;

namespace System.Windows.Controls
{
    /// <summary>
    /// WPF-compatible <see cref="StackPanel"/> exposing the <c>Orientation</c> dependency property.
    /// Upstream <c>NodeInput</c>/<c>NodeOutput</c> only reference <c>StackPanel.OrientationProperty</c> via
    /// <c>AddOwner(...)</c>; this wraps Avalonia's real <see cref="AvStackPanel.OrientationProperty"/>.
    /// </summary>
    public static class StackPanel
    {
        /// <summary>WPF <c>Orientation</c> property, wrapping Avalonia's <see cref="AvStackPanel.OrientationProperty"/>.</summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.FromExisting(AvStackPanel.OrientationProperty, typeof(StackPanel));
    }
}
