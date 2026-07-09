// -----------------------------------------------------------------------------
//  WPF routed-event instance-method bridge (System.Windows)
// -----------------------------------------------------------------------------
//  Avalonia's Interactive already provides:
//      RaiseEvent(RoutedEventArgs)                                  (1 arg)  -> used as-is
//      RemoveHandler(RoutedEvent, Delegate)                         (2 args) -> used as-is
//      AddHandler(RoutedEvent, Delegate, RoutingStrategies, bool)   (4 args)
//
//  Upstream Nodify, however, uses WPF's shorter AddHandler overloads:
//      AddHandler(RoutedEvent, Delegate)            (2 args)
//      AddHandler(RoutedEvent, Delegate, bool)      (3 args, handledEventsToo)
//
//  These extension methods supply exactly those two shapes. They bind because the
//  native Avalonia AddHandler needs four arguments, so the 2-/3-arg call sites fall
//  through to these extensions. The event's own routing strategy is forwarded to
//  Avalonia so bubbling/tunneling behavior is preserved.
// -----------------------------------------------------------------------------

using Avalonia.Interactivity;

namespace System.Windows
{
    /// <summary>WPF-shaped <c>AddHandler</c> overloads for Avalonia interactive elements.</summary>
    public static class RoutedEventServices
    {
        /// <summary>WPF <c>AddHandler(RoutedEvent, Delegate)</c>.</summary>
        public static void AddHandler(this Interactive element, RoutedEvent routedEvent, Delegate handler)
            => element.AddHandler(routedEvent, handler, routedEvent.RoutingStrategies, handledEventsToo: false);

        /// <summary>WPF <c>AddHandler(RoutedEvent, Delegate, bool handledEventsToo)</c>.</summary>
        public static void AddHandler(this Interactive element, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo)
            => element.AddHandler(routedEvent, handler, routedEvent.RoutingStrategies, handledEventsToo);
    }
}
