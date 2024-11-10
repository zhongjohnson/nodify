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
using static Nodify.SelectionHelper;

namespace Nodify
{
    /// <summary>The selecting state of the editor.</summary>
    public class EditorSelectingState : EditorState
    {
        private readonly SelectionType _type;
        private bool _canceled;

        /// <summary>The selection helper.</summary>
        protected SelectionHelper Selection { get; }

        /// <summary>Constructs an instance of the <see cref="EditorSelectingState"/> state.</summary>
        /// <param name="editor">The owner of the state.</param>
        /// <param name="type">The selection strategy.</param>
        public EditorSelectingState(NodifyEditor editor, SelectionType type) : base(editor)
        {
            Selection = new SelectionHelper(editor);
            _type = type;
        }

        /// <inheritdoc />
        public override void Enter(EditorState? from)
        {
            Editor.UnselectAllConnection();

            _canceled = false;
            Selection.Start(Editor.MouseLocation, _type);
        }

        /// <inheritdoc />
        public override void Exit()
        {
            if (_canceled)
            {
                Selection.Abort();
            }
            else
            {
                Selection.End();
            }
        }

        /// <inheritdoc />
        public override void HandleMouseMove(MouseEventArgs e)
            => Selection.Update(Editor.MouseLocation);

        /// <inheritdoc />
#if Avalonia
        public override void HandleMouseDown(PointerPressedEventArgs e)
#else
        public override void HandleMouseDown(MouseButtonEventArgs e)
#endif
        {
            if (!Editor.DisablePanning && EditorGestures.Mappings.Editor.Pan.Matches(e.Source, e))
            {
                PushState(new EditorPanningState(Editor));
            }
        }

        /// <inheritdoc />
#if Avalonia
        public override void HandleMouseUp(PointerReleasedEventArgs e)
#else
        public override void HandleMouseUp(MouseButtonEventArgs e)
#endif
        {
            EditorGestures.SelectionGestures gestures = EditorGestures.Mappings.Editor.Selection;

            bool canCancel = gestures.Cancel.Matches(e.Source, e);
            bool canComplete = gestures.Select.Matches(e.Source, e);
            if (canCancel || canComplete)
            {
                _canceled = !canComplete && canCancel;
                PopState();
            }
        }

        /// <inheritdoc />
        public override void HandleAutoPanning(MouseEventArgs e)
            => HandleMouseMove(e);

        public override void HandleKeyUp(KeyEventArgs e)
        {
            if (EditorGestures.Mappings.Editor.Selection.Cancel.Matches(e.Source, e))
            {
                _canceled = true;
                PopState();
            }
        }
    }
}
