using Nodify.Interactivity;
using System;
using System.Linq;
using Avalonia.Input;
using Avalonia;
using System.Windows.Input;
using Avalonia.Input;

namespace Nodify
{
    /// <summary>
    /// Provides common commands for the <see cref="NodifyEditor"/>.
    /// </summary>
    public static class EditorCommands
    {
        // Note: Avalonia doesn't have RoutedUICommand, so we use simple ICommand implementations
        // In a real scenario, you might want to use ReactiveCommand or implement ICommand

        /// <summary>
        /// Zoom in relative to the editor's viewport center.
        /// </summary>
        public static ICommand ZoomIn { get; } = CreateCommand(
            execute: o => (o as NodifyEditor)?.ZoomIn(),
            canExecute: o => o is NodifyEditor editor && editor.ViewportZoom < editor.MaxViewportZoom);

        /// <summary>
        /// Zoom out relative to the editor's viewport center.
        /// </summary>
        public static ICommand ZoomOut { get; } = CreateCommand(
            execute: o => (o as NodifyEditor)?.ZoomOut(),
            canExecute: o => o is NodifyEditor editor && editor.ViewportZoom > editor.MinViewportZoom);

        /// <summary>
        /// Select all <see cref="ItemContainer"/>s in the <see cref="NodifyEditor"/>.
        /// </summary>
        public static ICommand SelectAll { get; } = CreateCommand(
            execute: o => (o as NodifyEditor)?.SelectAll(),
            canExecute: o => o is NodifyEditor editor && !editor.IsSelecting && editor.CanSelectMultipleItems);

        /// <summary>
        /// Moves the <see cref="NodifyEditor.ViewportLocation"/> to the specified location.
        /// Parameter is a <see cref="Point"/> or a string that can be converted to a point.
        /// </summary>
        public static ICommand BringIntoView { get; } = CreateCommand(
            execute: o =>
            {
                if (o is not (NodifyEditor editor, object parameter)) return;

                switch (parameter)
                {
                    case Point location:
                        editor.BringIntoView(location);
                        break;
                    case string str:
                        if (Point.TryParse(str, out var parsed))
                            editor.BringIntoView(parsed);
                        break;
                    default:
                        editor.ResetViewport();
                        break;
                }
            },
            canExecute: o => o is (NodifyEditor editor, _) && !editor.DisablePanning);

        /// <summary>
        /// Scales the editor's viewport to fit all the <see cref="ItemContainer"/>s if that's possible.
        /// </summary>
        public static ICommand FitToScreen { get; } = CreateCommand(
            execute: o => (o as NodifyEditor)?.FitToScreen(),
            canExecute: o => o is NodifyEditor editor && editor.HasItems);

        /// <summary>
        /// Aligns the <see cref="NodifyEditor.SelectedContainers"/> using the specified alignment method.
        /// Parameter is of type <see cref="Alignment"/> or a string that can be converted to an alignment.
        /// </summary>
        public static ICommand Align { get; } = CreateCommand(
            execute: o =>
            {
                if (o is not (NodifyEditor editor, object parameter)) return;

                Alignment alignment = parameter switch
                {
                    Alignment a => a,
                    string str when Enum.TryParse<Alignment>(str, true, out var a) => a,
                    _ => Alignment.Top
                };

                editor.AlignSelection(alignment, null);
            },
            canExecute: o => o is (NodifyEditor editor, _) && editor.SelectedContainersCount > 1);

        /// <summary>
        /// Locks the position of the <see cref="NodifyEditor.SelectedContainers"/>.
        /// </summary>
        public static ICommand LockSelection { get; } = CreateCommand(
            execute: o => (o as NodifyEditor)?.LockSelection(),
            canExecute: o => o is NodifyEditor editor && editor.SelectedContainers.Any(x => x.IsDraggable));

        /// <summary>
        /// Unlocks the position of the <see cref="NodifyEditor.SelectedContainers"/>.
        /// </summary>
        public static ICommand UnlockSelection { get; } = CreateCommand(
            execute: o => (o as NodifyEditor)?.UnlockSelection(),
            canExecute: o => o is NodifyEditor editor && editor.SelectedContainers.Any(x => !x.IsDraggable));

        private static ICommand CreateCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
            => new RelayCommand(execute, canExecute);

        private class RelayCommand : ICommand
        {
            private readonly Action<object?> _execute;
            private readonly Func<object?, bool>? _canExecute;

            public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

            public void Execute(object? parameter) => _execute(parameter);

            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
