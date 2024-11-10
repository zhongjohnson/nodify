using System.Collections.Generic;

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
using FrameworkElement = Avalonia.Controls.Control;
using Control = Avalonia.Controls.Primitives.TemplatedControl;
#else
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
#endif

namespace Nodify
{
    public class EditorCuttingState : EditorState
    {
        private readonly LineGeometry _lineGeometry = new LineGeometry();
        private List<FrameworkElement>? _previousConnections;

        public bool Canceled { get; set; } = CuttingLine.AllowCuttingCancellation;

        public EditorCuttingState(NodifyEditor editor) : base(editor)
        {
        }

        public override void Enter(EditorState? from)
        {
            Canceled = false;

            var startLocation = Editor.MouseLocation;
            Editor.StartCutting(startLocation);

            _lineGeometry.StartPoint = startLocation;
            _lineGeometry.EndPoint = startLocation;
        }

        public override void Exit()
        {
            ResetConnectionStyle();

            // TODO: This is not canceled on LostMouseCapture (add OnLostMouseCapture/OnCancel callback?)
            if (Canceled)
            {
                Editor.CancelCutting();
            }
            else
            {
                Editor.EndCutting(Editor.MouseLocation);
            }
        }

#if Avalonia
        public override void HandleMouseUp(PointerReleasedEventArgs e)
#else
        public override void HandleMouseUp(MouseButtonEventArgs e)
#endif
        {
            EditorGestures.NodifyEditorGestures gestures = EditorGestures.Mappings.Editor;
            if (gestures.Cutting.Matches(e.Source, e))
            {
                PopState();
            }
            else if (CuttingLine.AllowCuttingCancellation && gestures.CancelAction.Matches(e.Source, e))
            {
                Canceled = true;
                e.Handled = true;   // prevents opening context menu

                PopState();
            }
        }

#if Avalonia
        public override void HandleMouseMove(PointerEventArgs e)
#else
        public override void HandleMouseMove(MouseEventArgs e)
#endif
        {
            Editor.CuttingLineEnd = Editor.MouseLocation;

            if (NodifyEditor.EnableCuttingLinePreview)
            {
                ResetConnectionStyle();

                _lineGeometry.EndPoint = Editor.MouseLocation;
                var connections = Editor.ConnectionsHost.GetIntersectingElements(_lineGeometry, NodifyEditor.CuttingConnectionTypes);
                foreach (var connection in connections)
                {
                    CuttingLine.SetIsOverElement(connection, true);
                }

                _previousConnections = connections;
            }
        }

        private void ResetConnectionStyle()
        {
            if (_previousConnections != null)
            {
                foreach (var connection in _previousConnections)
                {
                    CuttingLine.SetIsOverElement(connection, false);
                }
            }
        }

        public override void HandleKeyUp(KeyEventArgs e)
        {
            EditorGestures.NodifyEditorGestures gestures = EditorGestures.Mappings.Editor;
            if (CuttingLine.AllowCuttingCancellation && gestures.CancelAction.Matches(e.Source, e))
            {
                Canceled = true;
                PopState();
            }
        }
    }
}
