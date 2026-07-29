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
using AvItemsControl = Avalonia.Controls.ItemsControl;
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
            DependencyProperty.Register("DefaultStyleKey", typeof(object), typeof(Avalonia.Controls.Control), new FrameworkPropertyMetadata(null));

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
        /// <summary>The WPF items-host part name upstream items controls resolve from their template.</summary>
        public const string ItemsHostPartName = "PART_ItemsHost";

        /// <summary>Resolves a named template part from the last-applied name scope, mirroring WPF's <c>GetTemplateChild</c>.</summary>
        public static object? GetTemplateChild(INameScope? nameScope, string childName)
        {
            ArgumentException.ThrowIfNullOrEmpty(childName);
            return nameScope?.Find(childName);
        }

        /// <summary>
        /// Resolves a named template part for an items control, falling back to the realized items panel.
        /// </summary>
        /// <remarks>
        /// WPF marks the items host inline in the template (<c>IsItemsHost="True"</c>), so upstream resolves
        /// it by name. Avalonia instead realizes the panel from <c>ItemsPanel</c> inside an
        /// <see cref="ItemsPresenter"/>, so the panel is not a named part. When the name lookup misses and
        /// the requested part is the items host, this applies the presenter's template (the panel is
        /// otherwise created too late for the owner's <c>OnApplyTemplate</c>) and returns the realized panel.
        /// </remarks>
        public static object? GetTemplateChild(AvItemsControl owner, INameScope? nameScope, string childName)
        {
            var child = GetTemplateChild(nameScope, childName);
            if (child != null || childName != ItemsHostPartName)
            {
                return child;
            }

            owner.Presenter?.ApplyTemplate();
            return owner.ItemsPanelRoot;
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

        /// <summary>WPF hit-test-visible dependency property.</summary>
        public static new readonly DependencyProperty IsHitTestVisibleProperty =
            DependencyProperty.FromExisting(Avalonia.Input.InputElement.IsHitTestVisibleProperty, typeof(ContentControl));

        /// <summary>WPF enabled dependency property.</summary>
        public static new readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.FromExisting(Avalonia.Input.InputElement.IsEnabledProperty, typeof(ContentControl));

        /// <summary>WPF preview key-up routed event identity.</summary>
        public static readonly RoutedEvent PreviewKeyUpEvent = UIElement.PreviewKeyUpEvent;

        /// <summary>Record-only WPF focus visual style property.</summary>
        public static readonly DependencyProperty FocusVisualStyleProperty =
            DependencyProperty.Register(nameof(FocusVisualStyle), typeof(Style), typeof(ContentControl), new FrameworkPropertyMetadata(null));

        private INameScope? _templateNameScope;

        /// <summary>Initializes layout and loaded-event bridging.</summary>
        public ContentControl()
        {
            base.Loaded += (_, _) => Loaded?.Invoke(this, new RoutedEventArgs { Source = this });
            base.Unloaded += (_, _) => Unloaded?.Invoke(this, new RoutedEventArgs { Source = this });
            SizeChanged += (_, e) => OnRenderSizeChanged(new SizeChangedInfo(e.NewSize, e.PreviousSize));
        }

        /// <summary>Occurs when the control is attached to the visual tree.</summary>
        public new event RoutedEventHandler? Loaded;

        /// <summary>Occurs when the control is detached from the visual tree.</summary>
        public new event RoutedEventHandler? Unloaded;

        /// <summary>Gets the current rendered size.</summary>
        public Size RenderSize => Bounds.Size;

        /// <summary>Gets the rendered width.</summary>
        public double ActualWidth => Bounds.Width;

        /// <summary>Gets the rendered height.</summary>
        public double ActualHeight => Bounds.Height;

        /// <summary>Gets whether this control currently has keyboard focus.</summary>
        public bool IsKeyboardFocused => IsFocused;

        /// <summary>Gets or sets WPF visibility through Avalonia's IsVisible property.</summary>
        public Visibility Visibility
        {
            get => IsVisible ? Visibility.Visible : Visibility.Collapsed;
            set => IsVisible = value == Visibility.Visible;
        }

        /// <summary>Gets or sets the focus visual style carrier.</summary>
        public Style? FocusVisualStyle
        {
            get => (Style?)GetValue(FocusVisualStyleProperty);
            set => SetValue(FocusVisualStyleProperty, value);
        }

        /// <summary>Gets whether this control owns pointer capture.</summary>
        public bool IsMouseCaptured => global::System.Windows.Input.WpfInputBridge.IsMouseCaptured(this);

        /// <summary>Captures the current pointer.</summary>
        public bool CaptureMouse() => global::System.Windows.Input.WpfInputBridge.CaptureMouse(this);

        /// <summary>Releases pointer capture.</summary>
        public void ReleaseMouseCapture()
        {
            if (IsMouseCaptured)
            {
                global::System.Windows.Input.InputStateTracker.Pointer?.Capture(null);
            }
        }

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

        /// <inheritdoc />
        protected sealed override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _templateNameScope = e.NameScope;
            OnApplyTemplate();
        }

        /// <summary>WPF parameterless template-applied hook.</summary>
        public virtual void OnApplyTemplate()
        {
        }

        /// <summary>Resolves a named template child.</summary>
        protected object? GetTemplateChild(string childName)
            => WpfTemplateServices.GetTemplateChild(_templateNameScope, childName);

        /// <summary>WPF render-size-changed hook.</summary>
        protected virtual void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
        }

        /// <summary>WPF visual-parent-changed hook.</summary>
        protected virtual void OnVisualParentChanged(DependencyObject oldParent)
        {
        }

        /// <summary>WPF keyboard-focus-property-changed hook.</summary>
        protected virtual void OnIsKeyboardFocusedChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>WPF mouse-down hook.</summary>
        protected virtual void OnMouseDown(global::System.Windows.Input.MouseButtonEventArgs e) { }

        /// <summary>WPF mouse-up hook.</summary>
        protected virtual void OnMouseUp(global::System.Windows.Input.MouseButtonEventArgs e) { }

        /// <summary>WPF mouse-move hook.</summary>
        protected virtual void OnMouseMove(global::System.Windows.Input.MouseEventArgs e) { }

        /// <summary>WPF mouse-wheel hook.</summary>
        protected virtual void OnMouseWheel(global::System.Windows.Input.MouseWheelEventArgs e) { }

        /// <summary>WPF lost-mouse-capture hook.</summary>
        protected virtual void OnLostMouseCapture(global::System.Windows.Input.MouseEventArgs e) { }

        /// <summary>WPF key-down hook.</summary>
        protected virtual void OnKeyDown(global::System.Windows.Input.KeyEventArgs e) { }

        /// <summary>WPF key-up hook.</summary>
        protected virtual void OnKeyUp(global::System.Windows.Input.KeyEventArgs e) { }

        /// <inheritdoc />
        protected override void OnPointerPressed(Avalonia.Input.PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            var args = global::System.Windows.Input.WpfInputBridge.CreateMouseButtonEvent(this, e);
            OnMouseDown(args);
            global::System.Windows.Input.WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerReleased(Avalonia.Input.PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            var args = global::System.Windows.Input.WpfInputBridge.CreateMouseButtonEvent(this, e);
            OnMouseUp(args);
            global::System.Windows.Input.WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerMoved(Avalonia.Input.PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            var args = global::System.Windows.Input.WpfInputBridge.CreateMouseMoveEvent(this, e);
            OnMouseMove(args);
            global::System.Windows.Input.WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerWheelChanged(Avalonia.Input.PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            var args = global::System.Windows.Input.WpfInputBridge.CreateMouseWheelEvent(this, e);
            OnMouseWheel(args);
            global::System.Windows.Input.WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerCaptureLost(Avalonia.Input.PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            var args = global::System.Windows.Input.WpfInputBridge.CreateLostMouseCaptureEvent(this, e.Pointer, e.Source);
            OnLostMouseCapture(args);
            global::System.Windows.Input.WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            var args = global::System.Windows.Input.WpfInputBridge.CreateKeyEvent(e, true);
            OnKeyDown(args);
            global::System.Windows.Input.WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnKeyUp(Avalonia.Input.KeyEventArgs e)
        {
            base.OnKeyUp(e);
            var args = global::System.Windows.Input.WpfInputBridge.CreateKeyEvent(e, false);
            OnKeyUp(args);
            global::System.Windows.Input.WpfInputBridge.CopyHandled(args, e);
        }

        public Size DesiredSize => base.DesiredSize;

        public void Arrange(Rect finalRect) => base.Arrange(finalRect);
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

        /// <summary>WPF <c>Header</c> property, wrapping Avalonia's <see cref="AvHeaderedContentControl.HeaderProperty"/>.</summary>
        public static new readonly DependencyProperty HeaderProperty =
            DependencyProperty.FromExisting(AvHeaderedContentControl.HeaderProperty, typeof(HeaderedContentControl));

        /// <summary>WPF <c>HeaderTemplate</c> property, wrapping Avalonia's <see cref="AvHeaderedContentControl.HeaderTemplateProperty"/>.</summary>
        public static new readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.FromExisting(AvHeaderedContentControl.HeaderTemplateProperty, typeof(HeaderedContentControl));

        private INameScope? _templateNameScope;

        /// <summary>Initializes loaded-event bridging.</summary>
        public HeaderedContentControl()
        {
            base.Loaded += (_, _) => Loaded?.Invoke(this, new RoutedEventArgs { Source = this });
            base.Unloaded += (_, _) => Unloaded?.Invoke(this, new RoutedEventArgs { Source = this });
        }

        /// <summary>Occurs when the control is attached to the visual tree.</summary>
        public new event RoutedEventHandler? Loaded;

        /// <summary>Occurs when the control is detached from the visual tree.</summary>
        public new event RoutedEventHandler? Unloaded;

        /// <summary>Gets the current rendered size.</summary>
        public Size RenderSize => Bounds.Size;

        /// <summary>Gets the rendered width.</summary>
        public double ActualWidth => Bounds.Width;

        /// <summary>Gets the rendered height.</summary>
        public double ActualHeight => Bounds.Height;

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
    /// WPF-compatible <see cref="ContentPresenter"/>. It derives from the compatibility
    /// <see cref="ContentControl"/> so connection containers inherit the WPF input/template hooks;
    /// the content properties still wrap Avalonia's real content-control properties.
    /// </summary>
    public class ContentPresenter : ContentControl
    {
        /// <summary>WPF <c>Content</c> property, wrapping Avalonia's content property.</summary>
        public static new readonly DependencyProperty ContentProperty =
            DependencyProperty.FromExisting(AvContentControl.ContentProperty, typeof(ContentPresenter));

        /// <summary>WPF <c>ContentTemplate</c> property, wrapping Avalonia's content-template property.</summary>
        public static new readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.FromExisting(AvContentControl.ContentTemplateProperty, typeof(ContentPresenter));
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
