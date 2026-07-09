// -----------------------------------------------------------------------------
//  WPF input-gesture shims (System.Windows.Input)
// -----------------------------------------------------------------------------
//  Nodify builds its gesture system on WPF's input-gesture hierarchy:
//
//      InputGesture (abstract, virtual Matches(object, InputEventArgs))
//        +- KeyGesture   (Key + ModifierKeys)
//        +- MouseGesture (MouseAction + ModifierKeys)
//
//  Nodify's own gestures derive from these:
//      Nodify.Interactivity.MouseGesture   : System.Windows.Input.MouseGesture
//      Nodify.Interactivity.KeyComboGesture: KeyGesture
//      Nodify.Interactivity.MultiGesture   : InputGesture
//      Nodify.Interactivity.InputGestureRef: InputGesture
//  and call `base.Matches(...)`.
//
//  Avalonia has NO InputGesture/MouseGesture/KeyGesture with a Matches(object, InputEventArgs)
//  virtual, so WPF's matching semantics are reimplemented here:
//    * KeyGesture matches when a key-down KeyEventArgs has the same Key and the ambient
//      Keyboard.Modifiers equal the gesture's Modifiers.
//    * MouseGesture derives a MouseAction from the incoming MouseButtonEventArgs/
//      MouseWheelEventArgs (button + click count / wheel) and matches when the action and
//      ambient modifiers agree.
// -----------------------------------------------------------------------------

namespace System.Windows.Input
{
    /// <summary>
    /// WPF-compatible base class for input gestures. Derived gestures override
    /// <see cref="Matches(object, InputEventArgs)"/> to test whether an input event satisfies
    /// the gesture.
    /// </summary>
    public abstract class InputGesture
    {
        /// <summary>
        /// Determines whether this gesture matches the given input event.
        /// </summary>
        /// <param name="targetElement">The element the event is targeting.</param>
        /// <param name="inputEventArgs">The input event to test.</param>
        /// <returns><see langword="true"/> if the gesture matches; otherwise <see langword="false"/>.</returns>
        public virtual bool Matches(object targetElement, InputEventArgs inputEventArgs) => false;
    }

    /// <summary>
    /// WPF-compatible keyboard gesture defined by a <see cref="Input.Key"/> and optional
    /// <see cref="ModifierKeys"/>.
    /// </summary>
    public class KeyGesture : InputGesture
    {
        /// <summary>The key that must be pressed for the gesture to match.</summary>
        public Key Key { get; }

        /// <summary>The modifier keys that must be pressed for the gesture to match.</summary>
        public ModifierKeys Modifiers { get; }

        /// <summary>An optional display string describing the gesture.</summary>
        public string DisplayString { get; }

        /// <summary>Initializes a new instance with the specified key.</summary>
        /// <param name="key">The gesture key.</param>
        public KeyGesture(Key key) : this(key, ModifierKeys.None, string.Empty)
        {
        }

        /// <summary>Initializes a new instance with the specified key and modifiers.</summary>
        /// <param name="key">The gesture key.</param>
        /// <param name="modifiers">The required modifier keys.</param>
        public KeyGesture(Key key, ModifierKeys modifiers) : this(key, modifiers, string.Empty)
        {
        }

        /// <summary>Initializes a new instance with the specified key, modifiers, and display string.</summary>
        /// <param name="key">The gesture key.</param>
        /// <param name="modifiers">The required modifier keys.</param>
        /// <param name="displayString">A display string describing the gesture.</param>
        public KeyGesture(Key key, ModifierKeys modifiers, string displayString)
        {
            Key = key;
            Modifiers = modifiers;
            DisplayString = displayString ?? string.Empty;
        }

        /// <inheritdoc />
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (inputEventArgs is KeyEventArgs keyArgs && keyArgs.IsDown)
            {
                return keyArgs.Key == Key && Keyboard.Modifiers == Modifiers;
            }

            return false;
        }
    }

    /// <summary>
    /// WPF-compatible mouse gesture defined by a <see cref="Input.MouseAction"/> and optional
    /// <see cref="ModifierKeys"/>.
    /// </summary>
    public class MouseGesture : InputGesture
    {
        /// <summary>The mouse action that must occur for the gesture to match.</summary>
        public MouseAction MouseAction { get; set; }

        /// <summary>The modifier keys that must be pressed for the gesture to match.</summary>
        public ModifierKeys Modifiers { get; set; }

        /// <summary>Initializes a new instance with no action.</summary>
        public MouseGesture()
        {
        }

        /// <summary>Initializes a new instance with the specified action.</summary>
        /// <param name="action">The mouse action.</param>
        public MouseGesture(MouseAction action)
        {
            MouseAction = action;
        }

        /// <summary>Initializes a new instance with the specified action and modifiers.</summary>
        /// <param name="action">The mouse action.</param>
        /// <param name="modifiers">The required modifier keys.</param>
        public MouseGesture(MouseAction action, ModifierKeys modifiers)
        {
            MouseAction = action;
            Modifiers = modifiers;
        }

        /// <inheritdoc />
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (TryGetMouseAction(inputEventArgs, out MouseAction action))
            {
                return action == MouseAction && Keyboard.Modifiers == Modifiers;
            }

            return false;
        }

        /// <summary>
        /// Maps an incoming mouse event to the <see cref="Input.MouseAction"/> it represents,
        /// mirroring WPF's gesture-matching behavior.
        /// </summary>
        private static bool TryGetMouseAction(InputEventArgs inputEventArgs, out MouseAction action)
        {
            switch (inputEventArgs)
            {
                case MouseWheelEventArgs _:
                    action = MouseAction.WheelClick;
                    return true;

                case MouseButtonEventArgs buttonArgs:
                    action = ToMouseAction(buttonArgs.ChangedButton, buttonArgs.ClickCount);
                    return action != MouseAction.None;

                default:
                    action = MouseAction.None;
                    return false;
            }
        }

        private static MouseAction ToMouseAction(MouseButton button, int clickCount)
        {
            bool doubleClick = clickCount >= 2;

            switch (button)
            {
                case MouseButton.Left:
                    return doubleClick ? MouseAction.LeftDoubleClick : MouseAction.LeftClick;
                case MouseButton.Right:
                    return doubleClick ? MouseAction.RightDoubleClick : MouseAction.RightClick;
                case MouseButton.Middle:
                    return doubleClick ? MouseAction.MiddleDoubleClick : MouseAction.MiddleClick;
                default:
                    return MouseAction.None;
            }
        }
    }
}
