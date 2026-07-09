// -----------------------------------------------------------------------------
//  WPF input event-args shims (System.Windows.Input)
// -----------------------------------------------------------------------------
//  Upstream Nodify handles input through WPF's event-arg hierarchy:
//
//      InputEventArgs
//        +- MouseEventArgs          (GetPosition, LeftButton/RightButton/MiddleButton)
//        |    +- MouseButtonEventArgs (ChangedButton, ButtonState, ClickCount)
//        |    +- MouseWheelEventArgs  (Delta)
//        +- KeyEventArgs            (Key, IsDown)
//
//  These shims derive from the WPF `RoutedEventArgs` shim (System.Windows) so
//  `Handled`/`Source`/`RoutedEvent` come from Avalonia's routed-event base, and expose
//  the WPF-shaped members the ported code reads. Each can optionally wrap the underlying
//  Avalonia `PointerEventArgs`/`KeyEventArgs`, so `GetPosition`, `Pointer`, the per-button
//  states, and `Delta` reflect real Avalonia input data when raised from control overrides
//  during Phase 3b. They also support standalone construction for tests/synthetic input.
// -----------------------------------------------------------------------------

using System.Windows;
using Avalonia;
using Avalonia.Input;
using AvKeyEventArgs = Avalonia.Input.KeyEventArgs;
using AvPointerEventArgs = Avalonia.Input.PointerEventArgs;

namespace System.Windows.Input
{
    /// <summary>
    /// WPF-compatible base class for input event data. Derives from the routed-event shim
    /// so <see cref="RoutedEventArgs.Handled"/> and <see cref="RoutedEventArgs.Source"/> are
    /// available to the ported input pipeline.
    /// </summary>
    public abstract class InputEventArgs : RoutedEventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="InputEventArgs"/> class.</summary>
        protected InputEventArgs()
        {
        }

        /// <summary>Initializes a new instance wrapping an Avalonia routed-event args instance.</summary>
        /// <param name="source">The original Avalonia input event, if any.</param>
        protected InputEventArgs(Avalonia.Interactivity.RoutedEventArgs? source)
        {
            AvaloniaArgs = source;

            if (source != null)
            {
                Source = source.Source;
                Handled = source.Handled;
            }
        }

