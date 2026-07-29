// -----------------------------------------------------------------------------
//  WPF `UIElement.MouseDown` event bridge
// -----------------------------------------------------------------------------
//  Upstream GroupingNode subscribes with the WPF idiom:
//
//      HeaderControl.MouseDown += OnHeaderMouseDown;
//      HeaderControl.MouseDown -= OnHeaderMouseDown;
//
//  C# 14 extension members support properties and methods but NOT events, so the
//  event cannot be added to Avalonia's Control directly. However, `x.P += h` is
//  compiled as `x.P = x.P + h`, which means a read/write extension property whose
//  type overloads `+` and `-` reproduces the exact WPF syntax. This struct is that
//  type: the operators perform the real Avalonia PointerPressed subscription.
//
//  Each WPF handler is wrapped in an Avalonia handler; the wrapper is remembered per
//  (element, handler) pair so `-=` detaches precisely the subscription that `+=`
//  created. The table is keyed weakly on the element so controls are not leaked.
// -----------------------------------------------------------------------------
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Input;
using AvControl = Avalonia.Controls.Control;
using PointerPressedHandler = global::System.EventHandler<global::Avalonia.Input.PointerPressedEventArgs>;

namespace Nodify.Avalonia.Compatibility
{
    /// <summary>
    /// Bridges the WPF <c>MouseDown</c> event idiom onto Avalonia's <c>PointerPressed</c>.
    /// </summary>
    public readonly struct MouseDownAccessor
    {
        private static readonly ConditionalWeakTable<AvControl, Dictionary<MouseButtonEventHandler, PointerPressedHandler>> _wrappers = new();

        private readonly AvControl _element;

        internal MouseDownAccessor(AvControl element) => _element = element;

        /// <summary>Subscribes a WPF-shaped mouse-down handler.</summary>
        public static MouseDownAccessor operator +(MouseDownAccessor accessor, MouseButtonEventHandler handler)
        {
            AvControl element = accessor._element;
            if (element == null || handler == null)
            {
                return accessor;
            }

            void Wrapper(object? sender, PointerPressedEventArgs e)
            {
                MouseButtonEventArgs args = WpfInputBridge.CreateMouseButtonEvent(element, e);
                handler(sender ?? element, args);
                WpfInputBridge.CopyHandled(args, e);
            }

            Dictionary<MouseButtonEventHandler, PointerPressedHandler> map = _wrappers.GetOrCreateValue(element);
            PointerPressedHandler wrapper = Wrapper;

            lock (map)
            {
                if (map.ContainsKey(handler))
                {
                    return accessor;
                }

                map[handler] = wrapper;
            }

            element.PointerPressed += wrapper;
            return accessor;
        }

        /// <summary>Unsubscribes a previously added WPF-shaped mouse-down handler.</summary>
        public static MouseDownAccessor operator -(MouseDownAccessor accessor, MouseButtonEventHandler handler)
        {
            AvControl element = accessor._element;
            if (element == null || handler == null || !_wrappers.TryGetValue(element, out var map))
            {
                return accessor;
            }

            PointerPressedHandler wrapper;
            lock (map)
            {
                if (!map.TryGetValue(handler, out wrapper))
                {
                    return accessor;
                }

                map.Remove(handler);
            }

            element.PointerPressed -= wrapper;
            return accessor;
        }
    }
}
