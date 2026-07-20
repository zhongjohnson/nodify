// -----------------------------------------------------------------------------
//  WPF content-control shims (System.Windows.Controls)
// -----------------------------------------------------------------------------
//  Upstream Nodify controls derive from WPF's templated controls
//  (`ContentControl`, `HeaderedContentControl`, ...) and, in their static ctors,
//  call the WPF idioms:
//
//      DefaultStyleKeyProperty.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(typeof(T)));
//      FocusableProperty.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(BoxValue.False));
//
//  Avalonia's control hierarchy is DIFFERENT from the WPF `UIElement`/`FrameworkElement`
//  shim (which derives from `Avalonia.Controls.Control`), so these WPF control bases must
//  derive directly from their Avalonia counterparts. That means the WPF statics/value
//  accessors cannot be inherited from `System.Windows.FrameworkElement`; each control base
//  re-exposes them here (kept tiny by delegating to shared services):
//
//    * `DefaultStyleKeyProperty` ¡ª a RECORD-ONLY WPF dependency property. Avalonia resolves
//      control themes by type, not by a style key, so overriding it is a no-op today; the
//      registration is retained so upstream static ctors compile and so the intended default
//      style key is captured for the theme phase.
//    * `FocusableProperty` ¡ª wraps Avalonia's real `InputElement.FocusableProperty`, so
//      `OverrideMetadata(type, new FrameworkPropertyMetadata(false))` actually flips the
//      Avalonia `Focusable` default for that control type.
//    * WPF value accessors (`GetValue`/`SetValue`/`SetCurrentValue`/`ClearValue`) and the
//      `OnPropertyChanged` bridge that runs WPF coercion + change callbacks.
// -----------------------------------------------------------------------------

using System.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvContentControl = Avalonia.Controls.ContentControl;
using AvHeaderedContentControl = Avalonia.Controls.Primitives.HeaderedContentControl;
using AvInputElement = Avalonia.Input.InputElement;
using AvContentPresenter = Avalonia.Controls.Presenters.ContentPresenter;
using AvBorder = Avalonia.Controls.Border;

namespace System.Windows.Controls
{
    /// <summary>
    /// Shared WPF-control statics reused by every WPF control base (they cannot be inherited
    /// because the bases derive from different Avalonia control types).
    /// </summary>
    internal static class WpfControlServices
    {
        /// <summary>
        /// Record-only WPF <c>DefaultStyleKey</c> dependency property. Avalonia themes by type, so
        /// this has no runtime effect; it exists so upstream <c>DefaultStyleKeyProperty.OverrideMetadata</c>
        /// calls compile and so the intended style key is captured for the theme phase.
        /// </summary>
        public static readonly DependencyProperty DefaultStyleKeyProperty =
            DependencyProperty.Register("DefaultStyleKey", typeof(object), typeof(WpfControlServices), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// WPF <c>Focusable</c> dependency property, wrapping Avalonia's real
        /// <see cref="AvInputElement.FocusableProperty"/> so metadata overrides affect focus behavior.
        /// </summary>
        public static readonly DependencyProperty FocusableProperty =
            DependencyProperty.FromExisting(AvInputElement.FocusableProperty, typeof(WpfControlServices));
    }

    /// <summary>
    /// Bridges WPF's <c>GetTemplateChild(string)</c> / no-arg <c>OnApplyTemplate()</c> template idiom onto
    /// Avalonia's <see cref="TemplateAppliedEventArgs"/>-based <c>OnApplyTemplate(TemplateAppliedEventArgs)</c>.
    /// Upstream controls (e.g. <c>Node</c>) override the WPF <c>OnApplyTemplate()</c> and resolve named template
    /// parts via <c>GetTemplateChild(name)</c>; this captures the applied <see cref="INameScope"/> so those
    /// lookups resolve against the real Avalonia template.
    /// </summary>
    internal static class WpfTemplateServices
    {
        /// <summary>Resolves a named template part from the last-applied name scope, mirroring WPF's <c>GetTemplateChild</c>.</summary>
        public static object? GetTemplateChild(INameScope? nameScope, string childName)
        {
            ArgumentException.ThrowIfNullOrEmpty(childName);
            return nameScope?.Find(childName);
        }
    }

