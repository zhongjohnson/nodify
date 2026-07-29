// -----------------------------------------------------------------------------
//  WPF Selector / MultiSelector selection-engine shim (Phase 8a)
// -----------------------------------------------------------------------------
//  NodifyEditor's control base chain is:
//      NodifyEditor : ConnectionsMultiSelector : MultiSelector : Selector : ItemsControl
//  WPF's Selector/MultiSelector expose a selection engine that differs in shape from Avalonia's
//  SelectingItemsControl. These shims bridge the two so the upstream selection sources compile and
//  behave:
//    * Selector re-exposes the `IsSelected` attached property, a WPF-style `ItemContainerGenerator`
//      facade (ContainerFromIndex/ContainerFromItem/IndexFromContainer over the Avalonia control),
//      the WPF container-override idiom (GetContainerForItemOverride()/IsItemItsOwnContainerOverride)
//      mapped onto Avalonia's CreateContainerForItemOverride/NeedsContainerOverride, and an
//      OnSelectionChanged(SelectionChangedEventArgs) hook driven by Avalonia's SelectionChanged event.
//    * MultiSelector adds the WPF SelectedItems/CanSelectMultipleItems surface and the
//      Begin/EndUpdateSelectedItems batch markers (no-ops here; Avalonia updates selection eagerly).
//  Both re-expose the WPF value accessors + OnPropertyChanged coercion/change routing (as the other
//  control-base shims do) because they derive from a different Avalonia type than the FrameworkElement
//  shim and so cannot inherit them.
// -----------------------------------------------------------------------------

using System.Collections;
using System.Windows;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvSelectingItemsControl = Avalonia.Controls.Primitives.SelectingItemsControl;
using AvItemsControl = Avalonia.Controls.ItemsControl;
using AvSelectionMode = Avalonia.Controls.SelectionMode;

namespace System.Windows.Controls
{
    /// <summary>
    /// WPF-compatible <c>ItemContainerGenerator</c> facade. Avalonia exposes container lookup on the
    /// <see cref="ItemsControl"/> itself (and its generator lacks <c>ContainerFromItem</c>), so this thin
    /// facade delegates to the owning control, matching the members upstream uses.
    /// </summary>
    public sealed class ItemContainerGenerator
    {
        private readonly AvItemsControl _owner;

        internal ItemContainerGenerator(AvItemsControl owner)
        {
            _owner = owner;
        }

        /// <summary>Returns the container at the given index, or <c>null</c> if not realized.</summary>
        public DependencyObject? ContainerFromIndex(int index)
            => _owner.ContainerFromIndex(index);

        /// <summary>Returns the container for the given item, or <c>null</c> if not realized.</summary>
        public DependencyObject? ContainerFromItem(object item)
            => _owner.ContainerFromItem(item);

        /// <summary>Returns the index of the given container, or <c>-1</c> if not found.</summary>
        public int IndexFromContainer(DependencyObject container)
            => container is Avalonia.Controls.Control control ? _owner.IndexFromContainer(control) : -1;
    }
}

namespace System.Windows.Controls.Primitives
{
    /// <summary>
    /// WPF-compatible <see cref="Selector"/> over Avalonia's
    /// <see cref="Avalonia.Controls.Primitives.SelectingItemsControl"/>.
    /// </summary>
    public class Selector : AvSelectingItemsControl
    {
        /// <summary>Record-only WPF default-style-key property (theming deferred; see <see cref="WpfControlServices"/>).</summary>
        public static readonly DependencyProperty DefaultStyleKeyProperty = WpfControlServices.DefaultStyleKeyProperty;

        /// <summary>WPF focusable property wrapping Avalonia's real <c>Focusable</c>.</summary>
        public static new readonly DependencyProperty FocusableProperty = WpfControlServices.FocusableProperty;

        /// <summary>WPF <c>IsSelected</c> attached property, wrapping Avalonia's <see cref="AvSelectingItemsControl.IsSelectedProperty"/>.</summary>
        public static new readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.FromExisting(AvSelectingItemsControl.IsSelectedProperty, typeof(Selector));

