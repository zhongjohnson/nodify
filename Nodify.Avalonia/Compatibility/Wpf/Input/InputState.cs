// -----------------------------------------------------------------------------
//  WPF global input-state shims: Keyboard / Mouse (System.Windows.Input)
// -----------------------------------------------------------------------------
//  WPF exposes ambient input state through the `Keyboard` and `Mouse` device singletons
//  (e.g. `Keyboard.Modifiers`, `Keyboard.IsKeyDown(Key)`, `Mouse.LeftButton`,
//  `Mouse.MouseWheelDeltaForOneLine`, `Mouse.GetPosition(element)`). Avalonia has no
//  equivalent global poll of live device state, so these shims read from an
//  <see cref="InputStateTracker"/> that caches the most recent modifiers, pressed keys,
//  per-button states, and pointer position.
//
//  During Phase 3b the ported control overrides (OnPointerPressed/Released/Moved/
//  WheelChanged, OnKeyDown/Up) will feed the tracker via <see cref="InputStateTracker"/>
//  so this ambient state stays current, mirroring WPF's device singletons closely enough
//  for Nodify's gesture matching and capture-release checks.
// -----------------------------------------------------------------------------

using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using AvKeyModifiers = Avalonia.Input.KeyModifiers;

namespace System.Windows.Input
{
    /// <summary>
    /// Caches the most recent ambient input state so the WPF-shaped <see cref="Keyboard"/>
    /// and <see cref="Mouse"/> shims can report it. Updated from real Avalonia pointer/key
    /// events by the ported control overrides.
    /// </summary>
    public static class InputStateTracker
    {
        private static readonly HashSet<Key> _pressedKeys = new HashSet<Key>();

        /// <summary>The most recently observed keyboard modifiers.</summary>
        public static ModifierKeys Modifiers { get; private set; } = ModifierKeys.None;

        /// <summary>The most recently observed left mouse button state.</summary>
        public static MouseButtonState LeftButton { get; private set; } = MouseButtonState.Released;

        /// <summary>The most recently observed right mouse button state.</summary>
        public static MouseButtonState RightButton { get; private set; } = MouseButtonState.Released;

        /// <summary>The most recently observed middle mouse button state.</summary>
        public static MouseButtonState MiddleButton { get; private set; } = MouseButtonState.Released;

        /// <summary>The most recently observed pointer, used to compute relative positions.</summary>
        public static IPointer? Pointer { get; private set; }

        /// <summary>The element the last pointer position was observed relative to.</summary>
        public static Visual? PointerRoot { get; private set; }

        /// <summary>Determines whether the specified key is currently pressed.</summary>
        /// <param name="key">The key to test.</param>
        public static bool IsKeyDown(Key key) => _pressedKeys.Contains(key);

        /// <summary>Updates the cached modifiers from an Avalonia value.</summary>
        /// <param name="modifiers">The Avalonia key modifiers.</param>
        public static void UpdateModifiers(AvKeyModifiers modifiers)
            => Modifiers = modifiers.ToModifierKeys();

        /// <summary>Records a key being pressed.</summary>
        /// <param name="key">The pressed key.</param>
        public static void SetKeyDown(Key key) => _pressedKeys.Add(key);

        /// <summary>Records a key being released.</summary>
        /// <param name="key">The released key.</param>
        public static void SetKeyUp(Key key) => _pressedKeys.Remove(key);

        /// <summary>Updates the cached mouse button states.</summary>
        /// <param name="left">The left button state.</param>
        /// <param name="right">The right button state.</param>
        /// <param name="middle">The middle button state.</param>
        public static void UpdateButtons(MouseButtonState left, MouseButtonState right, MouseButtonState middle)
        {
            LeftButton = left;
            RightButton = right;
            MiddleButton = middle;
        }

        /// <summary>Updates the state of a single mouse button.</summary>
        /// <param name="button">The button whose state changed.</param>
        /// <param name="state">The new state.</param>
        public static void UpdateButton(MouseButton button, MouseButtonState state)
        {
            switch (button)
            {
                case MouseButton.Left:
                    LeftButton = state;
                    break;
                case MouseButton.Right:
                    RightButton = state;
                    break;
                case MouseButton.Middle:
                    MiddleButton = state;
                    break;
            }
        }

        /// <summary>Updates the cached pointer and its reference element.</summary>
        /// <param name="pointer">The pointer that produced the event.</param>
        /// <param name="root">The element the pointer position can be computed against.</param>
        public static void UpdatePointer(IPointer? pointer, Visual? root)
        {
            Pointer = pointer;
            PointerRoot = root;
        }
    }

    /// <summary>WPF-compatible ambient keyboard state.</summary>
    public static class Keyboard
    {
        /// <summary>Gets the set of modifier keys currently pressed.</summary>
        public static ModifierKeys Modifiers => InputStateTracker.Modifiers;

        /// <summary>Gets the currently focused element when available.</summary>
        public static IInputElement? FocusedElement { get; internal set; }

        /// <summary>Determines whether the specified key is currently pressed.</summary>
        /// <param name="key">The key to test.</param>
        public static bool IsKeyDown(Key key) => InputStateTracker.IsKeyDown(key);

        /// <summary>Determines whether the specified key is currently released.</summary>
        /// <param name="key">The key to test.</param>
        public static bool IsKeyUp(Key key) => !InputStateTracker.IsKeyDown(key);
    }

    /// <summary>Marker for keyboard-initiated input.</summary>
    public sealed class KeyboardDevice
    {
    }

    /// <summary>WPF-compatible most-recent-input-device tracker.</summary>
    public sealed class InputManager
    {
        private static readonly InputManager Instance = new InputManager();

        private InputManager()
        {
        }

        /// <summary>Gets the process-wide input manager.</summary>
        public static InputManager Current => Instance;

        /// <summary>Gets the most recently observed input device.</summary>
        public object? MostRecentInputDevice { get; internal set; }
    }

    /// <summary>WPF-compatible ambient mouse state.</summary>
    public static class Mouse
    {
        /// <summary>
        /// The amount of wheel delta that corresponds to one line of scrolling. Matches WPF's
        /// constant so upstream wheel math (<c>e.Delta / MouseWheelDeltaForOneLine</c>) is preserved.
        /// </summary>
        public const int MouseWheelDeltaForOneLine = 120;

        /// <summary>Gets the state of the left mouse button.</summary>
        public static MouseButtonState LeftButton => InputStateTracker.LeftButton;

        /// <summary>Gets the state of the right mouse button.</summary>
        public static MouseButtonState RightButton => InputStateTracker.RightButton;

        /// <summary>Gets the state of the middle mouse button.</summary>
        public static MouseButtonState MiddleButton => InputStateTracker.MiddleButton;

        /// <summary>Gets the element that currently has pointer capture, or <c>null</c> if none (WPF <c>Mouse.Captured</c>).</summary>
        public static IInputElement? Captured => InputStateTracker.Pointer?.Captured;

        /// <summary>
        /// Returns the last-known pointer position relative to the specified element.
        /// </summary>
        /// <param name="relativeTo">The element to compute the position relative to.</param>
        public static Point GetPosition(IInputElement? relativeTo)
        {
            if (InputStateTracker.PointerRoot is Visual root && relativeTo is Visual target)
            {
                Point? rootPosition = root.TranslatePoint(default, target);
                if (rootPosition.HasValue)
                {
                    return rootPosition.Value;
                }
            }

            return default;
        }
    }
}
