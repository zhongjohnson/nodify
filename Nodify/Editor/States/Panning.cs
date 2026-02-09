using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    public static partial class EditorState
    {
        /// <summary>
        /// Represents the panning state of the <see cref="NodifyEditor"/>, allowing the user to pan the viewport by clicking and dragging.
        /// </summary>
        public class Panning : DragState<NodifyEditor>
        {
            protected override bool HasContextMenu => Element.HasContextMenu;
            protected override bool CanBegin => IsPanningAllowed();
            protected override bool CanCancel => NodifyEditor.AllowPanningCancellation;
            protected override bool IsToggle => EnableToggledPanningMode;


            private Point _prevPosition;

            /// <summary>
            /// Initializes a new instance of the <see cref="Panning"/> class.
            /// </summary>
            /// <param name="editor">The <see cref="NodifyEditor"/> associated with this state.</param>
            public Panning(NodifyEditor editor)
                : base(editor, EditorGestures.Mappings.Editor.Pan, EditorGestures.Mappings.Editor.CancelAction)
            {
            }

            protected override void OnBegin(RoutedEventArgs e)
            {
                // Avalonia doesn't have static Mouse.GetPosition - use element center as starting point
                _prevPosition = new Point(Element.Bounds.Width / 2, Element.Bounds.Height / 2);
                Element.BeginPanning();
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                var currentMousePosition = e.GetPosition(Element);
                Element.UpdatePanning((currentMousePosition - _prevPosition) / Element.ViewportZoom);
                _prevPosition = currentMousePosition;
            }

            protected override void OnEnd(RoutedEventArgs e)
                => Element.EndPanning();

            protected override void OnCancel(RoutedEventArgs e)
                => Element.CancelPanning();

            private bool IsPanningAllowed()
            {
                return !Element.DisablePanning
                    && (AllowPanningWhileSelecting || !Element.IsSelecting)
                    && (AllowPanningWhileCutting || !Element.IsCutting)
                    && (AllowPanningWhilePushingItems || !Element.IsPushingItems);
            }
        }

        /// <summary>
        /// Represents the panning state of the <see cref="NodifyEditor"/> using the mouse wheel.
        /// Allows the user to pan horizontally or vertically by holding modifier keys while scrolling the mouse wheel.
        /// </summary>
        public class PanningWithMouseWheel : InputElementState<NodifyEditor>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PanningWithMouseWheel"/> class.
            /// </summary>
            /// <param name="editor">The <see cref="NodifyEditor"/> associated with this state.</param>
            public PanningWithMouseWheel(NodifyEditor editor) : base(editor)
            {
            }

            protected override void OnMouseWheel(MouseWheelEventArgs e)
            {
                EditorGestures.NodifyEditorGestures gestures = EditorGestures.Mappings.Editor;

                // TODO: Get actual key modifiers from the event or track them
                var currentModifiers = Avalonia.Input.KeyModifiers.None; // Placeholder

                if (gestures.PanWithMouseWheel && currentModifiers == ConvertToAvaloniaModifiers(gestures.PanHorizontalModifierKey))
                {
                    double offset = Math.Sign(e.Delta) * 120.0 / 2 / Element.ViewportZoom; // 120 is standard mouse wheel delta
                    Element.UpdatePanning(new Vector(offset, 0d));
                    e.Handled = true;
                }
                else if (gestures.PanWithMouseWheel && currentModifiers == ConvertToAvaloniaModifiers(gestures.PanVerticalModifierKey))
                {
                    double offset = Math.Sign(e.Delta) * 120.0 / 2 / Element.ViewportZoom;
                    Element.UpdatePanning(new Vector(0d, offset));
                    e.Handled = true;
                }
            }

            private Avalonia.Input.KeyModifiers ConvertToAvaloniaModifiers(System.Windows.Input.ModifierKeys modifiers)
            {
                var result = Avalonia.Input.KeyModifiers.None;

                if ((modifiers & System.Windows.Input.ModifierKeys.Control) != 0)
                    result |= Avalonia.Input.KeyModifiers.Control;
                if ((modifiers & System.Windows.Input.ModifierKeys.Shift) != 0)
                    result |= Avalonia.Input.KeyModifiers.Shift;
                if ((modifiers & System.Windows.Input.ModifierKeys.Alt) != 0)
                    result |= Avalonia.Input.KeyModifiers.Alt;
                if ((modifiers & System.Windows.Input.ModifierKeys.Windows) != 0)
                    result |= Avalonia.Input.KeyModifiers.Meta;

                return result;
            }
        }
    }
}
