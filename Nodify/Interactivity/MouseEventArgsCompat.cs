using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    // Compatibility types to bridge WPF-style mouse events to Avalonia pointer events
    // These allow existing code to compile with minimal changes

    public enum MouseButtonState
    {
        Released,
        Pressed
    }

    public enum MouseButton
    {
        Left,
        Middle,
        Right,
        XButton1,
        XButton2
    }

    public class MouseEventArgs : RoutedEventArgs
    {
        public MouseEventArgs() { }

        public MouseEventArgs(PointerEventArgs pointerArgs)
        {
            RoutedEvent = pointerArgs.RoutedEvent;
            Source = pointerArgs.Source;
            Handled = pointerArgs.Handled;
            PointerEventArgs = pointerArgs;
        }

        internal PointerEventArgs? PointerEventArgs { get; set; }

        public Point GetPosition(IInputElement? relativeTo)
        {
            if (PointerEventArgs != null && relativeTo is Visual visual)
            {
                return PointerEventArgs.GetPosition(visual);
            }
            return default;
        }
    }

    public class MouseButtonEventArgs : MouseEventArgs
    {
        public MouseButtonEventArgs() { }

        public MouseButtonEventArgs(PointerPressedEventArgs pointerArgs) : base(pointerArgs)
        {
            ButtonState = MouseButtonState.Pressed;
            ChangedButton = GetMouseButton(pointerArgs.GetCurrentPoint(null).Properties.PointerUpdateKind);
        }

        public MouseButtonEventArgs(PointerReleasedEventArgs pointerArgs)
        {
            RoutedEvent = pointerArgs.RoutedEvent;
            Source = pointerArgs.Source;
            Handled = pointerArgs.Handled;
            ButtonState = MouseButtonState.Released;
            // Avalonia doesn't have InitialPressMouseButton, so we try to infer from the properties
            var props = pointerArgs.GetCurrentPoint(null).Properties;
            ChangedButton = props.IsLeftButtonPressed ? MouseButton.Left :
                           props.IsRightButtonPressed ? MouseButton.Right :
                           props.IsMiddleButtonPressed ? MouseButton.Middle :
                           MouseButton.Left;
        }

        public MouseButtonState ButtonState { get; set; }
        public MouseButton ChangedButton { get; set; }

        private MouseButton GetMouseButton(PointerUpdateKind kind)
        {
            return kind switch
            {
                PointerUpdateKind.LeftButtonPressed or PointerUpdateKind.LeftButtonReleased => MouseButton.Left,
                PointerUpdateKind.RightButtonPressed or PointerUpdateKind.RightButtonReleased => MouseButton.Right,
                PointerUpdateKind.MiddleButtonPressed or PointerUpdateKind.MiddleButtonReleased => MouseButton.Middle,
                PointerUpdateKind.XButton1Pressed or PointerUpdateKind.XButton1Released => MouseButton.XButton1,
                PointerUpdateKind.XButton2Pressed or PointerUpdateKind.XButton2Released => MouseButton.XButton2,
                _ => MouseButton.Left
            };
        }
    }

    public class MouseWheelEventArgs : MouseEventArgs
    {
        public MouseWheelEventArgs() { }

        public MouseWheelEventArgs(PointerWheelEventArgs pointerArgs) : base(pointerArgs)
        {
            Delta = pointerArgs.Delta.Y;
        }

        public double Delta { get; set; }
        public MouseButtonState MiddleButton { get; set; } = MouseButtonState.Released;
    }

    public class KeyEventArgs : RoutedEventArgs
    {
        public KeyEventArgs() { }

        public KeyEventArgs(Avalonia.Input.KeyEventArgs avaloniaArgs, bool isDown)
        {
            RoutedEvent = avaloniaArgs.RoutedEvent;
            Source = avaloniaArgs.Source;
            Handled = avaloniaArgs.Handled;
            IsDown = isDown;
            IsUp = !isDown;
        }

        public bool IsDown { get; set; }
        public bool IsUp { get; set; }
    }
}
