using Nodify.Interactivity;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Nodify
{
    public class ConnectionsMultiSelector : SelectingItemsControl, IKeyboardNavigationLayer
    {
        #region Dependency Properties

        public static readonly StyledProperty<IList?> SelectedItemsProperty =
            NodifyEditor.SelectedItemsProperty.AddOwner<ConnectionsMultiSelector>();

        public static readonly StyledProperty<bool> CanSelectMultipleItemsProperty =
            NodifyEditor.CanSelectMultipleItemsProperty.AddOwner<ConnectionsMultiSelector>();

        /// <summary>
        /// Gets or sets the selected connections in the <see cref="NodifyEditor"/>.
        /// </summary>
        public new IList? SelectedItems
        {
            get => (IList?)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        /// <summary>
        /// Gets or sets whether multiple connections can be selected.
        /// </summary>
        public new bool CanSelectMultipleItems
        {
            get => (bool)GetValue(CanSelectMultipleItemsProperty);
            set => SetValue(CanSelectMultipleItemsProperty, value);
        }

        private bool CanSelectMultipleItemsBase
        {
            get => base.SelectionMode == SelectionMode.Multiple || base.SelectionMode == SelectionMode.Toggle;
            set => base.SelectionMode = value ? SelectionMode.Multiple : SelectionMode.Single;
        }

        #endregion

        /// <summary>
        /// Gets the <see cref="NodifyEditor"/> that owns this <see cref="ConnectionsMultiSelector"/>.
        /// </summary>
        public NodifyEditor? Editor { get; private set; }

        /// <summary>
        /// Gets a list of all <see cref="ConnectionContainer"/>s.
        /// </summary>
        /// <remarks>Cache the result before using it to avoid extra allocations.</remarks>
        protected internal IReadOnlyCollection<ConnectionContainer> ConnectionContainers
        {
            get
            {
                ItemCollection items = Items;
                var containers = new List<ConnectionContainer>(items.Count);

                for (var i = 0; i < items.Count; i++)
                {
                    containers.Add((ConnectionContainer)ItemContainerGenerator.ContainerFromIndex(i));
                }

                return containers;
            }
        }

        static ConnectionsMultiSelector()
        {
            // Property change handlers
            CanSelectMultipleItemsProperty.Changed.AddClassHandler<ConnectionsMultiSelector>((selector, e) =>
                selector.CanSelectMultipleItemsBase = (bool)e.NewValue!);

            SelectedItemsProperty.Changed.AddClassHandler<ConnectionsMultiSelector>((selector, e) =>
                selector.OnSelectedItemsSourceChanged(e.OldValue, e.NewValue));

            // Metadata overrides
            FocusableProperty.OverrideMetadata(typeof(ConnectionsMultiSelector), new StyledPropertyMetadata<bool>(false));

            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ConnectionsMultiSelector), new StyledPropertyMetadata<KeyboardNavigationMode>(KeyboardNavigationMode.None));
        }

        public ConnectionsMultiSelector()
        {
            _focusNavigator = new StatefulFocusNavigator<ConnectionContainer>(OnElementFocused);

            // Subscribe to SelectionChanged event instead of overriding OnSelectionChanged
            SelectionChanged += OnSelectionChangedHandler;
        }

        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
            => new ConnectionContainer(this);

        protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        {
            recycleKey = null;
            return item is not ConnectionContainer;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            Editor = this.GetParentOfType<NodifyEditor>();

            if (NodifyEditor.AutoRegisterConnectionsLayer)
            {
                Editor?.RegisterNavigationLayer(this);
            }
        }

        #region Keyboard Navigation

        public KeyboardNavigationLayerId Id { get; } = KeyboardNavigationLayerId.Connections;
        public IKeyboardFocusTarget<Control>? LastFocusedElement => _focusNavigator.LastFocusedElement;

        private readonly StatefulFocusNavigator<ConnectionContainer> _focusNavigator;

        public bool TryMoveFocus(TraversalRequest request)
        {
            return _focusNavigator.TryMoveFocus(request, TryFindContainerToFocus);
        }

        public bool TryRestoreFocus()
        {
            return _focusNavigator.TryRestoreFocus();
        }

        private bool TryFindContainerToFocus(ConnectionContainer? currentElement, TraversalRequest request, out ConnectionContainer? containerToFocus)
        {
            containerToFocus = null;

            if (currentElement is ConnectionContainer focusedContainer)
            {
                containerToFocus = FindNextFocusTarget(focusedContainer, request);
            }
            else if (currentElement is Control elem && elem.GetParentOfType<ConnectionContainer>() is ConnectionContainer parentContainer)
            {
                containerToFocus = parentContainer;
            }
            else if (Items.Count > 0 && Editor != null)
            {
                var viewport = new Rect(Editor.ViewportLocation, Editor.ViewportSize);
                var containers = ConnectionContainers;
                containerToFocus = containers.FirstOrDefault(container => viewport.Intersects(((IKeyboardFocusTarget<ConnectionContainer>)container).Bounds))
                    ?? containers.First();
            }

            return containerToFocus != null;
        }

        protected virtual ConnectionContainer? FindNextFocusTarget(ConnectionContainer currentContainer, TraversalRequest request)
        {
            var focusNavigator = new DirectionalFocusNavigator<ConnectionContainer>(ConnectionContainers);
            var result = focusNavigator.FindNextFocusTarget(currentContainer, request);

            return result?.Element;
        }

        protected virtual void OnElementFocused(IKeyboardFocusTarget<ConnectionContainer> target)
        {
            if (NodifyEditor.AutoPanOnNodeFocus)
            {
                Editor?.BringIntoView(target.Bounds, NodifyEditor.BringIntoViewEdgeOffset);
            }
        }

        void IKeyboardNavigationLayer.OnActivated()
        {
            TryRestoreFocus();
        }

        void IKeyboardNavigationLayer.OnDeactivated()
        {
        }

        #endregion

        public void Select(ConnectionContainer container)
        {
            // In Avalonia, we don't need Begin/EndUpdateSelectedItems
            var selected = base.SelectedItems;
            selected.Clear();
            selected.Add(container.DataContext);

#if NETCOREAPP3_0_OR_GREATER
            // For some reason the ConnectionContainer.IsSelected property change is not triggered, which prevents the visual update of the child connection.
            // To address this, we manually set the IsSelected property before it is automatically set to true.
            // Note: This approach will cause bindings to update out of order.
            // It is recommended to handle undo/redo operations using the SelectionChanged event in this case.
            container.IsSelected = true;
#endif

            Editor?.UnselectAll();
        }

        #region Selection Handlers

        private void OnSelectedItemsSourceChanged(object? oldValue, object? newValue)
        {
            if (oldValue is INotifyCollectionChanged oc)
            {
                oc.CollectionChanged -= OnSelectedItemsChanged;
            }

            if (newValue is INotifyCollectionChanged nc)
            {
                nc.CollectionChanged += OnSelectedItemsChanged;
            }

            IList selectedItems = base.SelectedItems;

            // In Avalonia, we don't need Begin/EndUpdateSelectedItems
            selectedItems.Clear();
            if (newValue is IList newList)
            {
                for (var i = 0; i < newList.Count; i++)
                {
                    selectedItems.Add(newList[i]);
                }
            }
        }

        private void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (!CanSelectMultipleItems)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    base.SelectedItems.Clear();
                    break;

                case NotifyCollectionChangedAction.Add:
                    IList? newItems = e.NewItems;
                    if (newItems != null)
                    {
                        IList selectedItems = base.SelectedItems;
                        for (var i = 0; i < newItems.Count; i++)
                        {
                            selectedItems.Add(newItems[i]);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    IList? oldItems = e.OldItems;
                    if (oldItems != null)
                    {
                        IList selectedItems = base.SelectedItems;
                        for (var i = 0; i < oldItems.Count; i++)
                        {
                            selectedItems.Remove(oldItems[i]);
                        }
                    }
                    break;
            }
        }

        private void OnSelectionChangedHandler(object? sender, SelectionChangedEventArgs e)
        {
            IList? selected = SelectedItems;
            if (selected != null)
            {
                IList added = e.AddedItems;
                for (var i = 0; i < added.Count; i++)
                {
                    // Ensure no duplicates are added
                    if (!selected.Contains(added[i]))
                    {
                        selected.Add(added[i]);
                    }
                }

                IList removed = e.RemovedItems;
                for (var i = 0; i < removed.Count; i++)
                {
                    selected.Remove(removed[i]);
                }
            }
        }

        #endregion
    }
}
