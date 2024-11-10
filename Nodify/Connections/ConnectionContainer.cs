using System.Windows.Input;

#if Avalonia
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.Presenters;
using Avalonia.Interactivity;
using Nodify.Avalonia.Helpers;
using Nodify.Avalonia.Extensions;
using RoutedEventHandler = System.EventHandler<Avalonia.Interactivity.RoutedEvent>;
using UIElement = Avalonia.Controls.Control;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif

namespace Nodify
{
    internal class ConnectionContainer : ContentPresenter
    {
        #region Dependency properties

#if Avalonia
        public static readonly AttachedProperty<bool> IsSelectableProperty = AvaloniaProperty.RegisterAttached<ConnectionContainer, Control, bool>("IsSelectable", BoxValue.False);
        public static readonly AttachedProperty<bool> IsSelectedProperty = AvaloniaProperty.RegisterAttached<ConnectionContainer, Control, bool>("IsSelected", BoxValue.False);
#else
        public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.Register(nameof(IsSelectable), typeof(bool), typeof(ConnectionContainer), new FrameworkPropertyMetadata(BoxValue.False));
        public static readonly DependencyProperty IsSelectedProperty = System.Windows.Controls.Primitives.Selector.IsSelectedProperty.AddOwner(typeof(ConnectionContainer), new FrameworkPropertyMetadata(BoxValue.False, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsSelectedChanged));
#endif

#if Avalonia
        private static void OnIsSelectedChanged(object d, AvaloniaPropertyChangedEventArgs<bool> e)
#else
        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
#endif
        {
            var elem = (ConnectionContainer)d;
#if Avalonia
            bool result = elem.IsSelectable && (bool)e.NewValue.Value;
#else
            bool result = elem.IsSelectable && (bool)e.NewValue;
#endif
            elem.IsSelected = result;
            elem.OnSelectedChanged(result);
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
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

#endregion

        #region Routed events

#if Avalonia
        public static readonly RoutedEvent SelectedEvent = RoutedEvent.Register<Connector, RoutedEventArgs>(nameof(Selected), RoutingStrategies.Bubble);
        public static readonly RoutedEvent UnselectedEvent = RoutedEvent.Register<Connector, RoutedEventArgs>(nameof(Unselected), RoutingStrategies.Bubble);
#else
        public static readonly RoutedEvent SelectedEvent = System.Windows.Controls.Primitives.Selector.SelectedEvent.AddOwner(typeof(ConnectionContainer));
        public static readonly RoutedEvent UnselectedEvent = System.Windows.Controls.Primitives.Selector.UnselectedEvent.AddOwner(typeof(ConnectionContainer));
#endif

        /// <summary>
        /// Occurs when this <see cref="ConnectionContainer"/> is selected.
        /// </summary>
        public event RoutedEventHandler Selected
        {
            add => AddHandler(SelectedEvent, value);
            remove => RemoveHandler(SelectedEvent, value);
        }

        /// <summary>
        /// Occurs when this <see cref="ConnectionContainer"/> is unselected.
        /// </summary>
        public event RoutedEventHandler Unselected
        {
            add => AddHandler(UnselectedEvent, value);
            remove => RemoveHandler(UnselectedEvent, value);
        }

#endregion

        private ConnectionsMultiSelector Selector { get; }

        private UIElement? _connection;
        private UIElement? Connection => _connection ??= BaseConnection.PrioritizeBaseConnectionForSelection
            ? this.GetChildOfType<BaseConnection>() ?? this.GetChildOfType<UIElement>()
            : this.GetChildOfType<UIElement>();

        internal ConnectionContainer(ConnectionsMultiSelector selector)
        {
            Selector = selector;
        }

        /// <summary>
        /// Raises the <see cref="SelectedEvent"/> or <see cref="UnselectedEvent"/> based on <paramref name="newValue"/>.
        /// Called when the <see cref="IsSelected"/> value is changed.
        /// </summary>
        /// <param name="newValue">True if selected, false otherwise.</param>
        protected void OnSelectedChanged(bool newValue)
        {
            BaseConnection.SetIsSelected(Connection, newValue);

            RaiseEvent(new RoutedEventArgs(newValue ? SelectedEvent : UnselectedEvent, this));
        }

#if Avalonia
        protected override void OnPointerPressed(PointerPressedEventArgs e)
#else
        protected override void OnMouseDown(MouseButtonEventArgs e)
#endif
        {
            if (IsSelectable && EditorGestures.Mappings.Connection.Selection.Select.Matches(e.Source, e))
            {
                e.Handled = true;
            }
        }

#if Avalonia
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
#else
        protected override void OnMouseUp(MouseButtonEventArgs e)
#endif
        {
            EditorGestures.ConnectionGestures gestures = EditorGestures.Mappings.Connection;
            if (gestures.Selection.Select.Matches(e.Source, e))
            {
                if (gestures.Selection.Append.Matches(e.Source, e))
                {
                    IsSelected = true;
                }
                else if (gestures.Selection.Invert.Matches(e.Source, e))
                {
                    IsSelected = !IsSelected;
                }
                else if (gestures.Selection.Remove.Matches(e.Source, e))
                {
                    IsSelected = false;
                }
                else
                {
                    // Allow context menu on selection
#if Avalonia
                    if (e.GetPointerUpdateKind() != PointerUpdateKind.RightButtonReleased || !IsSelected)
#else
                    if (!(e.ChangedButton == MouseButton.Right && e.RightButton == MouseButtonState.Released) || !IsSelected)
#endif
                    {
                        Selector.UnselectAll();
                    }

                    IsSelected = true;
                }
            }
        }
    }
}
