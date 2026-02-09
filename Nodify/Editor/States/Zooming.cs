using System;
using Avalonia.Input;

namespace Nodify.Interactivity
{
    public static partial class EditorState
    {
        /// <summary>
        /// Represents the zooming state of the <see cref="NodifyEditor"/>.
        /// This state handles zooming operations using the mouse wheel with an optional modifier key.
        /// </summary>
        public class Zooming : InputElementState<NodifyEditor>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Zooming"/> class.
            /// </summary>
            /// <param name="editor">The <see cref="NodifyEditor"/> associated with this state.</param>
            public Zooming(NodifyEditor editor) : base(editor)
            {
            }

            protected override void OnMouseWheel(MouseWheelEventArgs e)
            {
                EditorGestures.NodifyEditorGestures gestures = EditorGestures.Mappings.Editor;

                // Avalonia MouseWheelEventArgs doesn't have KeyModifiers, get from event base
                // For now, check modifier keys differently or skip the check
                // TODO: Get modifiers from input manager or key state tracking
                var expectedModifiers = gestures.ZoomModifierKey;
                bool modifiersMatch = (expectedModifiers == System.Windows.Input.ModifierKeys.None); // Simplified

                if (modifiersMatch && IsZoomingAllowed())
                {
                    // MouseWheelEventArgs.Delta is double, not Vector
                    double zoom = Math.Pow(2.0, e.Delta / 3.0 / 120.0);
                    Element.ZoomAtPosition(zoom, Element.MouseLocation);
                    e.Handled = true;
                }
            }

            private static bool CheckModifierKeys(Avalonia.Input.KeyModifiers actual, System.Windows.Input.ModifierKeys expected)
            {
                // Simplified check - convert between ModifierKeys enums
                if (expected == System.Windows.Input.ModifierKeys.None)
                    return actual == Avalonia.Input.KeyModifiers.None;

                return true; // TODO: Proper conversion
            }

            private bool IsZoomingAllowed()
            {
                return !Element.DisableZooming
                    && (AllowZoomingWhileSelecting || !Element.IsSelecting)
                    && (AllowZoomingWhileCutting || !Element.IsCutting)
                    && (AllowZoomingWhilePushingItems || !Element.IsPushingItems)
                    && (AllowZoomingWhilePanning || !Element.IsPanning);
            }
        }
    }
}
