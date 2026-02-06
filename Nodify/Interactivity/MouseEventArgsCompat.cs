using Avalonia.Input;
using Avalonia.Interactivity;

namespace Nodify.Interactivity
{
    // Compatibility types to bridge WPF-style mouse events to Avalonia pointer events
    // These allow existing code to compile with minimal changes

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
    }

    public class MouseButtonEventArgs : MouseEventArgs
    {
        public MouseButtonEventArgs() { }
        
        public MouseButtonEventArgs(PointerPressedEventArgs pointerArgs) : base(pointerArgs)
        {
        }
        
        public MouseButtonEventArgs(PointerReleasedEventArgs pointerArgs)
        {
            RoutedEvent = pointerArgs.RoutedEvent;
            Source = pointerArgs.Source;
            Handled = pointerArgs.Handled;
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
    }
}
