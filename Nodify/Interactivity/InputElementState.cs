using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Input;

namespace Nodify.Interactivity
{
    /// <summary>
    /// Represents a base class for handling input events in a specific state for a framework element.
    /// </summary>
    /// <typeparam name="TElement">The type of the visual element that owns this state.</typeparam>
    public abstract class InputElementState<TElement> : IInputHandler
        where TElement : Visual
    {
        /// <summary>
        /// Gets the owner of the state.
        /// </summary>
        protected TElement Element { get; }

        public bool RequiresInputCapture { get; protected set; }
        public bool ProcessHandledEvents { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputElementState{TElement}"/> class.
        /// </summary>
        /// <param name="element">The framework element that owns this state.</param>
        protected InputElementState(TElement element)
        {
            Element = element;
        }

        /// <inheritdoc cref="Control.OnMouseDown(MouseButtonEventArgs)"/>
        protected virtual void OnMouseDown(MouseButtonEventArgs e) { }

        /// <inheritdoc cref="Control.OnMouseUp(MouseButtonEventArgs)"/>
        protected virtual void OnMouseUp(MouseButtonEventArgs e) { }

        /// <inheritdoc cref="Control.OnMouseMove(MouseEventArgs)"/>
        protected virtual void OnMouseMove(MouseEventArgs e) { }

        /// <inheritdoc cref="Control.OnMouseWheel(MouseWheelEventArgs)"/>
        protected virtual void OnMouseWheel(MouseWheelEventArgs e) { }

        /// <inheritdoc cref="Control.OnKeyUp(KeyEventArgs)"/>
        protected virtual void OnKeyUp(KeyEventArgs e) { }

        /// <inheritdoc cref="Control.OnKeyDown(KeyEventArgs)"/>
        protected virtual void OnKeyDown(KeyEventArgs e) { }

        /// <inheritdoc cref="Control.OnLostMouseCapture(MouseEventArgs)"/>
        protected virtual void OnLostMouseCapture(MouseEventArgs e) { }

        /// <summary>
        /// Called for any input event that is not explicitly handled by other methods.
        /// </summary>
        /// <param name="e">The input event arguments.</param>
        protected virtual void OnEvent(RoutedEventArgs e) { }

        /// <summary>
        /// Processes the input event by invoking the appropriate handler method based on the routed event.
        /// </summary>
        /// <param name="e">The input event arguments.</param>
        public void HandleEvent(RoutedEventArgs e)
        {
            if (e.RoutedEvent == InputElement.PointerMovedEvent && e is PointerEventArgs pointerMoveArgs)
            {
                OnMouseMove(CreateMouseEventArgs(pointerMoveArgs));
            }
            else if (e.RoutedEvent == InputElement.PointerPressedEvent && e is PointerPressedEventArgs pointerPressedArgs)
            {
                OnMouseDown(CreateMouseButtonEventArgs(pointerPressedArgs));
            }
            else if (e.RoutedEvent == InputElement.PointerReleasedEvent && e is PointerReleasedEventArgs pointerReleasedArgs)
            {
                OnMouseUp(CreateMouseButtonEventArgs(pointerReleasedArgs));
            }
            else if (e.RoutedEvent == InputElement.PointerWheelChangedEvent && e is PointerWheelEventArgs pointerWheelArgs)
            {
                OnMouseWheel(CreateMouseWheelEventArgs(pointerWheelArgs));
            }
            else if (e.RoutedEvent == InputElement.PointerCaptureLostEvent && e is PointerCaptureLostEventArgs captureLostArgs)
            {
                OnLostMouseCapture(CreateMouseEventArgs(captureLostArgs));
            }
            else if (e.RoutedEvent == InputElement.KeyDownEvent && e is KeyEventArgs keyDownArgs)
            {
                OnKeyDown(keyDownArgs);
            }
            else if (e.RoutedEvent == InputElement.KeyUpEvent && e is KeyEventArgs keyUpArgs)
            {
                OnKeyUp(keyUpArgs);
            }

            OnEvent(e);
        }

        // Helper methods to bridge Avalonia's Pointer events to WPF-style Mouse events
        private MouseEventArgs CreateMouseEventArgs(PointerEventArgs e)
            => new MouseEventArgs { RoutedEvent = e.RoutedEvent, Source = e.Source, Handled = e.Handled };

        private MouseButtonEventArgs CreateMouseButtonEventArgs(PointerPressedEventArgs e)
            => new MouseButtonEventArgs { RoutedEvent = e.RoutedEvent, Source = e.Source, Handled = e.Handled };

        private MouseButtonEventArgs CreateMouseButtonEventArgs(PointerReleasedEventArgs e)
            => new MouseButtonEventArgs { RoutedEvent = e.RoutedEvent, Source = e.Source, Handled = e.Handled };

        private MouseWheelEventArgs CreateMouseWheelEventArgs(PointerWheelEventArgs e)
            => new MouseWheelEventArgs { RoutedEvent = e.RoutedEvent, Source = e.Source, Handled = e.Handled };

        private MouseEventArgs CreateMouseEventArgs(PointerCaptureLostEventArgs e)
            => new MouseEventArgs { RoutedEvent = e.RoutedEvent, Source = e.Source, Handled = e.Handled };
    }
}
