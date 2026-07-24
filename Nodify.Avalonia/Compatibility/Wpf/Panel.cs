// -----------------------------------------------------------------------------
//  WPF Panel shim (System.Windows.Controls)
// -----------------------------------------------------------------------------
//  Upstream Nodify panels (NodifyCanvas, MinimapPanel, and the editor's ItemsHost)
//  derive from WPF's `System.Windows.Controls.Panel` and:
//    * iterate their children through `UIElementCollection InternalChildren` (indexable,
//      with `.Count`), and
//    * use the `Panel.ZIndex` attached property (`GetZIndex`/`SetZIndex`) ¡ª e.g. GroupingNode
//      pushes its container behind siblings.
//
//  Avalonia's `Avalonia.Controls.Panel` already exposes an indexable `Children`
//  (`Avalonia.Controls.Controls`, aliased to `UIElementCollection` in GlobalUsings), and
//  Z-index is the `Avalonia.Visual.ZIndexProperty` attached property. This shim bridges the
//  WPF spelling onto those:
//    * `InternalChildren` returns Avalonia's `Children`.
//    * `GetZIndex`/`SetZIndex` map onto Avalonia's `Visual` Z-index (WPF uses `int`; Avalonia
//      too, so the mapping is direct).
//    * `ZIndexProperty` is re-exposed as a WPF `DependencyProperty` wrapping the real Avalonia
//      property so upstream `Panel.ZIndexProperty.OverrideMetadata(...)` works.
// -----------------------------------------------------------------------------

using System.Windows;
using Avalonia;
using AvPanel = Avalonia.Controls.Panel;
using AvVisual = Avalonia.Visual;

namespace System.Windows.Controls
{
    /// <summary>
    /// WPF-compatible <see cref="Panel"/> over Avalonia's <see cref="Avalonia.Controls.Panel"/>.
    /// Exposes the WPF <c>InternalChildren</c> spelling and the <c>Panel.ZIndex</c> attached property.
    /// </summary>
    public class Panel : AvPanel
    {
        /// <summary>Gets or sets the WPF cache-mode carrier.</summary>
        public object? CacheMode { get; set; }
        /// <summary>WPF <c>ZIndex</c> attached property, wrapping Avalonia's <see cref="AvVisual.ZIndexProperty"/>.</summary>
        public static readonly DependencyProperty ZIndexProperty =
            DependencyProperty.FromExisting(AvVisual.ZIndexProperty, typeof(Panel));

        /// <summary>WPF-style access to the panel's children (maps onto Avalonia's <c>Children</c>).</summary>
        protected UIElementCollection InternalChildren => Children;

        /// <summary>Gets the WPF <c>Panel.ZIndex</c> of an element (Avalonia <c>Visual.ZIndex</c>).</summary>
        public static int GetZIndex(AvaloniaObject element)
            => element.GetValue(AvVisual.ZIndexProperty);

        /// <summary>Sets the WPF <c>Panel.ZIndex</c> of an element (Avalonia <c>Visual.ZIndex</c>).</summary>
        public static void SetZIndex(AvaloniaObject element, int value)
            => element.SetValue(AvVisual.ZIndexProperty, value);

        /// <summary>WPF-style value accessor. Shadows Avalonia's <c>GetValue(AvaloniaProperty)</c>.</summary>
        public object? GetValue(DependencyProperty property)
            => DependencyPropertyServices.GetValue(this, property);

        /// <summary>WPF-style value setter for a <see cref="DependencyProperty"/>.</summary>
        public void SetValue(DependencyProperty property, object? value)
            => DependencyPropertyServices.SetValue(this, property, value);

        /// <summary>WPF-style value setter for a read-only <see cref="DependencyPropertyKey"/>.</summary>
        public void SetValue(DependencyPropertyKey key, object? value)
            => DependencyPropertyServices.SetValue(this, key, value);

        /// <summary>WPF-style local-value setter (no coercion re-entry).</summary>
        public void SetCurrentValue(DependencyProperty property, object? value)
            => DependencyPropertyServices.SetCurrentValue(this, property, value);

        /// <summary>WPF-style value clear.</summary>
        public void ClearValue(DependencyProperty property)
            => DependencyPropertyServices.ClearValue(this, property);

        /// <inheritdoc />
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            DependencyPropertyServices.OnPropertyChanged(this, change);
        }
    }
}
