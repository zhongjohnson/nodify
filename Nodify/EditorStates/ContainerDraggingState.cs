#if Avalonia
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Nodify.Avalonia.Helpers;
using Nodify.Avalonia.Extensions;
using Nodify.Avalonia.EditorStates;
using MouseButtonEventArgs = Avalonia.Input.PointerEventArgs;
using MouseEventArgs = Avalonia.Input.PointerEventArgs;
using MouseWheelEventArgs = Avalonia.Input.PointerWheelEventArgs;
using RoutedEventHandler = System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>;
using DragStartedEventHandler = System.EventHandler<Avalonia.Input.VectorEventArgs>;
using DragDeltaEventHandler = System.EventHandler<Avalonia.Input.VectorEventArgs>;
using DragCompletedEventHandler = System.EventHandler<Nodify.Avalonia.EditorStates.DragCompletedEventArgs>;
using DragDeltaEventArgs = Avalonia.Input.VectorEventArgs;
using DragStartedEventArgs = Avalonia.Input.VectorEventArgs;
#else
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
#endif

namespace Nodify
{
    /// <summary>Dragging state of the container.</summary>
    public class ContainerDraggingState : ContainerState
    {
        private Point _initialMousePosition;
        private Point _previousMousePosition;
        private Point _currentMousePosition;
        public bool Canceled { get; set; } = ItemContainer.AllowDraggingCancellation;   // Because of LostMouseCapture that calls Exit

        /// <summary>Constructs an instance of the <see cref="ContainerDraggingState"/> state.</summary>
        /// <param name="container">The owner of the state.</param>
        public ContainerDraggingState(ItemContainer container) : base(container)
        {
        }

        /// <inheritdoc />
        public override void Enter(ContainerState? from)
        {
#if Avalonia
            _initialMousePosition = Editor.State.CurrentPointerArgs.GetPosition(Editor.ItemsHost);
#else
            _initialMousePosition = Mouse.GetPosition(Editor.ItemsHost);
#endif

            Container.IsSelected = true;
            Container.IsPreviewingLocation = true;
#if Avalonia
            Container.RaiseEvent(new DragStartedEventArgs
            {
                Vector = new(_initialMousePosition.X, _initialMousePosition.Y),
                RoutedEvent = ItemContainer.DragStartedEvent
            });
#else
            Container.RaiseEvent(new DragStartedEventArgs(_initialMousePosition.X, _initialMousePosition.Y)
            {
                RoutedEvent = ItemContainer.DragStartedEvent
            });
#endif
            _previousMousePosition = _initialMousePosition;
        }

        /// <inheritdoc />
        public override void Exit()
        {
            Container.IsPreviewingLocation = false;
            var delta = _currentMousePosition - _initialMousePosition;
#if Avalonia
            Container.RaiseEvent(new DragCompletedEventArgs
            {
                Vector = new(delta.X, delta.Y),
                Canceled = Canceled,
                RoutedEvent = ItemContainer.DragCompletedEvent
            });
#else
            Container.RaiseEvent(new DragCompletedEventArgs(delta.X, delta.Y, Canceled)
            {
                RoutedEvent = ItemContainer.DragCompletedEvent
            });
#endif
        }

        /// <inheritdoc />
#if Avalonia
        public override void HandleMouseMove(PointerEventArgs e)
#else
        public override void HandleMouseMove(MouseEventArgs e)
#endif 
        {
            _currentMousePosition = e.GetPosition(Editor.ItemsHost);
            var delta = _currentMousePosition - _previousMousePosition;
#if Avalonia
            Container.RaiseEvent(new DragDeltaEventArgs
            {
                Vector = new(delta.X, delta.Y),
                RoutedEvent = ItemContainer.DragDeltaEvent
            });
#else
            Container.RaiseEvent(new DragDeltaEventArgs(delta.X, delta.Y)
            {
                RoutedEvent = ItemContainer.DragDeltaEvent
            });
#endif

            _previousMousePosition = _currentMousePosition;
        }

        /// <inheritdoc />
#if Avalonia
        public override void HandleMouseUp(PointerReleasedEventArgs e)
#else
        public override void HandleMouseUp(MouseButtonEventArgs e)
#endif
        {
            EditorGestures.ItemContainerGestures gestures = EditorGestures.Mappings.ItemContainer;

            bool canCancel = gestures.CancelAction.Matches(e.Source, e) && ItemContainer.AllowDraggingCancellation;
            bool canComplete = gestures.Drag.Matches(e.Source, e);
            if (canCancel || canComplete)
            {
                // Prevent canceling if drag and cancel are bound to the same mouse action
                Canceled = !canComplete && canCancel;

#if Avalonia
                // Handle right click if dragging or canceled and moved the mouse more than threshold so context menus don't open
                if (e.GetChangedButton() == MouseButton.Right)
                {
                    double contextMenuTreshold = NodifyEditor.HandleRightClickAfterPanningThreshold * NodifyEditor.HandleRightClickAfterPanningThreshold;
                    if (_currentMousePosition.VectorSubtract(_initialMousePosition).SquaredLength > contextMenuTreshold)
                    {
                        e.Handled = true;
                    }
                }
#else
                // Handle right click if dragging or canceled and moved the mouse more than threshold so context menus don't open
                if (e.ChangedButton == MouseButton.Right)
                {
                    double contextMenuTreshold = NodifyEditor.HandleRightClickAfterPanningThreshold * NodifyEditor.HandleRightClickAfterPanningThreshold;
                    if ((_currentMousePosition - _initialMousePosition).LengthSquared > contextMenuTreshold)
                    {
                        e.Handled = true;
                    }
                }
#endif

                PopState();
            }
        }

        /// <inheritdoc />
        public override void HandleKeyUp(KeyEventArgs e)
        {
            Canceled = EditorGestures.Mappings.ItemContainer.CancelAction.Matches(e.Source, e) && ItemContainer.AllowDraggingCancellation;
            if (Canceled)
            {
                PopState();
            }
        }
    }
}
