#if Avalonia
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Nodify.Avalonia.Extensions;
using MouseButtonEventArgs = Avalonia.Input.PointerEventArgs;
using MouseEventArgs = Avalonia.Input.PointerEventArgs;
using MouseWheelEventArgs = Avalonia.Input.PointerWheelEventArgs;
#else
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
#endif

namespace Nodify
{
    /// <summary>The base class for editor states.</summary>
    public abstract class EditorState
    {
        /// <summary>Constructs a new <see cref="EditorState"/>.</summary>
        /// <param name="editor">The owner of the state.</param>
        public EditorState(NodifyEditor editor)
        {
            Editor = editor;
        }

        /// <summary>The owner of the state.</summary>
        protected NodifyEditor Editor { get; }

#if Avalonia
        public KeyModifiers CurrentKeyModifiers { get; private set; }

        public PointerEventArgs? CurrentPointerArgs { get; private set; }

        public Key CurrentKey { get; private set; }


        /// <inheritdoc cref="NodifyEditor.OnMouseDown(MouseButtonEventArgs)"/>
        public virtual void HandleMouseDown(PointerPressedEventArgs e)
        {
            CurrentPointerArgs = e;
        }


        /// <inheritdoc cref="NodifyEditor.OnMouseUp(MouseButtonEventArgs)"/>
        public virtual void HandleMouseUp(PointerReleasedEventArgs e)
        {
            CurrentPointerArgs = e;
        }

        /// <inheritdoc cref="NodifyEditor.OnMouseMove(MouseEventArgs)"/>
        public virtual void HandleMouseMove(PointerEventArgs e)
        {
            CurrentPointerArgs = e;
        }

        /// <inheritdoc cref="NodifyEditor.OnMouseWheel(MouseWheelEventArgs)"/>
        public virtual void HandleMouseWheel(PointerWheelEventArgs e)
        {
            CurrentPointerArgs = e;
        }
#else
        /// <inheritdoc cref="NodifyEditor.OnMouseDown(MouseButtonEventArgs)"/>
        public virtual void HandleMouseDown(MouseButtonEventArgs e) { }

        /// <inheritdoc cref="NodifyEditor.OnMouseUp(MouseButtonEventArgs)"/>
        public virtual void HandleMouseUp(MouseButtonEventArgs e) { }

        /// <inheritdoc cref="NodifyEditor.OnMouseMove(MouseEventArgs)"/>
        public virtual void HandleMouseMove(MouseEventArgs e) { }

        /// <inheritdoc cref="NodifyEditor.OnMouseWheel(MouseWheelEventArgs)"/>
        public virtual void HandleMouseWheel(MouseWheelEventArgs e) { }
#endif

        /// <summary>Handles auto panning when mouse is outside the editor.</summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> that contains the event data.</param>
        public virtual void HandleAutoPanning(MouseEventArgs e) { }

        /// <inheritdoc cref="NodifyEditor.OnKeyUp(KeyEventArgs)"/>
        public virtual void HandleKeyUp(KeyEventArgs e)
        {
#if Avalonia
            if (CurrentKey == e.Key)
            {
                CurrentKey = default;
            }

            CurrentKeyModifiers = CurrentKeyModifiers & ~(e.KeyModifiers);
#endif
        }

        /// <inheritdoc cref="NodifyEditor.OnKeyDown(KeyEventArgs)"/>
        public virtual void HandleKeyDown(KeyEventArgs e)
        {
#if Avalonia
            CurrentKey = e.Key;
            CurrentKeyModifiers = e.KeyModifiers;
#endif
        }


        /// <summary>Called when <see cref="NodifyEditor.PushState(EditorState)"/> is called.</summary>
        /// <param name="from">The state we enter from (is null for root state).</param>
        public virtual void Enter(EditorState? from)
        {
#if Avalonia
            if (from != null)
            {
                CurrentPointerArgs = from.CurrentPointerArgs;
            }
#endif
        }

        /// <summary>Called when <see cref="NodifyEditor.PopState"/> is called.</summary>
        public virtual void Exit() { }

        /// <summary>Called when <see cref="NodifyEditor.PopState"/> is called.</summary>
        /// <param name="from">The state we re-enter from.</param>
        public virtual void ReEnter(EditorState from) { }

        /// <summary>Pushes a new state into the stack.</summary>
        /// <param name="newState">The new state.</param>
        public virtual void PushState(EditorState newState) => Editor.PushState(newState);

        /// <summary>Pops the current state from the stack.</summary>
        public virtual void PopState() => Editor.PopState();
    }
}
