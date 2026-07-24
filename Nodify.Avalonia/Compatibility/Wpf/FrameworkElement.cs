// -----------------------------------------------------------------------------
//  WPF framework-element / UIElement shims (System.Windows)
// -----------------------------------------------------------------------------
//  The Nodify interactivity primitives are generic over `TElement : FrameworkElement`
//  and dispatch input by comparing `e.RoutedEvent` against WPF's UIElement input events:
//
//      if (e.RoutedEvent == UIElement.MouseMoveEvent) OnMouseMove(...);
//      else if (e.RoutedEvent == UIElement.KeyDownEvent) OnKeyDown(...);
//      ...
//
//  These shims provide:
//    * `UIElement` / `FrameworkElement` deriving from Avalonia `Control`, so upstream
//      generic constraints (`where TElement : FrameworkElement`) resolve and future control
//      ports can derive from them.
//    * Stable WPF-shaped input `RoutedEvent` statics on `UIElement`. They only need identity:
//      the ported control overrides (later phases) stamp the shim input args with these events,
//      and the state machine dispatches by reference-equality. Where a real Avalonia routed
//      event is the natural counterpart (key-up / lost-focus for class handlers), the shim also
//      records the mapping so `EventManager.RegisterClassHandler` can bridge to it.
//    * `FocusNavigationDirection`, `KeyboardFocusChangedEventArgs` and its handler, used by
//      keyboard-navigation code and `KeyComboGesture`.
//
//  RULE (see GlobalUsings.cs): names provided as `global using` aliases (e.g. `IInputElement`)
//  are NOT also declared as shim types here.
// -----------------------------------------------------------------------------

// Upstream references IInputElement as a bare name; it needs no WPF-specific surface, so it
// maps straight onto Avalonia's interface.
global using IInputElement = Avalonia.Input.IInputElement;