        /// <summary>WPF selected routed event identity.</summary>
        public static readonly RoutedEvent SelectedEvent =
            EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Selector));

        /// <summary>WPF unselected routed event identity.</summary>
        public static readonly RoutedEvent UnselectedEvent =
            EventManager.RegisterRoutedEvent("Unselected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Selector));

        /// <summary>WPF mouse-move routed event identity.</summary>
        public static readonly RoutedEvent MouseMoveEvent = UIElement.MouseMoveEvent;

        /// <summary>Record-only WPF item-container style property.</summary>
        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register(nameof(ItemContainerStyle), typeof(Style), typeof(Selector), new FrameworkPropertyMetadata(null));

        private global::System.Windows.Controls.ItemContainerGenerator? _generator;
        private Avalonia.Controls.INameScope? _templateNameScope;

        /// <summary>WPF-style container generator facade (delegates to this control's Avalonia lookup methods).</summary>
        public new global::System.Windows.Controls.ItemContainerGenerator ItemContainerGenerator
            => _generator ??= new global::System.Windows.Controls.ItemContainerGenerator(this);

        /// <summary>Initializes the selector and bridges Avalonia's selection event to the WPF hook.</summary>
        public Selector()
        {
            SelectionChanged += OnSelectionChangedBridge;
            SizeChanged += (_, e) => OnRenderSizeChanged(new SizeChangedInfo(e.NewSize, e.PreviousSize));
            base.Loaded += (_, _) => Loaded?.Invoke(this, new RoutedEventArgs { Source = this });
            base.Unloaded += (_, _) => Unloaded?.Invoke(this, new RoutedEventArgs { Source = this });
        }

        /// <summary>Occurs when the selector is attached to the visual tree.</summary>
        public new event RoutedEventHandler? Loaded;

        /// <summary>Occurs when the selector is detached from the visual tree.</summary>
        public new event RoutedEventHandler? Unloaded;

        /// <summary>Gets or sets the WPF item-container style carrier.</summary>
        public Style? ItemContainerStyle
        {
            get => (Style?)GetValue(ItemContainerStyleProperty);
            set => SetValue(ItemContainerStyleProperty, value);
        }

        /// <summary>Gets the current rendered size.</summary>
        public Size RenderSize => Bounds.Size;

        /// <summary>Gets the rendered width.</summary>
        public double ActualWidth => Bounds.Width;

        /// <summary>Gets the rendered height.</summary>
        public double ActualHeight => Bounds.Height;

        /// <summary>Gets whether the selector contains any items.</summary>
        public bool HasItems => Items.Count > 0;

        /// <summary>Gets the UI dispatcher.</summary>
        public Avalonia.Threading.Dispatcher Dispatcher => Avalonia.Threading.Dispatcher.UIThread;

        /// <summary>Gets whether this selector currently has keyboard focus.</summary>
        public bool IsKeyboardFocused => IsFocused;

        /// <summary>Gets whether this selector owns pointer capture.</summary>
        public bool IsMouseCaptured => WpfInputBridge.IsMouseCaptured(this);

        /// <summary>Gets whether pointer capture is held within this selector's visual subtree.</summary>
        public bool IsMouseCaptureWithin
            => InputStateTracker.Pointer?.Captured is Visual captured
               && (ReferenceEquals(captured, this) || this.IsVisualAncestorOf(captured));

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

        /// <summary>Returns whether this selector is an ancestor of the supplied visual.</summary>
        public bool IsAncestorOf(Visual descendant)
            => descendant != null && this.IsVisualAncestorOf(descendant);

        /// <summary>Returns whether this selector is an ancestor of the supplied dependency object.</summary>
        public bool IsAncestorOf(DependencyObject descendant)
            => descendant is Visual visual && this.IsVisualAncestorOf(visual);

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

        /// <summary>WPF render-size-changed hook.</summary>
        protected virtual void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
        }

        /// <summary>WPF mouse-down hook.</summary>
        protected virtual void OnMouseDown(global::System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        /// <summary>WPF mouse-up hook.</summary>
        protected virtual void OnMouseUp(global::System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        /// <summary>WPF mouse-move hook.</summary>
        protected virtual void OnMouseMove(global::System.Windows.Input.MouseEventArgs e)
        {
        }

        /// <summary>WPF mouse-wheel hook.</summary>
        protected virtual void OnMouseWheel(global::System.Windows.Input.MouseWheelEventArgs e)
        {
        }

        /// <summary>WPF lost-mouse-capture hook.</summary>
        protected virtual void OnLostMouseCapture(global::System.Windows.Input.MouseEventArgs e)
        {
        }

        /// <summary>WPF key-down hook.</summary>
        protected virtual void OnKeyDown(global::System.Windows.Input.KeyEventArgs e)
        {
        }

        /// <summary>WPF key-up hook.</summary>
        protected virtual void OnKeyUp(global::System.Windows.Input.KeyEventArgs e)
        {
        }

        /// <summary>WPF keyboard-focus-acquired hook.</summary>
        protected virtual void OnGotKeyboardFocus(global::System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
        }

        /// <summary>WPF keyboard-focus-lost hook.</summary>
        protected virtual void OnLostKeyboardFocus(global::System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
        }

        /// <inheritdoc />
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            global::System.Windows.Input.MouseButtonEventArgs args = WpfInputBridge.CreateMouseButtonEvent(this, e);
            OnMouseDown(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            global::System.Windows.Input.MouseButtonEventArgs args = WpfInputBridge.CreateMouseButtonEvent(this, e);
            OnMouseUp(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            global::System.Windows.Input.MouseEventArgs args = WpfInputBridge.CreateMouseMoveEvent(this, e);
            OnMouseMove(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            base.OnPointerWheelChanged(e);
            global::System.Windows.Input.MouseWheelEventArgs args = WpfInputBridge.CreateMouseWheelEvent(this, e);
            OnMouseWheel(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            base.OnPointerCaptureLost(e);
            global::System.Windows.Input.MouseEventArgs args = WpfInputBridge.CreateLostMouseCaptureEvent(this, e.Pointer, e.Source);
            OnLostMouseCapture(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnKeyDown(Avalonia.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            global::System.Windows.Input.KeyEventArgs args = WpfInputBridge.CreateKeyEvent(e, true);
            OnKeyDown(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        /// <inheritdoc />
        protected override void OnKeyUp(Avalonia.Input.KeyEventArgs e)
        {
            base.OnKeyUp(e);
            global::System.Windows.Input.KeyEventArgs args = WpfInputBridge.CreateKeyEvent(e, false);
            OnKeyUp(args);
            WpfInputBridge.CopyHandled(args, e);
        }

        private void OnSelectionChangedBridge(object? sender, global::Avalonia.Controls.SelectionChangedEventArgs e)
            => OnSelectionChanged(e);

        /// <summary>WPF-style selection-changed hook. Override to react to selection changes.</summary>
        protected virtual void OnSelectionChanged(global::Avalonia.Controls.SelectionChangedEventArgs e)
        {
        }

        /// <summary>
        /// WPF-style container factory. Override to return a new container for an item.
        /// Return <c>null</c> to defer to Avalonia's default container creation.
        /// </summary>
        protected virtual DependencyObject? GetContainerForItemOverride() => null;

        /// <summary>WPF-style test for whether an item is already its own container.</summary>
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

        /// <summary>WPF-style value accessor. Shadows Avalonia's <c>GetValue(AvaloniaProperty)</c>.</summary>
        public object? GetValue(DependencyProperty property)
            => DependencyPropertyServices.GetValue(this, property);

        /// <summary>WPF-style value setter for a <see cref="DependencyProperty"/>.</summary>
        public void SetValue(DependencyProperty property, object? value)
            => DependencyPropertyServices.SetValue(this, property, value);

        /// <summary>WPF-style value setter for a read-only <see cref="DependencyPropertyKey"/>.</summary>
        public void SetValue(DependencyPropertyKey key, object? value)
            => DependencyPropertyServices.SetValue(this, key, value);

        /// <summary>WPF-style local-value setter (no coercion re-entry).</summary>
        public void SetCurrentValue(DependencyProperty property, object? value)
            => DependencyPropertyServices.SetCurrentValue(this, property, value);

        /// <summary>WPF-style value clear.</summary>
        public void ClearValue(DependencyProperty property)
            => DependencyPropertyServices.ClearValue(this, property);

        /// <inheritdoc />
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            DependencyPropertyServices.OnPropertyChanged(this, change);
        }
    }

    /// <summary>
    /// WPF-compatible <see cref="MultiSelector"/> exposing the multi-selection surface used by
    /// <c>NodifyEditor</c>/<c>ConnectionsMultiSelector</c>.
    /// </summary>
    public class MultiSelector : Selector
    {
        /// <summary>Gets the list of currently-selected items (WPF <c>MultiSelector.SelectedItems</c>).</summary>
        public new IList SelectedItems => base.SelectedItems;

        /// <summary>Selects all current items.</summary>
        public void SelectAll()
        {
            foreach (object? item in Items)
            {
                if (item != null && !SelectedItems.Contains(item))
                {
                    SelectedItems.Add(item);
                }
            }
        }

        /// <summary>Clears the current selection.</summary>
        public void UnselectAll() => SelectedItems.Clear();

        /// <summary>Gets or sets whether multiple items can be selected.</summary>
        public bool CanSelectMultipleItems
        {
            get => SelectionMode.HasFlag(AvSelectionMode.Multiple);
            set => SelectionMode = value
                ? SelectionMode | AvSelectionMode.Multiple
                : SelectionMode & ~AvSelectionMode.Multiple;
        }

        /// <summary>
        /// WPF batch-update marker. Avalonia updates selection eagerly, so this is a no-op today;
        /// it exists so upstream's <c>BeginUpdateSelectedItems()</c>/<c>EndUpdateSelectedItems()</c>
        /// bracketing compiles and behaves (selection still applies, just not batched).
        /// </summary>
        protected void BeginUpdateSelectedItems()
        {
        }

        /// <summary>WPF batch-update marker (no-op; see <see cref="BeginUpdateSelectedItems"/>).</summary>
        protected void EndUpdateSelectedItems()
        {
        }
    }
}
