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
using Nodify.Avalonia.Helpers;
#else
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
#endif

namespace Nodify
{
    /// <summary>The default state of the <see cref="ItemContainer"/>.</summary>
    public class ContainerDefaultState : ContainerState
    {
        private bool _canBeDragging;
        private bool _canceled;

        /// <summary>Creates a new instance of the <see cref="ContainerDefaultState"/>.</summary>
        /// <param name="container">The owner of the state.</param>
        public ContainerDefaultState(ItemContainer container) : base(container)
        {
        }

        /// <inheritdoc />
        public override void ReEnter(ContainerState from)
        {
            if (from is ContainerDraggingState drag)
            {
                Container.IsSelected = true;
                _canceled = drag.Canceled;
            }

            _canBeDragging = false;
        }

        /// <inheritdoc />
        public override void HandleMouseDown(MouseButtonEventArgs e)
        {
            _canceled = false;

            EditorGestures.ItemContainerGestures gestures = EditorGestures.Mappings.ItemContainer;
            if (gestures.Drag.Matches(e.Source, e))
            {
                _canBeDragging = Container.IsDraggable;

                // Clear the selection if dragging an item that is not part of the selection will not add it to the selection
                if (_canBeDragging && !Container.IsSelected && !gestures.Selection.Append.Matches(e.Source, e) && !gestures.Selection.Invert.Matches(e.Source, e))
                {
                    Editor.UnselectAll();
                }
            }
        }

        /// <inheritdoc />
        public override void HandleMouseUp(MouseButtonEventArgs e)
        {
            EditorGestures.ItemContainerGestures gestures = EditorGestures.Mappings.ItemContainer;
            if (!_canceled && gestures.Selection.Select.Matches(e.Source, e))
            {
                if (gestures.Selection.Append.Matches(e.Source, e))
                {
                    Container.IsSelected = true;
                }
                else if (gestures.Selection.Invert.Matches(e.Source, e))
                {
                    Container.IsSelected = !Container.IsSelected;
                }
                else if (gestures.Selection.Remove.Matches(e.Source, e))
                {
                    Container.IsSelected = false;
                }
                else
                {
                    // Allow context menu on selection
#if Avalonia
                    if (e.GetPointerUpdateKind() != PointerUpdateKind.RightButtonReleased || !Container.IsSelected)
#else
                    if (!(e.ChangedButton == MouseButton.Right && e.RightButton == MouseButtonState.Released) || !Container.IsSelected)
#endif
                    {
                        Editor.UnselectAll();
                    }

                    Container.IsSelected = true;
                }

                _canBeDragging = false;
            }

            if(!_canceled && gestures.Drag.Matches(e.Source, e))
            {
                _canBeDragging = false;
            }

            _canceled = false;
        }

        /// <inheritdoc />
        public override void HandleMouseMove(MouseEventArgs e)
        {
            if (_canBeDragging)
            {
                PushState(new ContainerDraggingState(Container));
            }
        }
    }
}