        /// <summary>
        /// The underlying Avalonia event args this instance wraps, when raised from a real
        /// pointer/key event. May be <see langword="null"/> for synthetic input.
        /// </summary>
        public Avalonia.Interactivity.RoutedEventArgs? AvaloniaArgs { get; }
    }

    /// <summary>WPF-compatible mouse event data over Avalonia's pointer system.</summary>
    public class MouseEventArgs : InputEventArgs
    {
        private readonly AvPointerEventArgs? _pointerArgs;

        /// <summary>Initializes a new instance of the <see cref="MouseEventArgs"/> class.</summary>
        public MouseEventArgs()
        {
        }

        /// <summary>Initializes a new instance wrapping an Avalonia <see cref="AvPointerEventArgs"/>.</summary>
        /// <param name="pointerArgs">The originating Avalonia pointer event.</param>
        public MouseEventArgs(AvPointerEventArgs pointerArgs)
            : base(pointerArgs)
        {
            _pointerArgs = pointerArgs;
        }

        /// <summary>The Avalonia pointer associated with this event, used for capture.</summary>
        public IPointer? Pointer => _pointerArgs?.Pointer;

        /// <summary>Gets the state of the left mouse button.</summary>
        public MouseButtonState LeftButton { get; set; }

        /// <summary>Gets the state of the right mouse button.</summary>
        public MouseButtonState RightButton { get; set; }

        /// <summary>Gets the state of the middle mouse button.</summary>
        public MouseButtonState MiddleButton { get; set; }

        /// <summary>
        /// Returns the pointer position relative to the specified element. Delegates to the
        /// wrapped Avalonia pointer event when present.
        /// </summary>
        /// <param name="relativeTo">The element to compute the position relative to.</param>
        public Point GetPosition(IInputElement? relativeTo)
        {
            if (_pointerArgs != null && relativeTo is Avalonia.Visual visual)
            {
                return _pointerArgs.GetPosition(visual);
            }

            return default;
        }

        /// <summary>Populates the per-button states from the current global mouse state.</summary>
        internal void UpdateButtonStatesFromGlobal()
        {
            LeftButton = Mouse.LeftButton;
            RightButton = Mouse.RightButton;
            MiddleButton = Mouse.MiddleButton;
        }
    }

    /// <summary>WPF-compatible mouse button event data.</summary>
    public class MouseButtonEventArgs : MouseEventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="MouseButtonEventArgs"/> class.</summary>
        public MouseButtonEventArgs()
        {
        }

        /// <summary>Initializes a new instance wrapping an Avalonia pointer event.</summary>
        /// <param name="pointerArgs">The originating Avalonia pointer event.</param>
        public MouseButtonEventArgs(AvPointerEventArgs pointerArgs)
            : base(pointerArgs)
        {
        }

        /// <summary>The mouse button whose state change triggered the event.</summary>
        public MouseButton ChangedButton { get; set; }

        /// <summary>The state of <see cref="ChangedButton"/>.</summary>
        public MouseButtonState ButtonState { get; set; }

        /// <summary>The number of times the button was clicked.</summary>
        public int ClickCount { get; set; } = 1;
    }

    /// <summary>WPF-compatible mouse wheel event data.</summary>
    public class MouseWheelEventArgs : MouseEventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="MouseWheelEventArgs"/> class.</summary>
        public MouseWheelEventArgs()
        {
        }

        /// <summary>Initializes a new instance wrapping an Avalonia pointer event.</summary>
        /// <param name="pointerArgs">The originating Avalonia pointer event.</param>
        public MouseWheelEventArgs(AvPointerEventArgs pointerArgs)
            : base(pointerArgs)
        {
        }

        /// <summary>
        /// A value indicating the amount the wheel has rotated, expressed in WPF's units
        /// (multiples of <see cref="Mouse.MouseWheelDeltaForOneLine"/>).
        /// </summary>
        public int Delta { get; set; }
    }

    /// <summary>WPF-compatible keyboard event data over Avalonia's key system.</summary>
    public class KeyEventArgs : InputEventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="KeyEventArgs"/> class.</summary>
        public KeyEventArgs()
        {
        }

        /// <summary>Initializes a new instance wrapping an Avalonia <see cref="AvKeyEventArgs"/>.</summary>
        /// <param name="keyArgs">The originating Avalonia key event.</param>
        /// <param name="isDown">Whether this represents a key-down event.</param>
        public KeyEventArgs(AvKeyEventArgs keyArgs, bool isDown)
            : base(keyArgs)
        {
            Key = keyArgs.Key;
            IsDown = isDown;
        }

        /// <summary>The key referenced by this event.</summary>
        public Key Key { get; set; }

        /// <summary>A value indicating whether the key is pressed (down) for this event.</summary>
        public bool IsDown { get; set; }

        /// <summary>A value indicating whether the key is released (up) for this event.</summary>
        public bool IsUp => !IsDown;
    }

    /// <summary>WPF-compatible mouse event handler delegate.</summary>
    public delegate void MouseEventHandler(object sender, MouseEventArgs e);

    /// <summary>WPF-compatible mouse button event handler delegate.</summary>
    public delegate void MouseButtonEventHandler(object sender, MouseButtonEventArgs e);

    /// <summary>WPF-compatible mouse wheel event handler delegate.</summary>
    public delegate void MouseWheelEventHandler(object sender, MouseWheelEventArgs e);

    /// <summary>WPF-compatible key event handler delegate.</summary>
    public delegate void KeyEventHandler(object sender, KeyEventArgs e);
}
