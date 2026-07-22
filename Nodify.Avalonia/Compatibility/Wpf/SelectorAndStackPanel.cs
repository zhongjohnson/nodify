// -----------------------------------------------------------------------------
//  WPF selector / stack-panel primitive shims
// -----------------------------------------------------------------------------
//  The editor-core controls reuse a few WPF framework dependency-property statics via
//  AddOwner(...):
//    * ItemContainer:      Selector.IsSelectedProperty
//    * NodeInput/NodeOutput: StackPanel.OrientationProperty
//  Avalonia exposes the equivalent properties on different types (the attached
//  SelectingItemsControl.IsSelected and StackPanel.Orientation), so these shims re-expose
//  them as WPF-shaped DP statics wrapping the real Avalonia properties (via
//  DependencyProperty.FromExisting) so upstream AddOwner(...) works unchanged.
// -----------------------------------------------------------------------------

using System.Windows;
using AvSelectingItemsControl = Avalonia.Controls.Primitives.SelectingItemsControl;
using AvStackPanel = Avalonia.Controls.StackPanel;

namespace System.Windows.Controls.Primitives
{
    /// <summary>
    /// WPF-compatible <see cref="Selector"/> exposing the <c>IsSelected</c> attached dependency property.
    /// Upstream <c>ItemContainer</c> only references <c>Selector.IsSelectedProperty</c> via <c>AddOwner(...)</c>;
    /// this wraps Avalonia's real attached <see cref="AvSelectingItemsControl.IsSelectedProperty"/>.
    /// </summary>
    public static class Selector
    {
        /// <summary>WPF <c>IsSelected</c> attached property, wrapping Avalonia's <see cref="AvSelectingItemsControl.IsSelectedProperty"/>.</summary>
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.FromExisting(AvSelectingItemsControl.IsSelectedProperty, typeof(Selector));
    }
}

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
