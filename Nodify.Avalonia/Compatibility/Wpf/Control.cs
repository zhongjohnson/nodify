using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using System.Windows.Input;
using AvControl = Avalonia.Controls.Primitives.TemplatedControl;
using AvKeyEventArgs = Avalonia.Input.KeyEventArgs;
using AvPointerEventArgs = Avalonia.Input.PointerEventArgs;
using AvPointerPressedEventArgs = Avalonia.Input.PointerPressedEventArgs;
using AvPointerReleasedEventArgs = Avalonia.Input.PointerReleasedEventArgs;
using AvPointerWheelEventArgs = Avalonia.Input.PointerWheelEventArgs;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfMouseWheelEventArgs = System.Windows.Input.MouseWheelEventArgs;

namespace System.Windows.Controls
{
    /// <summary>WPF-compatible control base over Avalonia's <see cref="AvControl"/>.</summary>
    public class Control : AvControl
    {
        /// <summary>WPF default style key dependency property.</summary>
        public static readonly DependencyProperty DefaultStyleKeyProperty = WpfControlServices.DefaultStyleKeyProperty;

        /// <summary>WPF focusable dependency property.</summary>
        public static new readonly DependencyProperty FocusableProperty = WpfControlServices.FocusableProperty;

        private Avalonia.Controls.INameScope? _templateNameScope;

        /// <summary>Initializes input and size bridges.</summary>
        public Control()
        {
            SizeChanged += (_, e) => OnRenderSizeChanged(new SizeChangedInfo(e.NewSize, e.PreviousSize));
            base.Loaded += (_, _) => Loaded?.Invoke(this, new RoutedEventArgs { Source = this });
            base.Unloaded += (_, _) => Unloaded?.Invoke(this, new RoutedEventArgs { Source = this });
        }

        /// <summary>Occurs when the control is attached to the visual tree.</summary>
        public new event RoutedEventHandler? Loaded;

        /// <summary>Occurs when the control is detached from the visual tree.</summary>
        public new event RoutedEventHandler? Unloaded;

        /// <summary>Gets the current rendered size.</summary>
        public Size RenderSize => Bounds.Size;

        /// <summary>Gets whether this control currently has keyboard focus.</summary>
        public bool IsKeyboardFocused => IsFocused;

        /// <summary>Gets whether this control owns pointer capture.</summary>
        public bool IsMouseCaptured => WpfInputBridge.IsMouseCaptured(this);

        /// <summary>Captures the current pointer.</summary>
        public bool CaptureMouse() => WpfInputBridge.CaptureMouse(this);

        /// <summary>Releases pointer capture.</summary>
        public void ReleaseMouseCapture()
        {
            if (IsMouseCaptured)
            {
                InputStateTracker.Pointer?.Capture(null);
            }
        }

        /// <summary>Translates a point into another visual's coordinate space.</summary>
        public Point TranslatePoint(Point point, Visual relativeTo)
            => global::Avalonia.VisualExtensions.TranslatePoint(this, point, relativeTo) ?? default;

        /// <summary>Returns whether this control is an ancestor of the supplied visual.</summary>
        public bool IsAncestorOf(Visual descendant)
            => descendant != null && this.IsVisualAncestorOf(descendant);

        /// <summary>Gets a WPF dependency property value.</summary>
        public object? GetValue(DependencyProperty property) => DependencyPropertyServices.GetValue(this, property);

        /// <summary>Sets a WPF dependency property value.</summary>
        public void SetValue(DependencyProperty property, object? value) => DependencyPropertyServices.SetValue(this, property, value);

        /// <summary>Sets a read-only WPF dependency property value.</summary>
        public void SetValue(DependencyPropertyKey key, object? value) => DependencyPropertyServices.SetValue(this, key, value);

        /// <summary>Sets the current WPF dependency property value.</summary>
        public void SetCurrentValue(DependencyProperty property, object? value) => DependencyPropertyServices.SetCurrentValue(this, property, value);

        /// <summary>Clears a WPF dependency property value.</summary>
        public void ClearValue(DependencyProperty property) => DependencyPropertyServices.ClearValue(this, property);

        /// <inheritdoc />
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            DependencyPropertyServices.OnPropertyChanged(this, change);
        }

        /// <inheritdoc />
        protected sealed override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _templateNameScope = e.NameScope;
            OnApplyTemplate();
        }

        /// <summary>WPF parameterless template-applied hook.</summary>
        public virtual void OnApplyTemplate()
        {
        }

        /// <summary>Resolves a named template child.</summary>
        protected object? GetTemplateChild(string childName)
            => WpfTemplateServices.GetTemplateChild(_templateNameScope, childName);

        /// <summary>WPF render-size-changed hook.</summary>
        protected virtual void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
        }

        /// <summary>WPF mouse-down hook.</summary>
        protected virtual void OnMouseDown(WpfMouseButtonEventArgs e)
        {
        }

        /// <summary>WPF mouse-up hook.</summary>
        protected virtual void OnMouseUp(WpfMouseButtonEventArgs e)
        {
        }

        /// <summary>WPF mouse-move hook.</summary>
        protected virtual void OnMouseMove(WpfMouseEventArgs e)
        {
        }

        /// <summary>WPF mouse-wheel hook.</summary>
        protected virtual void OnMouseWheel(WpfMouseWheelEventArgs e)
        {
        }

        /// <summary>WPF lost-mouse-capture hook.</summary>
        protected virtual void OnLostMouseCapture(WpfMouseEventArgs e)
        {
        }

        /// <summary>WPF key-down hook.</summary>
        protected virtual void OnKeyDown(WpfKeyEventArgs e)
        {
        }

        /// <summary>WPF key-up hook.</summary>
        protected virtual void OnKeyUp(WpfKeyEventArgs e)
        {
        }

        /// <inheritdoc />
        protected override void OnPointerPressed(AvPointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            WpfMouseButtonEventArgs args = WpfInputBridge.CreateMouseButtonEvent(this, e);
            OnMouseDown(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerReleased(AvPointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            WpfMouseButtonEventArgs args = WpfInputBridge.CreateMouseButtonEvent(this, e);
            OnMouseUp(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerMoved(AvPointerEventArgs e)
        {
            base.OnPointerMoved(e);
            WpfMouseEventArgs args = WpfInputBridge.CreateMouseMoveEvent(this, e);
            OnMouseMove(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerWheelChanged(AvPointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            WpfMouseWheelEventArgs args = WpfInputBridge.CreateMouseWheelEvent(this, e);
            OnMouseWheel(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            WpfMouseEventArgs args = WpfInputBridge.CreateLostMouseCaptureEvent(this, e.Pointer, e.Source);
            OnLostMouseCapture(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnKeyDown(AvKeyEventArgs e)
        {
            base.OnKeyDown(e);
            WpfKeyEventArgs args = WpfInputBridge.CreateKeyEvent(e, true);
            OnKeyDown(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnKeyUp(AvKeyEventArgs e)
        {
            base.OnKeyUp(e);
            WpfKeyEventArgs args = WpfInputBridge.CreateKeyEvent(e, false);
            OnKeyUp(args);
            WpfInputBridge.CopyHandled(args, e);
        }
    }
}