    /// <summary>
    /// WPF-compatible <see cref="ContentControl"/> over Avalonia's
    /// <see cref="Avalonia.Controls.ContentControl"/>. Exposes the WPF control statics and value
    /// accessors so upstream controls that derive from it compile and behave as expected.
    /// </summary>
    public class ContentControl : AvContentControl
    {
        /// <summary>Record-only WPF default-style-key property (theming deferred; see <see cref="WpfControlServices"/>).</summary>
        public static readonly DependencyProperty DefaultStyleKeyProperty = WpfControlServices.DefaultStyleKeyProperty;

        /// <summary>WPF focusable property wrapping Avalonia's real <c>Focusable</c>.</summary>
        public static new readonly DependencyProperty FocusableProperty = WpfControlServices.FocusableProperty;

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

    /// <summary>
    /// WPF-compatible <see cref="HeaderedContentControl"/> over Avalonia's
    /// <see cref="Avalonia.Controls.Primitives.HeaderedContentControl"/>.
    /// </summary>
    public class HeaderedContentControl : AvHeaderedContentControl
    {
        /// <summary>Record-only WPF default-style-key property (theming deferred; see <see cref="WpfControlServices"/>).</summary>
        public static readonly DependencyProperty DefaultStyleKeyProperty = WpfControlServices.DefaultStyleKeyProperty;

        /// <summary>WPF focusable property wrapping Avalonia's real <c>Focusable</c>.</summary>
        public static new readonly DependencyProperty FocusableProperty = WpfControlServices.FocusableProperty;

        private INameScope? _templateNameScope;

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

        /// <summary>
        /// Captures the applied template's name scope so WPF-style <see cref="GetTemplateChild"/> lookups
        /// resolve, then invokes the WPF-style parameterless <see cref="OnApplyTemplate()"/> override.
        /// </summary>
        protected sealed override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _templateNameScope = e.NameScope;
            OnApplyTemplate();
        }

        /// <summary>WPF-style template-applied hook. Override this to resolve named parts via <see cref="GetTemplateChild"/>.</summary>
        public virtual void OnApplyTemplate()
        {
        }

        /// <summary>Resolves a named template part, mirroring WPF's <c>FrameworkElement.GetTemplateChild</c>.</summary>
        protected object? GetTemplateChild(string childName)
            => WpfTemplateServices.GetTemplateChild(_templateNameScope, childName);
    }

    /// <summary>
    /// WPF-compatible <see cref="ContentPresenter"/> over Avalonia's
    /// <see cref="Avalonia.Controls.Presenters.ContentPresenter"/>. Upstream controls only reference
    /// its dependency-property statics via <c>AddOwner(...)</c> (e.g. <c>StateNode</c> reuses
    /// <c>ContentPresenter.ContentProperty</c>/<c>ContentTemplateProperty</c>); those statics wrap the
    /// real Avalonia properties so binding/templating behave normally.
    /// </summary>
    public class ContentPresenter : AvContentPresenter
    {
        /// <summary>WPF <c>Content</c> property, wrapping Avalonia's <see cref="AvContentPresenter.ContentProperty"/>.</summary>
        public static new readonly DependencyProperty ContentProperty =
            DependencyProperty.FromExisting(AvContentPresenter.ContentProperty, typeof(ContentPresenter));

        /// <summary>WPF <c>ContentTemplate</c> property, wrapping Avalonia's <see cref="AvContentPresenter.ContentTemplateProperty"/>.</summary>
        public static new readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.FromExisting(AvContentPresenter.ContentTemplateProperty, typeof(ContentPresenter));
    }

    /// <summary>
    /// WPF-compatible <see cref="Border"/> over Avalonia's <see cref="Avalonia.Controls.Border"/>.
    /// Upstream controls only reference <c>Border.CornerRadiusProperty</c> via <c>AddOwner(...)</c>
    /// (e.g. <c>StateNode</c>), so the static wraps the real Avalonia property.
    /// </summary>
    public class Border : AvBorder
    {
        /// <summary>WPF <c>CornerRadius</c> property, wrapping Avalonia's <see cref="AvBorder.CornerRadiusProperty"/>.</summary>
        public static new readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.FromExisting(AvBorder.CornerRadiusProperty, typeof(Border));
    }
}
