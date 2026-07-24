using System.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;

namespace System.Windows.Threading
{
    /// <summary>WPF-compatible dispatcher timer over Avalonia's dispatcher timer.</summary>
    public class DispatcherTimer
    {
        private readonly Avalonia.Threading.DispatcherTimer _inner;

        /// <summary>Initializes a timer with a priority and dispatcher.</summary>
        public DispatcherTimer(Avalonia.Threading.DispatcherPriority priority, Avalonia.Threading.Dispatcher dispatcher)
        {
            _inner = new Avalonia.Threading.DispatcherTimer(priority);
        }

        /// <summary>Gets or sets the timer interval.</summary>
        public TimeSpan Interval
        {
            get => _inner.Interval;
            set => _inner.Interval = value;
        }

        /// <summary>Occurs on each timer tick.</summary>
        public event EventHandler? Tick
        {
            add => _inner.Tick += value;
            remove => _inner.Tick -= value;
        }

        /// <summary>Starts the timer.</summary>
        public void Start() => _inner.Start();

        /// <summary>Stops the timer.</summary>
        public void Stop() => _inner.Stop();
    }

    /// <summary>WPF-compatible dispatcher priority values.</summary>
    public static class DispatcherPriority
    {
        /// <summary>Gets Avalonia's background dispatcher priority.</summary>
        public static Avalonia.Threading.DispatcherPriority Background => Avalonia.Threading.DispatcherPriority.Background;
    }
}

namespace System.Windows.Controls.Primitives
{
    /// <summary>WPF-compatible drag delta event data.</summary>
    public class DragDeltaEventArgs : RoutedEventArgs
    {
        /// <summary>Gets or sets the horizontal drag delta.</summary>
        public double HorizontalChange { get; set; }

        /// <summary>Gets or sets the vertical drag delta.</summary>
        public double VerticalChange { get; set; }
    }

    /// <summary>WPF-compatible drag-start event data.</summary>
    public class DragStartedEventArgs : RoutedEventArgs
    {
    }

    /// <summary>WPF-compatible drag-completed event data.</summary>
    public class DragCompletedEventArgs : RoutedEventArgs
    {
    }

    /// <summary>WPF-compatible drag delta handler.</summary>
    public delegate void DragDeltaEventHandler(object sender, DragDeltaEventArgs e);

    /// <summary>WPF-compatible drag started handler.</summary>
    public delegate void DragStartedEventHandler(object sender, DragStartedEventArgs e);

    /// <summary>WPF-compatible drag completed handler.</summary>
    public delegate void DragCompletedEventHandler(object sender, DragCompletedEventArgs e);

    /// <summary>WPF-compatible thumb routed-event identities.</summary>
    public class Thumb : global::System.Windows.Controls.Control
    {
        /// <summary>Drag delta routed event.</summary>
        public static readonly RoutedEvent DragDeltaEvent =
            EventManager.RegisterRoutedEvent("DragDelta", RoutingStrategy.Bubble, typeof(DragDeltaEventHandler), typeof(Thumb));

        /// <summary>Drag started routed event.</summary>
        public static readonly RoutedEvent DragStartedEvent =
            EventManager.RegisterRoutedEvent("DragStarted", RoutingStrategy.Bubble, typeof(DragStartedEventHandler), typeof(Thumb));

        /// <summary>Drag completed routed event.</summary>
        public static readonly RoutedEvent DragCompletedEvent =
            EventManager.RegisterRoutedEvent("DragCompleted", RoutingStrategy.Bubble, typeof(DragCompletedEventHandler), typeof(Thumb));
    }
}

namespace System.Windows.Input
{
    /// <summary>WPF-compatible executed-command event data.</summary>
    public class ExecutedRoutedEventArgs : RoutedEventArgs
    {
        /// <summary>Gets or sets the command parameter.</summary>
        public object? Parameter { get; set; }
    }

    /// <summary>WPF-compatible can-execute event data.</summary>
    public class CanExecuteRoutedEventArgs : RoutedEventArgs
    {
        /// <summary>Gets or sets the command parameter.</summary>
        public object? Parameter { get; set; }

        /// <summary>Gets or sets whether the command can execute.</summary>
        public bool CanExecute { get; set; }
    }

    /// <summary>WPF-compatible executed command handler.</summary>
    public delegate void ExecutedRoutedEventHandler(object sender, ExecutedRoutedEventArgs e);

    /// <summary>WPF-compatible can-execute command handler.</summary>
    public delegate void CanExecuteRoutedEventHandler(object sender, CanExecuteRoutedEventArgs e);

    /// <summary>Stores a routed command and its handlers.</summary>
    public sealed class CommandBinding
    {
        /// <summary>Initializes a command binding.</summary>
        public CommandBinding(ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute)
        {
            Command = command;
            Executed = executed;
            CanExecute = canExecute;
        }

        internal ICommand Command { get; }
        internal ExecutedRoutedEventHandler Executed { get; }
        internal CanExecuteRoutedEventHandler CanExecute { get; }
    }

    /// <summary>Records WPF class command bindings for later Avalonia behavior wiring.</summary>
    public static class CommandManager
    {
        private static readonly Dictionary<Type, List<CommandBinding>> Bindings = new();

        /// <summary>Registers a class command binding.</summary>
        public static void RegisterClassCommandBinding(Type type, CommandBinding binding)
        {
            if (!Bindings.TryGetValue(type, out List<CommandBinding>? bindings))
            {
                bindings = new List<CommandBinding>();
                Bindings[type] = bindings;
            }

            bindings.Add(binding);
        }
    }
}

namespace System.Windows
{
    /// <summary>Base type for WPF-compatible resource keys.</summary>
    public abstract class ResourceKey
    {
    }

    /// <summary>WPF-compatible component resource key.</summary>
    public sealed class ComponentResourceKey : ResourceKey
    {
        /// <summary>Initializes a component resource key.</summary>
        public ComponentResourceKey(Type typeInTargetAssembly, object resourceId)
        {
            TypeInTargetAssembly = typeInTargetAssembly;
            ResourceId = resourceId;
        }

        /// <summary>Gets the owning type.</summary>
        public Type TypeInTargetAssembly { get; }

        /// <summary>Gets the resource identifier.</summary>
        public object ResourceId { get; }
    }

    /// <summary>WPF-compatible font-size converter.</summary>
    public sealed class FontSizeConverter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        /// <inheritdoc />
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
            => value is string text
                ? double.Parse(text, NumberStyles.Float, culture ?? CultureInfo.InvariantCulture)
                : base.ConvertFrom(context, culture, value);
    }
}
