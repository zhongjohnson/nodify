// -----------------------------------------------------------------------------
//  WPF ItemsControl / GroupStyle shims (System.Windows.Controls)
// -----------------------------------------------------------------------------
//  Upstream `Node` declares template parts typed as `ItemsControl` and mirrors its
//  `InputGroupStyle` / `OutputGroupStyle` collections into `ItemsControl.GroupStyle`.
//  Avalonia's `ItemsControl` has no `GroupStyle` (it has no WPF-style item grouping),
//  so this shim derives from Avalonia's `ItemsControl` and adds a WPF-shaped
//  `GroupStyle` collection. The collection is a passive carrier today (grouping is a
//  theme/behavior concern deferred to a later phase); it exists so the upstream
//  `Node` source compiles and mirrors styles without modification.
// -----------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using AvItemsControl = Avalonia.Controls.ItemsControl;

namespace System.Windows.Controls
{
    /// <summary>
    /// WPF-compatible <see cref="GroupStyle"/> placeholder. Upstream <c>Node</c> only stores and mirrors
    /// instances of this type between its group-style collections; item grouping itself is deferred to the
    /// theme/behavior phase, so this carries no runtime behavior yet.
    /// </summary>
    public class GroupStyle
    {
    }

    /// <summary>
    /// WPF-compatible <see cref="ItemsControl"/> over Avalonia's <see cref="Avalonia.Controls.ItemsControl"/>.
    /// Adds the WPF <see cref="GroupStyle"/> collection that upstream <c>Node</c> populates for its input/output
    /// connector item hosts.
    /// </summary>
    public class ItemsControl : AvItemsControl
    {
        /// <summary>WPF default style key dependency property.</summary>
        public static readonly DependencyProperty DefaultStyleKeyProperty = WpfControlServices.DefaultStyleKeyProperty;

        /// <summary>WPF focusable dependency property.</summary>
        public static new readonly DependencyProperty FocusableProperty = WpfControlServices.FocusableProperty;

        /// <summary>Record-only WPF item-container style property.</summary>
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register(nameof(ItemContainerStyle), typeof(Style), typeof(ItemsControl), new FrameworkPropertyMetadata(null));

        private Avalonia.Controls.INameScope? _templateNameScope;

        /// <summary>WPF-style item group styles. A passive carrier today; grouping behavior is deferred.</summary>
        public ObservableCollection<GroupStyle> GroupStyle { get; } = new ObservableCollection<GroupStyle>();

        /// <summary>Gets or sets the WPF item-container style carrier.</summary>
        public Style? ItemContainerStyle
        {
            get => (Style?)GetValue(ItemContainerStyleProperty);
            set => SetValue(ItemContainerStyleProperty, value);
        }

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

        /// <summary>Gets a WPF dependency property value.</summary>
        public object? GetValue(DependencyProperty property) => DependencyPropertyServices.GetValue(this, property);

        /// <summary>Sets a WPF dependency property value.</summary>
        public void SetValue(DependencyProperty property, object? value) => DependencyPropertyServices.SetValue(this, property, value);

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
            => WpfTemplateServices.GetTemplateChild(this, _templateNameScope, childName);

        /// <summary>WPF-style container factory.</summary>
        protected virtual DependencyObject? GetContainerForItemOverride() => null;

        /// <summary>WPF-style test for an item that is its own container.</summary>
        protected virtual bool IsItemItsOwnContainerOverride(object item) => false;

        /// <inheritdoc />
        protected override Avalonia.Controls.Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
            => GetContainerForItemOverride() as Avalonia.Controls.Control
               ?? base.CreateContainerForItemOverride(item, index, recycleKey);

        /// <inheritdoc />
        protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        {
            recycleKey = null;
            return item is null || !IsItemItsOwnContainerOverride(item);
        }

        /// <summary>WPF mouse-down hook.</summary>
        protected virtual void OnMouseDown(global::System.Windows.Input.MouseButtonEventArgs e) { }

        /// <summary>WPF mouse-up hook.</summary>
        protected virtual void OnMouseUp(global::System.Windows.Input.MouseButtonEventArgs e) { }

        /// <summary>WPF mouse-move hook.</summary>
        protected virtual void OnMouseMove(global::System.Windows.Input.MouseEventArgs e) { }

        /// <summary>WPF mouse-wheel hook.</summary>
        protected virtual void OnMouseWheel(global::System.Windows.Input.MouseWheelEventArgs e) { }

        /// <summary>WPF lost-mouse-capture hook.</summary>
        protected virtual void OnLostMouseCapture(global::System.Windows.Input.MouseEventArgs e) { }

        /// <summary>WPF key-down hook.</summary>
        protected virtual void OnKeyDown(global::System.Windows.Input.KeyEventArgs e) { }

        /// <summary>WPF key-up hook.</summary>
        protected virtual void OnKeyUp(global::System.Windows.Input.KeyEventArgs e) { }

        /// <inheritdoc />
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            var args = WpfInputBridge.CreateMouseButtonEvent(this, e);
            OnMouseDown(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            var args = WpfInputBridge.CreateMouseButtonEvent(this, e);
            OnMouseUp(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            var args = WpfInputBridge.CreateMouseMoveEvent(this, e);
            OnMouseMove(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            var args = WpfInputBridge.CreateMouseWheelEvent(this, e);
            OnMouseWheel(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            var args = WpfInputBridge.CreateLostMouseCaptureEvent(this, e.Pointer, e.Source);
            OnLostMouseCapture(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            var args = WpfInputBridge.CreateKeyEvent(e, true);
            OnKeyDown(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnKeyUp(Avalonia.Input.KeyEventArgs e)
        {
            base.OnKeyUp(e);
            var args = WpfInputBridge.CreateKeyEvent(e, false);
            OnKeyUp(args);
            WpfInputBridge.CopyHandled(args, e);
        }
    }
}
