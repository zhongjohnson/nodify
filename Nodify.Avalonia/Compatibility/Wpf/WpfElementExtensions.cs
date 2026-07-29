// -----------------------------------------------------------------------------
//  WPF-shaped API surface projected onto Avalonia's Control (C# 14 extension members)
// -----------------------------------------------------------------------------
//  GlobalUsings.cs aliases `UIElement` and `FrameworkElement` onto Avalonia's
//  `Control`. That alias is required so upstream generic constraints such as
//  `where TElement : FrameworkElement` are satisfied by the ported controls, which
//  derive from Avalonia controls. The consequence is that the WPF-shaped shim
//  classes in Compatibility/Wpf/FrameworkElement.cs are shadowed by the alias and
//  their members are unreachable from the linked upstream sources.
//
//  C# 14 extension members close that gap without touching upstream Nodify: they
//  can add INSTANCE members *and* STATIC members to a type we do not own, so both
//  `element.ActualWidth` and `UIElement.MouseMoveEvent` resolve against `Control`.
//
//  The routed-event identities deliberately map onto the real Avalonia events, so
//  the interactivity state machine dispatches genuine Avalonia input rather than
//  merely comparing opaque tokens.
// -----------------------------------------------------------------------------
using AvControl = Avalonia.Controls.Control;
using AvVisual = Avalonia.Visual;
using AvPoint = Avalonia.Point;
using RoutedEvent = Avalonia.Interactivity.RoutedEvent;
using InputElement = Avalonia.Input.InputElement;
using WpfPoint = System.Windows.Point;
using WpfSize = System.Windows.Size;
using Avalonia.VisualTree;
using Avalonia.Input;
using System.Collections.Generic;
using System.Windows.Input;

namespace Nodify.Avalonia.Compatibility
{
    /// <summary>
    /// Projects the WPF <c>UIElement</c>/<c>FrameworkElement</c> API surface used by the linked
    /// Nodify sources onto Avalonia's <see cref="AvControl"/>.
    /// </summary>
    public static class WpfElementExtensions
    {
        extension(AvControl element)
        {
            /// <summary>WPF's <c>RenderSize</c>; Avalonia exposes the rendered size as <c>Bounds.Size</c>.</summary>
            public WpfSize RenderSize => new WpfSize(element.Bounds.Width, element.Bounds.Height);

            /// <summary>WPF's <c>ActualWidth</c>.</summary>
            public double ActualWidth => element.Bounds.Width;

            /// <summary>WPF's <c>ActualHeight</c>.</summary>
            public double ActualHeight => element.Bounds.Height;

            /// <summary>WPF's <c>IsKeyboardFocused</c>.</summary>
            public bool IsKeyboardFocused => element.IsFocused;

            /// <summary>
            /// WPF's <c>IsMouseCaptured</c>. Avalonia tracks capture on the pointer device rather
            /// than the element, so this delegates to the existing input bridge.
            /// </summary>
            public bool IsMouseCaptured
                => WpfInputBridge.IsMouseCaptured(element);

            /// <summary>WPF's <c>CaptureMouse</c>.</summary>
            public bool CaptureMouse()
                => WpfInputBridge.CaptureMouse(element);

            /// <summary>WPF's <c>ReleaseMouseCapture</c>.</summary>
            public void ReleaseMouseCapture()
                => InputStateTracker.Pointer?.Capture(null);

            /// <summary>
            /// WPF's <c>TranslatePoint</c>. Avalonia's equivalent returns a nullable point because the
            /// two visuals may live in disconnected trees; upstream assumes a non-null result, so an
            /// unconnected pair degrades to the untranslated point.
            /// </summary>
            public WpfPoint TranslatePoint(WpfPoint point, AvVisual relativeTo)
            {
                AvPoint? translated = element.TranslatePoint(new AvPoint(point.X, point.Y), relativeTo);
                return translated.HasValue
                    ? new WpfPoint(translated.Value.X, translated.Value.Y)
                    : point;
            }

            /// <summary>
            /// WPF's <c>IsAncestorOf</c>, expressed via Avalonia's visual-tree ancestry test.
            /// </summary>
            public bool IsAncestorOf(AvVisual descendant)
                => descendant != null && element.IsVisualAncestorOf(descendant);

            /// <summary>
            /// WPF's <c>UIElement.MouseDown</c>, bridged onto Avalonia's <c>PointerPressed</c>.
            /// </summary>
            /// <remarks>
            /// C# 14 has no extension EVENTS, so this is modelled as a read/write extension property
            /// whose type overloads <c>+</c> and <c>-</c>. The compiler expands the upstream
            /// <c>element.MouseDown += handler</c> into <c>element.MouseDown = element.MouseDown + handler</c>,
            /// so the operator performs the real subscription and the setter is intentionally inert.
            /// </remarks>
            public global::Nodify.Avalonia.Compatibility.MouseDownAccessor MouseDown
            {
                get => new global::Nodify.Avalonia.Compatibility.MouseDownAccessor(element);
                set { /* Subscription already happened in the +/- operator. */ }
            }
        }

        extension(AvControl)
        {
            // WPF input routed-event identities. `KeyDownEvent`/`KeyUpEvent` are NOT declared here:
            // Avalonia's InputElement already exposes those exact names, and a real member always
            // takes precedence over an extension member.

            /// <summary>WPF's <c>UIElement.MouseMoveEvent</c>.</summary>
            public static RoutedEvent MouseMoveEvent => InputElement.PointerMovedEvent;

            /// <summary>WPF's <c>UIElement.MouseDownEvent</c>.</summary>
            public static RoutedEvent MouseDownEvent => InputElement.PointerPressedEvent;

            /// <summary>WPF's <c>UIElement.MouseUpEvent</c>.</summary>
            public static RoutedEvent MouseUpEvent => InputElement.PointerReleasedEvent;

            /// <summary>WPF's <c>UIElement.MouseWheelEvent</c>.</summary>
            public static RoutedEvent MouseWheelEvent => InputElement.PointerWheelChangedEvent;

            /// <summary>WPF's <c>UIElement.LostMouseCaptureEvent</c>.</summary>
            public static RoutedEvent LostMouseCaptureEvent => InputElement.PointerCaptureLostEvent;

            /// <summary>
            /// WPF's <c>UIElement.PreviewKeyUpEvent</c>. Avalonia has no distinct preview event; the
            /// tunnelling phase is a routing strategy on the single key-up event.
            /// </summary>
            public static RoutedEvent PreviewKeyUpEvent => InputElement.KeyUpEvent;

            /// <summary>WPF's <c>UIElement.LostKeyboardFocusEvent</c>.</summary>
            public static RoutedEvent LostKeyboardFocusEvent => InputElement.LostFocusEvent;
        }
    }
}