using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace System.Windows
{
    /// <summary>WPF-compatible visibility states.</summary>
    public enum Visibility
    {
        /// <summary>The element is visible.</summary>
        Visible,

        /// <summary>The element is hidden but retains layout space.</summary>
        Hidden,

        /// <summary>The element is hidden and excluded from layout.</summary>
        Collapsed
    }

    /// <summary>WPF-compatible DPI scale value.</summary>
    public readonly struct DpiScale
    {
        /// <summary>Initializes a DPI scale.</summary>
        public DpiScale(double dpiScaleX, double dpiScaleY)
        {
            DpiScaleX = dpiScaleX;
            DpiScaleY = dpiScaleY;
        }

        /// <summary>Gets the horizontal scale.</summary>
        public double DpiScaleX { get; }

        /// <summary>Gets the vertical scale.</summary>
        public double DpiScaleY { get; }

        /// <summary>Gets pixels per device-independent pixel.</summary>
        public double PixelsPerDip => DpiScaleY;
    }

    /// <summary>
    /// WPF-compatible <c>SizeChangedInfo</c>. Carries the new/previous render size for the
    /// <see cref="UIElement.OnRenderSizeChanged"/> override, mirroring WPF's type. Only the members
    /// used by upstream controls (<c>NewSize</c>/<c>PreviousSize</c>) are provided.
    /// </summary>
    public class SizeChangedInfo
    {
        /// <summary>Initializes a new instance of the <see cref="SizeChangedInfo"/> class.</summary>
        public SizeChangedInfo(Size newSize, Size previousSize)
        {
            NewSize = newSize;
            PreviousSize = previousSize;
        }

        /// <summary>Gets the new render size of the element.</summary>
        public Size NewSize { get; }

        /// <summary>Gets the previous render size of the element.</summary>
        public Size PreviousSize { get; }
    }

    /// <summary>
    /// WPF-compatible base for input-aware elements. Derives from Avalonia's
    /// <see cref="Control"/> so ported controls can inherit it, and hosts the WPF-shaped
    /// input <see cref="RoutedEvent"/> identities used by the interactivity state machine.
    /// </summary>
    public class UIElement : Control
    {
        /// <summary>Bridges Avalonia's <see cref="Control.SizeChanged"/> to the WPF
        /// <see cref="OnRenderSizeChanged"/> override so upstream layout code runs unchanged.</summary>
        public UIElement()
        {
            SizeChanged += OnSizeChangedBridge;
        }

        private void OnSizeChangedBridge(object? sender, global::Avalonia.Controls.SizeChangedEventArgs e)
            => OnRenderSizeChanged(new SizeChangedInfo(e.NewSize, e.PreviousSize));

        /// <summary>WPF-compatible render size. Maps onto Avalonia's <see cref="Visual.Bounds"/> size.</summary>
        public Size RenderSize => Bounds.Size;

        /// <summary>Gets the rendered width.</summary>
        public double ActualWidth => Bounds.Width;

        /// <summary>Gets the rendered height.</summary>
        public double ActualHeight => Bounds.Height;

        /// <summary>Gets whether this element currently has keyboard focus.</summary>
        public bool IsKeyboardFocused => IsFocused;

        /// <summary>Occurs when a mouse button is pressed over this element.</summary>
        public event System.Windows.Input.MouseButtonEventHandler? MouseDown;

        /// <summary>
        /// WPF-compatible <c>OnRenderSizeChanged</c> hook. Invoked from Avalonia's <c>SizeChanged</c>
        /// event; override to react to size changes. The base implementation does nothing.
        /// </summary>
        protected virtual void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
        }

        /// <summary>
        /// WPF-compatible <c>TranslatePoint</c>. Transforms <paramref name="point"/> from this element's
        /// coordinate space into <paramref name="relativeTo"/>'s, mirroring WPF's method (returns the
        /// zero point if the elements are not connected in the visual tree).
        /// </summary>
        public Point TranslatePoint(Point point, Visual relativeTo)
            => global::Avalonia.VisualExtensions.TranslatePoint(this, point, relativeTo) ?? default;

        /// <summary>
        /// WPF-compatible <c>IsAncestorOf</c>. Returns whether this element is a visual ancestor of
        /// <paramref name="descendant"/>.
        /// </summary>
        public bool IsAncestorOf(Visual descendant)
            => descendant != null && this.IsVisualAncestorOf(descendant);

        /// <summary>Gets whether this element currently has the captured pointer (WPF <c>IsMouseCaptured</c>).</summary>
        public bool IsMouseCaptured
            => ReferenceEquals(InputStateTracker.Pointer?.Captured, this);

        /// <summary>Captures the current pointer to this element (WPF <c>CaptureMouse</c>).</summary>
        /// <returns>True if capture was acquired; otherwise false.</returns>
        public bool CaptureMouse()
        {
            InputStateTracker.Pointer?.Capture(this);
            return IsMouseCaptured;
        }

        /// <summary>Releases the pointer capture held by this element (WPF <c>ReleaseMouseCapture</c>).</summary>
        public void ReleaseMouseCapture()
        {
            if (IsMouseCaptured)
            {
                InputStateTracker.Pointer?.Capture(null);
            }
        }

        /// <summary>WPF mouse-down hook.</summary>
        protected virtual void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        /// <summary>WPF mouse-up hook.</summary>
        protected virtual void OnMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        /// <summary>WPF mouse-move hook.</summary>
        protected virtual void OnMouseMove(System.Windows.Input.MouseEventArgs e)
        {
        }

        /// <summary>WPF mouse-wheel hook.</summary>
        protected virtual void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
        {
        }

        /// <summary>WPF lost-mouse-capture hook.</summary>
        protected virtual void OnLostMouseCapture(System.Windows.Input.MouseEventArgs e)
        {
        }

        /// <summary>WPF key-down hook.</summary>
        protected virtual void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
        }

        /// <summary>WPF key-up hook.</summary>
        protected virtual void OnKeyUp(System.Windows.Input.KeyEventArgs e)
        {
        }

        /// <inheritdoc />
        protected override void OnPointerPressed(Avalonia.Input.PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            System.Windows.Input.MouseButtonEventArgs args = WpfInputBridge.CreateMouseButtonEvent(this, e);
            OnMouseDown(args);
            MouseDown?.Invoke(this, args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerReleased(Avalonia.Input.PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            System.Windows.Input.MouseButtonEventArgs args = WpfInputBridge.CreateMouseButtonEvent(this, e);
            OnMouseUp(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerMoved(Avalonia.Input.PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            System.Windows.Input.MouseEventArgs args = WpfInputBridge.CreateMouseMoveEvent(this, e);
            OnMouseMove(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerWheelChanged(Avalonia.Input.PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            System.Windows.Input.MouseWheelEventArgs args = WpfInputBridge.CreateMouseWheelEvent(this, e);
            OnMouseWheel(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerCaptureLost(Avalonia.Input.PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            System.Windows.Input.MouseEventArgs args = WpfInputBridge.CreateLostMouseCaptureEvent(this, e.Pointer, e.Source);
            OnLostMouseCapture(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            System.Windows.Input.KeyEventArgs args = WpfInputBridge.CreateKeyEvent(e, true);
            OnKeyDown(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnKeyUp(Avalonia.Input.KeyEventArgs e)
        {
            base.OnKeyUp(e);
            System.Windows.Input.KeyEventArgs args = WpfInputBridge.CreateKeyEvent(e, false);
            OnKeyUp(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <summary>WPF-style value accessor. Shadows Avalonia's <c>GetValue(AvaloniaProperty)</c> so
        /// unqualified <c>GetValue(dp)</c> in ported controls binds the WPF <see cref="DependencyProperty"/>.</summary>
        public object? GetValue(DependencyProperty property)
            => DependencyPropertyServices.GetValue(this, property);

        /// <summary>WPF-style value setter for a <see cref="DependencyProperty"/>.</summary>
        public void SetValue(DependencyProperty property, object? value)
            => DependencyPropertyServices.SetValue(this, property, value);

        /// <summary>WPF-style value setter for a read-only <see cref="DependencyPropertyKey"/>.</summary>
        public void SetValue(DependencyPropertyKey key, object? value)
            => DependencyPropertyServices.SetValue(this, key, value);

        /// <summary>WPF-style local-value setter (no coercion re-entry) for a <see cref="DependencyProperty"/>.</summary>
        public void SetCurrentValue(DependencyProperty property, object? value)
            => DependencyPropertyServices.SetCurrentValue(this, property, value);

        /// <summary>WPF-style value clear for a <see cref="DependencyProperty"/>.</summary>
        public void ClearValue(DependencyProperty property)
            => DependencyPropertyServices.ClearValue(this, property);

        /// <summary>Identity for WPF's <c>MouseMove</c> routed event.</summary>
        public static readonly RoutedEvent MouseMoveEvent =
            EventManager.RegisterRoutedEvent("MouseMove", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(UIElement));

        /// <summary>Identity for WPF's <c>MouseDown</c> (preview/bubble) routed event.</summary>
        public static readonly RoutedEvent MouseDownEvent =
            EventManager.RegisterRoutedEvent("MouseDown", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(UIElement));

        /// <summary>Identity for WPF's <c>MouseUp</c> routed event.</summary>
        public static readonly RoutedEvent MouseUpEvent =
            EventManager.RegisterRoutedEvent("MouseUp", RoutingStrategy.Bubble, typeof(MouseButtonEventHandler), typeof(UIElement));

        /// <summary>Identity for WPF's <c>MouseWheel</c> routed event.</summary>
        public static readonly RoutedEvent MouseWheelEvent =
            EventManager.RegisterRoutedEvent("MouseWheel", RoutingStrategy.Bubble, typeof(MouseWheelEventHandler), typeof(UIElement));

        /// <summary>Identity for WPF's <c>LostMouseCapture</c> routed event.</summary>
        public static readonly RoutedEvent LostMouseCaptureEvent =
            EventManager.RegisterRoutedEvent("LostMouseCapture", RoutingStrategy.Bubble, typeof(MouseEventHandler), typeof(UIElement));

        /// <summary>Identity for WPF's <c>KeyDown</c> routed event. Hides the Avalonia base field by design.</summary>
        public static readonly new RoutedEvent KeyDownEvent =
            EventManager.RegisterRoutedEvent("KeyDown", RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(UIElement));

        /// <summary>Identity for WPF's <c>KeyUp</c> routed event. Hides the Avalonia base field by design.</summary>
        public static readonly new RoutedEvent KeyUpEvent =
            EventManager.RegisterRoutedEvent("KeyUp", RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(UIElement));

        /// <summary>Identity for WPF's <c>PreviewKeyUp</c> routed event.</summary>
        public static readonly RoutedEvent PreviewKeyUpEvent =
            EventManager.RegisterRoutedEvent("PreviewKeyUp", RoutingStrategy.Tunnel, typeof(KeyEventHandler), typeof(UIElement));

        /// <summary>Identity for WPF's <c>LostKeyboardFocus</c> routed event.</summary>
        public static readonly RoutedEvent LostKeyboardFocusEvent =
            EventManager.RegisterRoutedEvent("LostKeyboardFocus", RoutingStrategy.Bubble, typeof(KeyboardFocusChangedEventHandler), typeof(UIElement));
    }

    /// <summary>
    /// WPF-compatible <see cref="FrameworkElement"/>. Mirrors WPF's hierarchy
    /// (<c>FrameworkElement : UIElement</c>) over Avalonia's <see cref="Control"/>.
    /// </summary>
    public class FrameworkElement : UIElement
    {
    }

    /// <summary>
    /// Records WPF-style class-handler registrations. The runtime bridging of these to Avalonia's
    /// class-handler system (e.g. <c>KeyComboGesture</c>'s reset on key-up / focus-lost) is wired
    /// when the owning controls are ported; for now the registrations are simply retained so the
    /// WPF-shaped <see cref="EventManager.RegisterClassHandler"/> call sites compile and run.
    /// </summary>
    internal static class ClassHandlerRegistry
    {
        private static readonly List<(Type ClassType, RoutedEvent Event, Delegate Handler, bool HandledToo)> _handlers
            = new List<(Type, RoutedEvent, Delegate, bool)>();

        public static void Register(Type classType, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
            => _handlers.Add((classType, routedEvent, handler, handledEventsToo));
    }
}

namespace System.Windows.Input
{
    /// <summary>WPF-compatible focus navigation directions.</summary>
    public enum FocusNavigationDirection
    {
        /// <summary>Move to the next focusable element.</summary>
        Next,

        /// <summary>Move to the previous focusable element.</summary>
        Previous,

        /// <summary>Move to the first focusable element.</summary>
        First,

        /// <summary>Move to the last focusable element.</summary>
        Last,

        /// <summary>Move to the element on the left.</summary>
        Left,

        /// <summary>Move to the element on the right.</summary>
        Right,

        /// <summary>Move to the element above.</summary>
        Up,

        /// <summary>Move to the element below.</summary>
        Down,
    }

    /// <summary>WPF-compatible keyboard-focus-changed event data.</summary>
    public class KeyboardFocusChangedEventArgs : RoutedEventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="KeyboardFocusChangedEventArgs"/> class.</summary>
        public KeyboardFocusChangedEventArgs()
        {
        }

        /// <summary>Gets or sets the element that previously held focus.</summary>
        public IInputElement? OldFocus { get; set; }

        /// <summary>Gets or sets the element that now holds focus.</summary>
        public IInputElement? NewFocus { get; set; }
    }

    /// <summary>WPF-compatible keyboard-focus-changed event handler.</summary>
    public delegate void KeyboardFocusChangedEventHandler(object sender, KeyboardFocusChangedEventArgs e);
}
