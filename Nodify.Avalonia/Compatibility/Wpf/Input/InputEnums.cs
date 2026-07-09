// -----------------------------------------------------------------------------
//  WPF input enum shims (System.Windows.Input)
// -----------------------------------------------------------------------------
//  Upstream Nodify (Interactivity\, Minimap\, control OnMouse*/OnKey* overrides)
//  builds its gesture/input system on WPF's input enums via `using System.Windows.Input;`
//  and bare names such as `Key.Space`, `ModifierKeys.Control`, `MouseAction.LeftClick`,
//  `MouseButton.Right`, and `MouseButtonState.Released`.
//
//  Avalonia already provides `Avalonia.Input.Key` (member-for-member a superset of the
//  keys Nodify uses) and `Avalonia.Input.MouseButton` (None/Left/Right/Middle/XButton1/
//  XButton2). Those two are therefore mapped with `global using` aliases so bare `Key`
//  and `MouseButton` resolve to the Avalonia enums everywhere in this project (matching
//  the convention documented in GlobalUsings.cs).
//
//  `ModifierKeys`, `MouseAction`, and `MouseButtonState` are WPF-shaped and have no direct
//  Avalonia counterpart with the same shape, so they are declared here inside
//  System.Windows.Input. `ModifierKeys` keeps WPF's [Flags] values (so the bitwise combos
//  used by EditorGestures keep working) and maps to/from Avalonia's `KeyModifiers`.
//
//  RULE (see GlobalUsings.cs): a name provided as a `global using` alias MUST NOT also be
//  declared as a shim type in a matching System.Windows.* namespace. Hence `Key` and
//  `MouseButton` are ONLY aliased (never redeclared below).
// -----------------------------------------------------------------------------

// Avalonia's Key/MouseButton enums are a drop-in match for the members Nodify uses.
global using Key = Avalonia.Input.Key;
global using MouseButton = Avalonia.Input.MouseButton;

using AvKeyModifiers = Avalonia.Input.KeyModifiers;

namespace System.Windows.Input
{
    /// <summary>
    /// WPF-compatible modifier keys. Values match WPF (<c>None=0, Alt=1, Control=2, Shift=4,
    /// Windows=8</c>) so the bitwise combinations used across Nodify's gesture definitions
    /// keep working. Convert to/from Avalonia's <see cref="AvKeyModifiers"/> with
    /// <see cref="ModifierKeysExtensions"/>.
    /// </summary>
    [Flags]
    public enum ModifierKeys
    {
        /// <summary>No modifiers are pressed.</summary>
        None = 0,

        /// <summary>The ALT key.</summary>
        Alt = 1,

        /// <summary>The CTRL key.</summary>
        Control = 2,

        /// <summary>The SHIFT key.</summary>
        Shift = 4,

        /// <summary>The Windows / Meta / Command key.</summary>
        Windows = 8,
    }

    /// <summary>
    /// WPF-compatible mouse actions used when defining <see cref="MouseGesture"/>s.
    /// </summary>
    public enum MouseAction
    {
        /// <summary>No action.</summary>
        None,

        /// <summary>A left mouse button click.</summary>
        LeftClick,

        /// <summary>A right mouse button click.</summary>
        RightClick,

        /// <summary>A middle mouse button click.</summary>
        MiddleClick,

        /// <summary>A mouse wheel rotation.</summary>
        WheelClick,

        /// <summary>A left mouse button double-click.</summary>
        LeftDoubleClick,

        /// <summary>A right mouse button double-click.</summary>
        RightDoubleClick,

        /// <summary>A middle mouse button double-click.</summary>
        MiddleDoubleClick,
    }

    /// <summary>WPF-compatible mouse button state.</summary>
    public enum MouseButtonState
    {
        /// <summary>The button is released.</summary>
        Released,

        /// <summary>The button is pressed.</summary>
        Pressed,
    }

    /// <summary>
    /// Conversion helpers between the WPF-shaped <see cref="ModifierKeys"/> and Avalonia's
    /// <see cref="AvKeyModifiers"/>.
    /// </summary>
    public static class ModifierKeysExtensions
    {
        /// <summary>Converts Avalonia key modifiers to WPF-shaped <see cref="ModifierKeys"/>.</summary>
        public static ModifierKeys ToModifierKeys(this AvKeyModifiers modifiers)
        {
            ModifierKeys result = ModifierKeys.None;

            if ((modifiers & AvKeyModifiers.Alt) != 0)
            {
                result |= ModifierKeys.Alt;
            }
            if ((modifiers & AvKeyModifiers.Control) != 0)
            {
                result |= ModifierKeys.Control;
            }
            if ((modifiers & AvKeyModifiers.Shift) != 0)
            {
                result |= ModifierKeys.Shift;
            }
            if ((modifiers & AvKeyModifiers.Meta) != 0)
            {
                result |= ModifierKeys.Windows;
            }

            return result;
        }

        /// <summary>Converts WPF-shaped <see cref="ModifierKeys"/> to Avalonia key modifiers.</summary>
        public static AvKeyModifiers ToKeyModifiers(this ModifierKeys modifiers)
        {
            AvKeyModifiers result = AvKeyModifiers.None;

            if ((modifiers & ModifierKeys.Alt) != 0)
            {
                result |= AvKeyModifiers.Alt;
            }
            if ((modifiers & ModifierKeys.Control) != 0)
            {
                result |= AvKeyModifiers.Control;
            }
            if ((modifiers & ModifierKeys.Shift) != 0)
            {
                result |= AvKeyModifiers.Shift;
            }
            if ((modifiers & ModifierKeys.Windows) != 0)
            {
                result |= AvKeyModifiers.Meta;
            }

            return result;
        }
    }
}
