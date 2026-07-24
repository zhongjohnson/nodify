using Avalonia.Controls;
using Avalonia.Input;
using System.Windows;
using AvKeyEventArgs = Avalonia.Input.KeyEventArgs;
using AvPointerEventArgs = Avalonia.Input.PointerEventArgs;
using AvPointerPressedEventArgs = Avalonia.Input.PointerPressedEventArgs;
using AvPointerReleasedEventArgs = Avalonia.Input.PointerReleasedEventArgs;
using AvPointerWheelEventArgs = Avalonia.Input.PointerWheelEventArgs;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfMouseWheelEventArgs = System.Windows.Input.MouseWheelEventArgs;

namespace System.Windows.Input
{
    internal static class WpfInputBridge
    {
        public static WpfMouseButtonEventArgs CreateMouseButtonEvent(Control owner, AvPointerPressedEventArgs source)
        {
            UpdatePointerState(owner, source);
            MouseButton button = GetChangedButton(source.GetCurrentPoint(owner).Properties.PointerUpdateKind);
            InputStateTracker.UpdateButton(button, MouseButtonState.Pressed);

            var result = new WpfMouseButtonEventArgs(source)
            {
                RoutedEvent = UIElement.MouseDownEvent,
                ChangedButton = button,
                ButtonState = MouseButtonState.Pressed,
                ClickCount = source.ClickCount
            };
            result.UpdateButtonStatesFromGlobal();
            return result;
        }

        public static WpfMouseButtonEventArgs CreateMouseButtonEvent(Control owner, AvPointerReleasedEventArgs source)
        {
            UpdatePointerState(owner, source);
            MouseButton button = GetChangedButton(source.GetCurrentPoint(owner).Properties.PointerUpdateKind);
            InputStateTracker.UpdateButton(button, MouseButtonState.Released);

            var result = new WpfMouseButtonEventArgs(source)
            {
                RoutedEvent = UIElement.MouseUpEvent,
                ChangedButton = button,
                ButtonState = MouseButtonState.Released
            };
            result.UpdateButtonStatesFromGlobal();
            return result;
        }

        public static WpfMouseEventArgs CreateMouseMoveEvent(Control owner, AvPointerEventArgs source)
        {
            UpdatePointerState(owner, source);
            var result = new WpfMouseEventArgs(source) { RoutedEvent = UIElement.MouseMoveEvent };
            result.UpdateButtonStatesFromGlobal();
            return result;
        }

        public static WpfMouseWheelEventArgs CreateMouseWheelEvent(Control owner, AvPointerWheelEventArgs source)
        {
            UpdatePointerState(owner, source);
            var result = new WpfMouseWheelEventArgs(source)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Delta = (int)(source.Delta.Y * Mouse.MouseWheelDeltaForOneLine)
            };
            result.UpdateButtonStatesFromGlobal();
            return result;
        }

        public static WpfMouseEventArgs CreateLostMouseCaptureEvent(Control owner, IPointer pointer, object? source)
        {
            InputStateTracker.UpdatePointer(pointer, owner);
            var result = new WpfMouseEventArgs
            {
                RoutedEvent = UIElement.LostMouseCaptureEvent,
                Source = source ?? owner
            };
            result.UpdateButtonStatesFromGlobal();
            return result;
        }

        public static WpfKeyEventArgs CreateKeyEvent(AvKeyEventArgs source, bool isDown)
        {
            InputManager.Current.MostRecentInputDevice = new KeyboardDevice();
            InputStateTracker.UpdateModifiers(source.KeyModifiers);
            if (isDown)
            {
                InputStateTracker.SetKeyDown(source.Key);
            }
            else
            {
                InputStateTracker.SetKeyUp(source.Key);
            }

            return new WpfKeyEventArgs(source, isDown)
            {
                RoutedEvent = isDown ? UIElement.KeyDownEvent : UIElement.KeyUpEvent
            };
        }

        public static void CopyHandled(InputEventArgs source, Avalonia.Interactivity.RoutedEventArgs target)
            => target.Handled = source.Handled;

        public static bool IsMouseCaptured(Control element)
            => ReferenceEquals(InputStateTracker.Pointer?.Captured, element);

        public static bool CaptureMouse(Control element)
        {
            InputStateTracker.Pointer?.Capture(element);
            return IsMouseCaptured(element);
        }

        private static void UpdatePointerState(Control owner, AvPointerEventArgs source)
        {
            InputManager.Current.MostRecentInputDevice = source.Pointer;
            InputStateTracker.UpdateModifiers(source.KeyModifiers);
            InputStateTracker.UpdatePointer(source.Pointer, owner);
            PointerPointProperties properties = source.GetCurrentPoint(owner).Properties;
            InputStateTracker.UpdateButtons(
                properties.IsLeftButtonPressed ? MouseButtonState.Pressed : MouseButtonState.Released,
                properties.IsRightButtonPressed ? MouseButtonState.Pressed : MouseButtonState.Released,
                properties.IsMiddleButtonPressed ? MouseButtonState.Pressed : MouseButtonState.Released);
        }

        private static MouseButton GetChangedButton(PointerUpdateKind updateKind)
            => updateKind switch
            {
                PointerUpdateKind.LeftButtonPressed or PointerUpdateKind.LeftButtonReleased => MouseButton.Left,
                PointerUpdateKind.RightButtonPressed or PointerUpdateKind.RightButtonReleased => MouseButton.Right,
                PointerUpdateKind.MiddleButtonPressed or PointerUpdateKind.MiddleButtonReleased => MouseButton.Middle,
                PointerUpdateKind.XButton1Pressed or PointerUpdateKind.XButton1Released => MouseButton.XButton1,
                PointerUpdateKind.XButton2Pressed or PointerUpdateKind.XButton2Released => MouseButton.XButton2,
                _ => MouseButton.None
            };
    }
}
