// -----------------------------------------------------------------------------
//  WPF routed-command / application-command shims (System.Windows.Input)
// -----------------------------------------------------------------------------
//  Upstream Nodify uses WPF's command surface in a couple of places:
//
//      // EditorGestures.cs
//      SelectAll = ApplicationCommands.SelectAll.InputGestures[0].AsRef();
//
//      // EditorCommands.cs (control phase)
//      public static readonly RoutedUICommand Align = new RoutedUICommand(...);
//
//  Avalonia has no `RoutedUICommand` / `ApplicationCommands` with an `InputGestures`
//  collection, so this shim provides just enough of that shape:
//
//    * ICommand                : System.Windows.Input.ICommand -> System.Windows.Input.ICommand
//        (aliased to the BCL interface so bindings/commands compile verbatim).
//    * InputGestureCollection  : a List<InputGesture> with the WPF type name.
//    * RoutedCommand / RoutedUICommand : WPF-shaped commands exposing Name/Text and an
//        InputGestures collection. Execution is intentionally inert here; the control phase
//        wires real behavior (or replaces these with Avalonia commands) as needed.
//    * ApplicationCommands     : exposes the standard commands Nodify references, pre-seeded
//        with their conventional key gestures (e.g. SelectAll = Ctrl+A).
//
//  Only the members Nodify actually consumes are implemented; this is a compatibility
//  shim, not a full WPF command system.
// -----------------------------------------------------------------------------

using System.Collections.Generic;

namespace System.Windows.Input
{
    /// <summary>WPF-compatible collection of <see cref="InputGesture"/> objects.</summary>
    public class InputGestureCollection : List<InputGesture>
    {
        /// <summary>Initializes an empty collection.</summary>
        public InputGestureCollection()
        {
        }

        /// <summary>Initializes the collection with the given gestures.</summary>
        /// <param name="gestures">The gestures to seed the collection with.</param>
        public InputGestureCollection(IEnumerable<InputGesture> gestures) : base(gestures)
        {
        }
    }

    /// <summary>
    /// WPF-compatible routed command. Carries a name and an <see cref="InputGestures"/> collection.
    /// Execution is inert in the shim; the control phase supplies real behavior where required.
    /// </summary>
    public class RoutedCommand : ICommand
    {
        /// <summary>Gets the command's name.</summary>
        public string Name { get; }

        /// <summary>Gets the type that owns this command.</summary>
        public Type? OwnerType { get; }

        /// <summary>Gets the input gestures associated with this command.</summary>
        public InputGestureCollection InputGestures { get; } = new InputGestureCollection();

        /// <summary>Initializes a new unnamed routed command.</summary>
        public RoutedCommand() : this(string.Empty, null)
        {
        }

        /// <summary>Initializes a new routed command with a name and owner type.</summary>
        /// <param name="name">The command name.</param>
        /// <param name="ownerType">The owning type.</param>
        public RoutedCommand(string name, Type? ownerType)
        {
            Name = name;
            OwnerType = ownerType;
        }

        /// <summary>Initializes a new routed command with a name, owner type, and gestures.</summary>
        /// <param name="name">The command name.</param>
        /// <param name="ownerType">The owning type.</param>
        /// <param name="inputGestures">The input gestures to associate.</param>
        public RoutedCommand(string name, Type? ownerType, InputGestureCollection inputGestures)
            : this(name, ownerType)
        {
            InputGestures = inputGestures;
        }

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        /// <inheritdoc />
        public virtual bool CanExecute(object? parameter) => true;

        /// <inheritdoc />
        public virtual void Execute(object? parameter)
        {
        }

        /// <summary>Raises <see cref="CanExecuteChanged"/>.</summary>
        protected void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// WPF-compatible routed UI command. Adds a user-visible <see cref="Text"/> to
    /// <see cref="RoutedCommand"/>.
    /// </summary>
    public class RoutedUICommand : RoutedCommand
    {
        /// <summary>Gets or sets the descriptive text for the command.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Initializes a new unnamed UI command.</summary>
        public RoutedUICommand()
        {
        }

        /// <summary>Initializes a new UI command with text, name, and owner type.</summary>
        /// <param name="text">The descriptive text.</param>
        /// <param name="name">The command name.</param>
        /// <param name="ownerType">The owning type.</param>
        public RoutedUICommand(string text, string name, Type? ownerType) : base(name, ownerType)
        {
            Text = text;
        }

        /// <summary>Initializes a new UI command with text, name, owner type, and gestures.</summary>
        /// <param name="text">The descriptive text.</param>
        /// <param name="name">The command name.</param>
        /// <param name="ownerType">The owning type.</param>
        /// <param name="inputGestures">The input gestures to associate.</param>
        public RoutedUICommand(string text, string name, Type? ownerType, InputGestureCollection inputGestures)
            : base(name, ownerType, inputGestures)
        {
            Text = text;
        }
    }

    /// <summary>
    /// WPF-compatible subset of <c>ApplicationCommands</c>, exposing only the standard
    /// commands Nodify references, pre-seeded with their conventional key gestures.
    /// </summary>
    public static class ApplicationCommands
    {
        /// <summary>The Select-All command, seeded with the conventional Ctrl+A gesture.</summary>
        public static RoutedUICommand SelectAll { get; } = CreateSelectAll();

        private static RoutedUICommand CreateSelectAll()
        {
            var command = new RoutedUICommand("Select All", nameof(SelectAll), typeof(ApplicationCommands));
            command.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
            return command;
        }
    }
}
