using Nodify.Interactivity;
using System;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Media;

namespace Nodify
{
    public class ConnectionContainer : ContentPresenter, IKeyboardFocusTarget<ConnectionContainer>
    {
        #region Avalonia properties

        public static readonly StyledProperty<bool> IsSelectableProperty =
            AvaloniaProperty.Register<ConnectionContainer, bool>(nameof(IsSelectable), defaultValue: false);

        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<ConnectionContainer, bool>(nameof(IsSelected), defaultValue: false, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

        private static void OnIsSelectedChanged(ConnectionContainer elem, AvaloniaPropertyChangedEventArgs e)
        {
            bool result = elem.IsSelectable && (bool)e.NewValue!;
            elem.IsSelected = result;
            elem.OnSelectedChanged(result);
        }

        static ConnectionContainer()
        {
            IsSelectedProperty.Changed.AddClassHandler<ConnectionContainer>((x, e) => OnIsSelectedChanged(x, e));
        }

        /// <summary>
        /// Gets or sets whether this <see cref="ConnectionContainer"/> can be selected.
        /// </summary>
        public bool IsSelectable
        {
            get => BaseConnection.GetIsSelectable(Connection ?? this);
            set => BaseConnection.SetIsSelectable(Connection ?? this, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether this <see cref="ConnectionContainer"/> is selected.
        /// Can only be set if <see cref="IsSelectable"/> is true.
        /// </summary>
        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        #endregion

        #region Routed events

        public static readonly RoutedEvent<RoutedEventArgs> SelectedEvent =
            RoutedEvent.Register<ConnectionContainer, RoutedEventArgs>(nameof(Selected), Avalonia.Interactivity.RoutingStrategies.Bubble);

        public static readonly RoutedEvent<RoutedEventArgs> UnselectedEvent =
            RoutedEvent.Register<ConnectionContainer, RoutedEventArgs>(nameof(Unselected), Avalonia.Interactivity.RoutingStrategies.Bubble);

        /// <summary>
        /// Occurs when this <see cref="ConnectionContainer"/> is selected.
        /// </summary>
        public event EventHandler<RoutedEventArgs> Selected
        {
            add => AddHandler(SelectedEvent, value);
            remove => RemoveHandler(SelectedEvent, value);
        }

        /// <summary>
        /// Occurs when this <see cref="ConnectionContainer"/> is unselected.
        /// </summary>
        public event EventHandler<RoutedEventArgs> Unselected
        {
            add => AddHandler(UnselectedEvent, value);
            remove => RemoveHandler(UnselectedEvent, value);
        }

        #endregion

        private Control? _connection;
        private SelectionType? _selectionType;

        public Rect Bounds => ConnectionFocusTarget.Bounds;
        ConnectionContainer IKeyboardFocusTarget<ConnectionContainer>.Element => this;

        private IKeyboardFocusTarget<Visual> ConnectionFocusTarget => Connection as IKeyboardFocusTarget<Visual>
            ?? throw new NotSupportedException($"Custom connections must implement {nameof(IKeyboardFocusTarget<Visual>)} for keyboard navigation. Or disable keyboard navigation for the connections layer.");

        public Control? Connection => _connection ??= BaseConnection.PrioritizeBaseConnectionForSelection
            ? this.GetChildOfType<BaseConnection>() ?? this.GetChildOfType<Control>()
            : this.GetChildOfType<Control>();

        public ConnectionsMultiSelector Selector { get; }

        public ConnectionContainer(ConnectionsMultiSelector selector)
        {
            Selector = selector;
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            if (IsKeyboardFocusWithin)
            {
                Selector.Editor?.Focus();
            }
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            if (Connection is BaseConnection baseConnection)
            {
                baseConnection.UpdateFocusVisual();
            }
            else
            {
                Connection?.InvalidateVisual();
            }
        }

        /// <summary>
        /// Raises the <see cref="SelectedEvent"/> or <see cref="UnselectedEvent"/> based on <paramref name="newValue"/>.
        /// Called when the <see cref="IsSelected"/> value is changed.
        /// </summary>
        /// <param name="newValue">True if selected, false otherwise.</param>
        private void OnSelectedChanged(bool newValue)
        {
            BaseConnection.SetIsSelected(Connection, newValue);

            RaiseEvent(new RoutedEventArgs(newValue ? SelectedEvent : UnselectedEvent, this));
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            EditorGestures.ConnectionGestures gestures = EditorGestures.Mappings.Connection;
            if (IsSelectable && gestures.Selection.Select.Matches(e.Source, e))
            {
                _selectionType = gestures.Selection.GetSelectionType(e);
            }
            // Replaces the current selection when right-clicking on an element that has a context menu and is not selected.
            // Applies only when the select gesture is not right click.
            else if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed && Connection?.ContextMenu != null)
            {
                _selectionType = IsSelected ? SelectionType.Append : SelectionType.Replace;
            }

            if (_selectionType.HasValue)
            {
                Focus();
                e.Handled = true;
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (_selectionType.HasValue)
            {
                // Determine whether the current selection should remain intact or be replaced by the clicked item. 
                // If the right mouse button is pressed on an already selected item, and the item either has an 
                // explicit context menu, the selection remains unchanged.
                // This ensures that the context menu applies to the entire selection rather than only the clicked item.
                var pointerPoint = e.GetCurrentPoint(this);
                bool allowContextMenu = pointerPoint.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonReleased && IsSelected && Connection?.ContextMenu != null;
                if (!allowContextMenu)
                {
                    Select(_selectionType.Value);
                }

                _selectionType = null;
            }
        }

        /// <summary>
        /// Modifies the selection state of the current item based on the specified selection type.
        /// </summary>
        /// <param name="type">The type of selection to perform.</param>
        public void Select(SelectionType type)
        {
            switch (type)
            {
                case SelectionType.Append:
                    IsSelected = true;
                    break;
                case SelectionType.Remove:
                    IsSelected = false;
                    break;
                case SelectionType.Invert:
                    IsSelected = !IsSelected;
                    break;
                case SelectionType.Replace:
                    Selector.Select(this);
                    break;
            }
        }
    }
}
