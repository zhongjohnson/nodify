// -----------------------------------------------------------------------------
//  WPF routed-event shims (System.Windows)
// -----------------------------------------------------------------------------
//  Upstream Nodify declares routed events with WPF's model:
//
//      public static readonly RoutedEvent FooEvent =
//          EventManager.RegisterRoutedEvent(nameof(Foo), RoutingStrategy.Bubble,
//              typeof(FooEventHandler), typeof(Owner));
//
//      RaiseEvent(new FooEventArgs(...) { RoutedEvent = FooEvent });
//
//  Avalonia already has a routed-event system (Avalonia.Interactivity) that is
//  *shaped differently* but expressive enough to host the WPF surface. Rather than
//  rewriting every upstream registration/raise site, these shims let the original
//  code compile and run:
//
//    * System.Windows.RoutedEvent      : Avalonia.Interactivity.RoutedEvent
//        -> a WPF RoutedEvent IS an Avalonia RoutedEvent, so it can be passed
//           straight to Interactive.AddHandler / RaiseEvent. Adds WPF's AddOwner.
//    * System.Windows.RoutedEventArgs  : Avalonia.Interactivity.RoutedEventArgs
//        -> Handled/Source/RoutedEvent come from the Avalonia base; we add the
//           WPF (RoutedEvent, source) ctor and the InvokeEventHandler override
//           point so upstream event-arg classes compile verbatim.
//    * System.Windows.EventManager.RegisterRoutedEvent(...) -> builds the shim event.
//
//  NOTE: Avalonia invokes handlers itself through its EventRoute, so the WPF
//  InvokeEventHandler override on ported args classes is not called by the runtime;
//  it is retained only to keep those files identical to upstream (easy merges).
//  All upstream handler delegates have the shape `void (object sender, TArgs e)`
//  with `TArgs : RoutedEventArgs`, which Avalonia's non-generic AddHandler accepts.
// -----------------------------------------------------------------------------

using System;
using Avalonia.Interactivity;
using AvRoutedEvent = Avalonia.Interactivity.RoutedEvent;
using AvRoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;
using AvRoutingStrategies = Avalonia.Interactivity.RoutingStrategies;

namespace System.Windows
{
    /// <summary>WPF routing strategy. Maps 1:1 to <see cref="AvRoutingStrategies"/>.</summary>
    public enum RoutingStrategy
    {
        /// <summary>Route tunnels down from the root to the source (preview events).</summary>
        Tunnel = AvRoutingStrategies.Tunnel,

        /// <summary>Route bubbles up from the source to the root.</summary>
        Bubble = AvRoutingStrategies.Bubble,

        /// <summary>Route does not travel; only the source element is notified.</summary>
        Direct = AvRoutingStrategies.Direct,
    }

    /// <summary>
    /// WPF-compatible routed event. Derives from Avalonia's routed event so it can be
    /// used directly with Avalonia's <c>AddHandler</c>/<c>RemoveHandler</c>/<c>RaiseEvent</c>.
    /// </summary>
    public class RoutedEvent : AvRoutedEvent
    {
        internal RoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
            : base(name, ToAvalonia(routingStrategy), typeof(AvRoutedEventArgs), ownerType)
        {
            HandlerType = handlerType;
        }

        /// <summary>The WPF handler delegate type registered for this event.</summary>
        public Type HandlerType { get; }

        /// <summary>The WPF routing strategy for this event.</summary>
        public RoutingStrategy RoutingStrategy => (RoutingStrategy)RoutingStrategies;

        /// <summary>
        /// WPF's <c>AddOwner</c> lets another type reuse an existing routed event.
        /// Avalonia routed events are not owner-scoped for handler attachment, so the
        /// same event instance is simply returned (matching observable WPF behavior for
        /// how Nodify uses it).
        /// </summary>
        public RoutedEvent AddOwner(Type ownerType) => this;

        internal static AvRoutingStrategies ToAvalonia(RoutingStrategy strategy)
            => (AvRoutingStrategies)strategy;
    }

    /// <summary>
    /// WPF-compatible routed event data. Derives from Avalonia's <see cref="AvRoutedEventArgs"/>
    /// so <c>Handled</c>, <c>Source</c> and <c>RoutedEvent</c> are provided by the base.
    /// </summary>
    public class RoutedEventArgs : AvRoutedEventArgs
    {
        public RoutedEventArgs()
        {
        }

        public RoutedEventArgs(RoutedEvent routedEvent)
            : base(routedEvent)
        {
        }

        public RoutedEventArgs(RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
        }

        /// <summary>
        /// WPF extensibility point invoked by WPF's event system to call a strongly-typed
        /// handler. Retained for source compatibility; Avalonia performs handler invocation
        /// itself, so overrides are effectively unused at runtime.
        /// </summary>
        protected virtual void InvokeEventHandler(Delegate genericHandler, object genericTarget)
            => genericHandler.DynamicInvoke(genericTarget, this);
    }

    /// <summary>WPF-compatible <c>EventManager</c> for registering routed events.</summary>
    public static class EventManager
    {
        /// <summary>Registers a routed event, mirroring WPF's signature.</summary>
        public static RoutedEvent RegisterRoutedEvent(string name, RoutingStrategy routingStrategy, Type handlerType, Type ownerType)
            => new RoutedEvent(name, routingStrategy, handlerType, ownerType);

        /// <summary>
        /// Registers a class handler for a routed event on all instances of the given type,
        /// mirroring WPF's signature. The registration is recorded by the shim; the runtime
        /// bridge onto Avalonia's class-handler system is wired when the owning controls are
        /// ported. Used by <c>KeyComboGesture</c> to observe global key-up / focus-lost.
        /// </summary>
        public static void RegisterClassHandler(Type classType, RoutedEvent routedEvent, Delegate handler, bool handledEventsToo = false)
            => ClassHandlerRegistry.Register(classType, routedEvent, handler, handledEventsToo);
    }


    /// <summary>WPF's standard non-typed routed event handler.</summary>
    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);
}
