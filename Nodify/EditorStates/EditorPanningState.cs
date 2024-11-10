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
    /// <summary>The panning state of the editor.</summary>
    public class EditorPanningState : EditorState
    {
        private Point _initialMousePosition;
        private Point _previousMousePosition;
        private Point _currentMousePosition;

        /// <summary>Constructs an instance of the <see cref="EditorPanningState"/> state.</summary>
        /// <param name="editor">The owner of the state.</param>
        public EditorPanningState(NodifyEditor editor) : base(editor)
        {
        }

        /// <inheritdoc />
        public override void Exit()
            => Editor.IsPanning = false;

        /// <inheritdoc />
        public override void Enter(EditorState? from)
        {
#if Avalonia
            _initialMousePosition = CurrentPointerArgs.GetPosition(Editor);
#else
            _initialMousePosition = Mouse.GetPosition(Editor);
#endif
            _previousMousePosition = _initialMousePosition;
            _currentMousePosition = _initialMousePosition;
            Editor.IsPanning = true;
        }

        /// <inheritdoc />
#if Avalonia
        public override void HandleMouseMove(PointerEventArgs e)
        {
            base.HandleMouseMove(e);
#else
        public override void HandleMouseMove(MouseEventArgs e)
        {
#endif
            _currentMousePosition = e.GetPosition(Editor);
            Editor.ViewportLocation -= (_currentMousePosition - _previousMousePosition) / Editor.ViewportZoom;
            _previousMousePosition = _currentMousePosition;
        }

        /// <inheritdoc />
#if Avalonia
        public override void HandleMouseUp(PointerReleasedEventArgs e)
#else
        public override void HandleMouseUp(MouseButtonEventArgs e)
#endif
        {
            EditorGestures.NodifyEditorGestures gestures = EditorGestures.Mappings.Editor;
            if (gestures.Pan.Matches(e.Source, e))
            {
                // Handle right click if panning and moved the mouse more than threshold so context menu doesn't open
#if Avalonia
                if (e.InitialPressMouseButton == MouseButton.Right)
#else
                if (e.ChangedButton == MouseButton.Right)
#endif
                {
                    double contextMenuTreshold = NodifyEditor.HandleRightClickAfterPanningThreshold * NodifyEditor.HandleRightClickAfterPanningThreshold;
#if Avalonia
                    if (_currentMousePosition.VectorSubtract(_initialMousePosition).LengthSquared() > contextMenuTreshold)
#else
                    if ((_currentMousePosition - _initialMousePosition).LengthSquared > contextMenuTreshold)
#endif
                    {
                        e.Handled = true;
                    }
                }

                PopState();
            }
            else if (gestures.Selection.Select.Matches(e.Source, e) && Editor.IsSelecting)
            {
                PopState();
                // Cancel selection and continue panning
                if (Editor.State is EditorSelectingState && !Editor.DisablePanning)
                {
                    PopState();
                    PushState(new EditorPanningState(Editor));
                }
            }
        }
    }
}
