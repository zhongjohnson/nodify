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

namespace System.Windows
{
    /// <summary>
    /// WPF-compatible base for input-aware elements. Derives from Avalonia's
    /// <see cref="Control"/> so ported controls can inherit it, and hosts the WPF-shaped
    /// input <see cref="RoutedEvent"/> identities used by the interactivity state machine.
    /// </summary>
    public class UIElement : Control
    {
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
    }

    /// <summary>WPF-compatible keyboard-focus-changed event handler.</summary>
    public delegate void KeyboardFocusChangedEventHandler(object sender, KeyboardFocusChangedEventArgs e);
}
