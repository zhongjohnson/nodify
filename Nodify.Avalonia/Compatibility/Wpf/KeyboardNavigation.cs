// -----------------------------------------------------------------------------
//  WPF keyboard-navigation shims (System.Windows.Input)
// -----------------------------------------------------------------------------
//  The editor-core selection/navigation sources reference WPF's keyboard-navigation surface:
//    * KeyboardNavigation.TabNavigationProperty / ControlTabNavigationProperty /
//      DirectionalNavigationProperty -- overridden in static ctors to disable tab/directional
//      navigation on the selector controls.
//    * TraversalRequest -- passed to the focus navigators (carries a FocusNavigationDirection and a
//      mutable Wrapped flag).
//  Avalonia's KeyboardNavigation exposes TabNavigation only (no separate control-tab/directional
//  attached properties), so the missing ones are provided as record-only WPF DPs (metadata only;
//  navigation policy is a theme/behavior concern deferred), while TabNavigation wraps the real
//  Avalonia attached property so the override actually takes effect.
// -----------------------------------------------------------------------------

using System.Windows;
using AvKeyboardNavigation = Avalonia.Input.KeyboardNavigation;

namespace System.Windows.Input
{
    /// <summary>
    /// WPF-compatible <see cref="KeyboardNavigation"/> exposing the tab/control-tab/directional
    /// navigation attached properties upstream overrides in its selector static ctors.
    /// </summary>
    public static class KeyboardNavigation
    {
        /// <summary>WPF <c>TabNavigation</c> attached property, wrapping Avalonia's real
        /// <see cref="AvKeyboardNavigation.TabNavigationProperty"/>.</summary>
        public static readonly DependencyProperty TabNavigationProperty =
            DependencyProperty.FromExisting(AvKeyboardNavigation.TabNavigationProperty, typeof(KeyboardNavigation));

        /// <summary>Record-only WPF <c>ControlTabNavigation</c> attached property. Avalonia has no separate
        /// control-tab policy, so this carries metadata only (navigation policy deferred to the theme phase).</summary>
        public static readonly DependencyProperty ControlTabNavigationProperty =
            DependencyProperty.Register("ControlTabNavigation", typeof(KeyboardNavigationMode), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));

        /// <summary>Record-only WPF <c>DirectionalNavigation</c> attached property. Avalonia has no directional
        /// navigation policy attached property, so this carries metadata only (deferred to the theme phase).</summary>
        public static readonly DependencyProperty DirectionalNavigationProperty =
            DependencyProperty.Register("DirectionalNavigation", typeof(KeyboardNavigationMode), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));
    }

    /// <summary>
    /// WPF-compatible <see cref="TraversalRequest"/>. Describes a focus-traversal direction and records
    /// whether the traversal wrapped around, as the Nodify focus navigators expect.
    /// </summary>
    public class TraversalRequest
    {
        /// <summary>Initializes a new instance with the given navigation direction.</summary>
        public TraversalRequest(FocusNavigationDirection focusNavigationDirection)
        {
            FocusNavigationDirection = focusNavigationDirection;
        }

        /// <summary>Gets the direction of the traversal.</summary>
        public FocusNavigationDirection FocusNavigationDirection { get; }

        /// <summary>Gets or sets whether the traversal wrapped past the first/last element.</summary>
        public bool Wrapped { get; set; }
    }
}
